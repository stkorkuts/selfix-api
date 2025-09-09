using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Shared.Types;

namespace Selfix.Application.UseCases.Telegram.Prompts.Get;

internal sealed class GetPromptsUseCase : IGetPromptsUseCase
{
    private readonly IPromptsRepository _promptsRepository;

    public GetPromptsUseCase(CachedPromptsRepository promptsRepository)
    {
        _promptsRepository = promptsRepository;
    }

    public IO<GetPromptsResponse> Execute(GetPromptsRequest request,
        CancellationToken cancellationToken)
    {
        var skip = request.PageIndex * request.PageSize;
        var take = request.PageSize;
        return
            from prompts in _promptsRepository.Get(new PromptsFilter(skip, take), cancellationToken)
            from count in _promptsRepository.Count(cancellationToken)
            select new GetPromptsResponse(new Page<PromptDto>(prompts.Map(p => p.ToDto()), count, skip));
    }
}