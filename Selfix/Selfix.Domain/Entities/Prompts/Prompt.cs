using LanguageExt;
using Selfix.Domain.Entities.Prompts.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Prompts;

namespace Selfix.Domain.Entities.Prompts;

public sealed class Prompt
{
    private Prompt(Id<Prompt, Ulid> id, PromptName name, NaturalNumber numberInOrder, PromptText text)
    {
        Id = id;
        Name = name;
        NumberInOrder = numberInOrder;
        Text = text;
    }

    public Id<Prompt, Ulid> Id { get; private set; }
    public PromptName Name { get; private set; }
    public NaturalNumber NumberInOrder { get; private set; }
    public PromptText Text { get; private set; }

    public static Fin<Prompt> New(NewPromptSpecification specs)
    {
        var id = Id<Prompt, Ulid>.FromSafe(Ulid.NewUlid());
        return new Prompt(id, specs.Name, specs.NumberInOrder, specs.Text);
    }

    public static Fin<Prompt> Restore(RestorePromptSpecification specs)
    {
        return new Prompt(specs.Id, specs.Name, specs.NumberInOrder, specs.Data);
    }
}