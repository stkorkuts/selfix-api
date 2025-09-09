using LanguageExt;

namespace Selfix.Shared.Extensions;

public static class TaskExtensions
{
    public static IO<T> ToIO<T>(this Task<T> task)
    {
        return IO<T>.LiftAsync(() => task);
    }

    public static IO<Unit> ToIO(this Task task)
    {
        return IO<Unit>.LiftAsync(task.ToUnit);
    }

    public static IO<Unit> ToIOUnit<T>(this Task<T> task)
    {
        return IO<T>.LiftAsync(() => task).Map(_ => Unit.Default);
    }
}