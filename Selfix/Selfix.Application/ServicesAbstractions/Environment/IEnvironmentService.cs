using LanguageExt;

namespace Selfix.Application.ServicesAbstractions.Environment;

public interface IEnvironmentService
{
    IO<DateTimeOffset> GetCurrentTime(CancellationToken cancellationToken);
    IO<long> GenerateSeed(CancellationToken cancellationToken);
}