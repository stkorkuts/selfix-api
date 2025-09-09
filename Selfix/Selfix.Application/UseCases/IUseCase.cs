using LanguageExt;

namespace Selfix.Application.UseCases;

public interface IUseCase<in TRequest, TResponse>
{
    public IO<TResponse> Execute(TRequest request, CancellationToken cancellationToken);
}