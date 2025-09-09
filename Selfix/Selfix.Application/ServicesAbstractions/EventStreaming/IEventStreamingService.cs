using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Jobs;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Domain.Entities.TelegramProfiles;

namespace Selfix.Application.ServicesAbstractions.EventStreaming;

public interface IEventStreamingService
{
    IO<Unit> SendJobForProcessing(JobDto job, CancellationToken cancellationToken);
}