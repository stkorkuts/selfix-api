using LanguageExt;
using Selfix.Application.ServicesAbstractions.Environment;

namespace Selfix.Infrastructure.Environment;

internal sealed class EnvironmentService : IEnvironmentService
{
    private readonly IO<DateTimeOffset> _currentTimeIO;
    private readonly IO<long> _seedIO;

    public EnvironmentService()
    {
        _currentTimeIO = IO<DateTimeOffset>.Lift(() => DateTimeOffset.UtcNow);
        _seedIO = IO<long>.Lift(() => DateTimeOffset.UtcNow.Ticks);
    }

    public IO<DateTimeOffset> GetCurrentTime(CancellationToken cancellationToken)
    {
        return _currentTimeIO;
    }

    public IO<long> GenerateSeed(CancellationToken cancellationToken)
    {
        return _seedIO;
    }
}