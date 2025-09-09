using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Prompts;
using Selfix.Domain.Entities.Prompts.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Prompts;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared.Extensions;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class PromptsRepository : IPromptsRepository
{
    private readonly SelfixDbContext _context;

    public PromptsRepository(SelfixDbContext context)
    {
        _context = context;
    }

    public OptionT<IO, Prompt> GetById(Id<Prompt, Ulid> id, CancellationToken cancellationToken)
    {
        return
            from promptDb in OptionT<IO, PromptDb>.LiftIO(
                IO<Option<PromptDb>>.LiftAsync(() => _context.Prompts
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                    .Map(Prelude.Optional)))
            from prompt in FromDb(promptDb).ToIO()
            select prompt;
    }

    public IO<Iterable<Prompt>> Get(PromptsFilter filter, CancellationToken cancellationToken)
    {
        return
            from promptDbs in IO<List<PromptDb>>.LiftAsync(() => 
                _context.Prompts
                    .Skip((int)filter.Skip)
                    .Take((int)filter.Take)
                    .ToListAsync(cancellationToken))
            from prompts in promptDbs.AsIterable().Traverse(FromDb).As().ToIO()
            select prompts;
    }

    public IO<uint> Count(CancellationToken cancellationToken) =>
        IO<uint>.LiftAsync(() => _context.Prompts.CountAsync(cancellationToken).Map(c =>
        {
            checked
            {
                return (uint)c;
            }
        }));

    private static Fin<Prompt> FromDb(PromptDb promptDb)
    {
        var id = Id<Prompt, Ulid>.FromSafe(promptDb.Id);
        return
            from name in PromptName.From(promptDb.Name)
            from number in NaturalNumber.From((uint)promptDb.NumberInOrder)
            from text in PromptText.From(promptDb.Text)
            from prompt in Prompt.Restore(new RestorePromptSpecification(id, name, number, text))
            select prompt;
    }
}