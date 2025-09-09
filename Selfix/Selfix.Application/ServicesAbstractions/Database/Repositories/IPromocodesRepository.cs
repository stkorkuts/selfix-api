using LanguageExt;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Promocodes;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IPromocodesRepository
{
    OptionT<IO, Promocode> GetById(Id<Promocode, Ulid> id, CancellationToken cancellationToken);
    OptionT<IO, Promocode> GetByCode(AlphanumericString code, CancellationToken cancellationToken);
    IO<Unit> Save(Promocode promocode, CancellationToken cancellationToken);
}