using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Images;
using Selfix.Domain.Entities.Images.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Images;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Infrastructure.Database.Entities;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class ImagesRepository : IImagesRepository
{
    private readonly SelfixDbContext _context;

    public ImagesRepository(SelfixDbContext context)
    {
        _context = context;
    }
    
    public OptionT<IO, Image> GetById(Id<Image, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Option<Image>>.LiftAsync(async () =>
        {
            var imageDb = await _context.Images.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            return imageDb is null ? Option<Image>.None : FromDb(imageDb).ThrowIfFail();
        });
    }

    public IO<Unit> Save(Image image, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var existingImageDb = await _context.Images.AsTracking()
                .FirstOrDefaultAsync(i => i.Id == image.Id, cancellationToken);
            if (existingImageDb is not null)
            {
                existingImageDb.OSFilePath = image.Path;
            }
            else
            {
                var newImage = new ImageDb
                {
                    Id = image.Id,
                    UserId = image.UserId,
                    OSFilePath = image.Path,
                    CreatedAt = image.CreatedAt
                };
                _context.Add(newImage);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }
    
    private static Fin<Image> FromDb(ImageDb entity)
    {
        var id = Id<Image, Ulid>.FromSafe(entity.Id);
        var userId = Id<User, Ulid>.FromSafe(entity.UserId);
        return
            from path in OSFilePath.From(entity.OSFilePath)
            from image in Image.Restore(new RestoreImageSpecification(id, userId, path, entity.CreatedAt))
            select image;
    }
}