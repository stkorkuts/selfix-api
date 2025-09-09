using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Selfix.Infrastructure.Database.Converters;

internal sealed class UlidToStringConverter : ValueConverter<Ulid, string>
{
    private static readonly ConverterMappingHints DEFAULT_HINTS = new(26);

    public UlidToStringConverter() : this(null)
    {
    }

    public UlidToStringConverter(ConverterMappingHints? mappingHints = null)
        : base(
            x => x.ToString(),
            x => Ulid.Parse(x, CultureInfo.InvariantCulture),
            DEFAULT_HINTS.With(mappingHints))
    {
    }
}