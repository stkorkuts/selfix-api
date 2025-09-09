using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Selfix.Infrastructure.Database.Converters;

internal sealed class UlidToBytesConverter : ValueConverter<Ulid, byte[]>
{
    private static readonly ConverterMappingHints DEFAULT_HINTS = new(16);

    public UlidToBytesConverter() : this(null)
    {
    }

    public UlidToBytesConverter(ConverterMappingHints? mappingHints = null)
        : base(
            x => x.ToByteArray(),
            x => new Ulid(x),
            DEFAULT_HINTS.With(mappingHints))
    {
    }
}