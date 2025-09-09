using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Jobs.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.AvatarCreation;
using Selfix.Domain.ValueObjects.Jobs.ImageGeneration;
using Selfix.Domain.ValueObjects.Jobs.PromptProcessing;
using Selfix.Shared;

namespace Selfix.Domain.Entities.Jobs;

public sealed class Job
{
    private Job(Id<Job, Ulid> id, JobStatus status, JobData data, Option<Notes> notes, DateTimeOffset createdAt,
        Id<User, Ulid> userId)
    {
        Id = id;
        Status = status;
        Data = data;
        Notes = notes;
        CreatedAt = createdAt;
        UserId = userId;
    }

    public Id<Job, Ulid> Id { get; private set; }
    public JobStatus Status { get; private set; }
    public JobData Data { get; private set; }
    public Option<Notes> Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Id<User, Ulid> UserId { get; private set; }

    public static Fin<Job> New(NewJobSpecification specs)
    {
        var id = Id<Job, Ulid>.FromSafe(Ulid.NewUlid());
        var status = new JobStatus(JobStatusEnum.Created, specs.CurrentTime);
        var notes = Option<Notes>.None;
        return new Job(id, status, specs.Data, notes, specs.CurrentTime, specs.UserId);
    }

    public static Fin<Job> Restore(RestoreJobSpecification specs)
    {
        return new Job(specs.Id, specs.Status, specs.Data, specs.Notes, specs.CreatedAt, specs.UserId);
    }

    public Fin<Unit> ChangeStatus(JobStatus newStatus)
    {
        if (Status.IsCompleted)
            return Error.New("Job is already completed.");
        if (newStatus.Value is JobStatusEnum.Created)
            return Error.New("You can not change status to created");

        Status = newStatus;
        return Unit.Default;
    }

    public Fin<Unit> AddNotes(Notes notes)
    {
        return Notes.Match(
            currentNotes =>
                from combinedNotes in ValueObjects.Common.Notes.From((string)currentNotes + "\n" + (string)notes)
                let _1 = Notes = combinedNotes
                select Unit.Default,
            () =>
                from _1 in Fin<Unit>.Succ(Unit.Default)
                let _2 = Notes = notes
                select Unit.Default
        );
    }

    public Fin<AvatarCreationJobData> SetOutput(AvatarCreationJobOutput output)
    {
        return Data switch
        {
            AvatarCreationJobData data => data.Output.Match(
                _ => Error.New("Output already set"),
                () =>
                    from _1 in Fin<Unit>.Succ(Unit.Default)
                    let newData = new AvatarCreationJobData(data.Input, output)
                    let _ = Data = newData
                    select newData),
            { } data => Error.New(
                $"Job has different type: {data.GetType().Name}. AvatarCreation output can not be set")
        };
    }

    public Fin<ImageGenerationJobData> SetOutput(ImageGenerationJobOutput output)
    {
        return Data switch
        {
            ImageGenerationJobData data => data.Output.Match(
                _ => Error.New("Output already set"),
                () =>
                    from _1 in Fin<Unit>.Succ(Unit.Default)
                    let newData = new ImageGenerationJobData(data.Input, output)
                    let _ = Data = newData
                    select newData),
            { } data => Error.New(
                $"Job has different type: {data.GetType().Name}. ImageGeneration output can not be set")
        };
    }

    public Fin<PromptProcessingJobData> SetOutput(PromptProcessingJobOutput output)
    {
        return Data switch
        {
            PromptProcessingJobData data => data.Output.Match(
                _ => Error.New("Output already set"),
                () =>
                    from _1 in Fin<Unit>.Succ(Unit.Default)
                    let newData = new PromptProcessingJobData(data.Input, output)
                    let _ = Data = newData
                    select newData),
            { } data => Error.New(
                $"Job has different type: {data.GetType().Name}. PromptProcessing output can not be set")
        };
    }
}