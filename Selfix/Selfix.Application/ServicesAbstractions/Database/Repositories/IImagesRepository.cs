using LanguageExt;
using Selfix.Domain.Entities.Images;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IImagesRepository
{
    OptionT<IO, Image> GetById(Id<Image, Ulid> id, CancellationToken cancellationToken);
    IO<Unit> Save(Image image, CancellationToken cancellationToken);
}