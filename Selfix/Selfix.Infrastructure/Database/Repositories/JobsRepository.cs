using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.Entities.Jobs.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.AvatarCreation;
using Selfix.Domain.ValueObjects.Jobs.ImageGeneration;
using Selfix.Domain.ValueObjects.Jobs.PromptProcessing;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Domain.ValueObjects.Prompts;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Infrastructure.Database.JsonDocumentSchema.Jobs;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class JobsRepository : IJobsRepository
{
    private readonly SelfixDbContext _context;

    public JobsRepository(SelfixDbContext context)
    {
        _context = context;
    }

    public OptionT<IO, Job> GetById(Id<Job, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Option<Job>>.LiftAsync(async () =>
        {
            var jobDb = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
            return jobDb is null ? Option<Job>.None : FromDb(jobDb).ThrowIfFail();
        });
    }

    public IO<Unit> Save(Job job, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var (input, output) = ConvertJobDataToJsonDocuments(job.Data);
            var notes = job.Notes.Match<string?>(val => val, () => null);
            var existingJob = await _context.Jobs.AsTracking()
                .FirstOrDefaultAsync(j => j.Id == job.Id, cancellationToken);
            if (existingJob is not null)
            {
                existingJob.Status = job.Status.Value;
                existingJob.UpdatedAt = job.Status.UpdatedAt;
                existingJob.Output = output;
                existingJob.Notes = notes;
            }
            else
            {
                var newJob = new JobDb
                {
                    Id = job.Id,
                    UserId = job.UserId,
                    Type = job.Data switch
                    {
                        AvatarCreationJobData => JobTypeEnum.AvatarCreation,
                        PromptProcessingJobData => JobTypeEnum.PromptProcessing,
                        ImageGenerationJobData => JobTypeEnum.ImageGeneration,
                        _ => throw new Exception("Unsupported job data type")
                    },
                    Input = input,
                    Output = output,
                    Status = job.Status.Value,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.Status.UpdatedAt,
                    Notes = notes
                };
                _context.Add(newJob);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }

    private static (JsonDocument Input, JsonDocument? Output) ConvertJobDataToJsonDocuments(JobData jobData)
    {
        return jobData switch
        {
            AvatarCreationJobData data => (
                JsonSerializer.SerializeToDocument(new AvatarCreationJobInputJsonDocumentSchema(
                    data.Input.ImagePaths.Map(path => (string)path).ToArray(),
                    data.Input.Name), DatabaseConfiguration.JSON_SERIALIZER_OPTIONS),
                data.Output.Match<JsonDocument?>(
                    _ => JsonSerializer.SerializeToDocument(new AvatarCreationJobOutputJsonDocumentSchema(), DatabaseConfiguration.JSON_SERIALIZER_OPTIONS), 
                    () => null)),
            PromptProcessingJobData data => (
                JsonSerializer.SerializeToDocument(new PromptProcessingJobInputJsonDocumentSchema(
                    data.Input.AvatarDescription,
                    data.Input.RawPrompt,
                    data.Input.Quantity,
                    data.Input.Seed), DatabaseConfiguration.JSON_SERIALIZER_OPTIONS),
                data.Output.Match<JsonDocument?>(
                    val => JsonSerializer.SerializeToDocument(new PromptProcessingJobOutputJsonDocumentSchema(val.ProcessedPrompt), DatabaseConfiguration.JSON_SERIALIZER_OPTIONS), 
                    () => null)),
            ImageGenerationJobData data => (
                JsonSerializer.SerializeToDocument(new ImageGenerationJobInputJsonDocumentSchema(
                    data.Input.AvatarLoraPath,
                    data.Input.Prompt,
                    data.Input.Quantity,
                    data.Input.Seed,
                    data.Input.AspectRatio), DatabaseConfiguration.JSON_SERIALIZER_OPTIONS),
                data.Output.Match<JsonDocument?>(
                    _ => JsonSerializer.SerializeToDocument(new ImageGenerationJobOutputJsonDocumentSchema(), DatabaseConfiguration.JSON_SERIALIZER_OPTIONS),
                    () => null)),
            _ => throw new Exception("Unsupported job data type")
        };
    }

    private static Fin<Job> FromDb(JobDb entity)
    {
        var id = Id<Job, Ulid>.FromSafe(entity.Id);
        var userId = Id<User, Ulid>.FromSafe(entity.UserId);
        return
            from data in ParseJobData(entity.Type, entity.Input, entity.Output)
            let status = new JobStatus(entity.Status, entity.UpdatedAt)
            from notes in string.IsNullOrWhiteSpace(entity.Notes)
                ? Option<Notes>.None
                : Notes.From(entity.Notes).Map(Option<Notes>.Some)
            from job in Job.Restore(new RestoreJobSpecification(id, userId, status, data, notes, entity.CreatedAt))
            select job;
    }

    private static Fin<JobData> ParseJobData(JobTypeEnum type, JsonDocument input, JsonDocument? output)
    {
        try
        {
            return type switch
            {
                JobTypeEnum.AvatarCreation => ParseAvatarCreationJobData(input, output),
                JobTypeEnum.PromptProcessing => ParsePromptProcessingJobData(input, output),
                JobTypeEnum.ImageGeneration => ParseImageGenerationJobData(input, output),
                _ => Error.New("Unsupported job type")
            };
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }

    private static Fin<JobData> ParseAvatarCreationJobData(JsonDocument input, JsonDocument? output)
    {
        var inputSchema = input.Deserialize<AvatarCreationJobInputJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS)!;
        var jobOutput = Prelude.Optional(output?.Deserialize<AvatarCreationJobOutputJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS))
            .Map(_ => new AvatarCreationJobOutput());

        return
            (from imageLocations in inputSchema.ImagePaths.AsIterable().Traverse(OSFilePath.From)
                from avatarName in AvatarName.From(inputSchema.AvatarName)
                select new AvatarCreationJobData(new AvatarCreationJobInput(imageLocations, avatarName), jobOutput) as JobData)
            .As();
    }

    private static Fin<JobData> ParsePromptProcessingJobData(JsonDocument input, JsonDocument? output)
    {
        var inputSchema = input.Deserialize<PromptProcessingJobInputJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS)!;
        var outputFin = Prelude.Optional(output?.Deserialize<PromptProcessingJobOutputJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS))
            .Traverse(val => 
                from prompt in PromptText.From(val.ProcessedPrompt)
                select new PromptProcessingJobOutput(prompt)
            ).As();

        return
            (from avatarDescription in AvatarDescription.From(inputSchema.AvatarDescription)
                from rawPrompt in PromptText.From(inputSchema.RawPrompt)
                from quantity in NaturalNumber.From(inputSchema.Quantity)
                from jobOutput in outputFin
                select new PromptProcessingJobData(
                    new PromptProcessingJobInput(avatarDescription, rawPrompt, quantity, inputSchema.Seed), jobOutput) as JobData)
            .As();
    }

    private static Fin<JobData> ParseImageGenerationJobData(JsonDocument input, JsonDocument? output)
    {
        var inputSchema = input.Deserialize<ImageGenerationJobInputJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS)!;
        var jobOutput = Prelude.Optional(output?.Deserialize<ImageGenerationJobOutputJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS))
            .Map(_ => new ImageGenerationJobOutput());

        return
            (
                from loraPath in OSFilePath.From(inputSchema.AvatarLoraPath)
                from prompt in PromptText.From(inputSchema.Prompt)
                from quantity in NaturalNumber.From(inputSchema.Quantity)
                select new ImageGenerationJobData(new ImageGenerationJobInput(loraPath, prompt, quantity, inputSchema.Seed, inputSchema.ImageAspectRatio), jobOutput) as JobData)
            .As();
    }
}