using System.Globalization;

namespace Selfix.Shared.Utils;

public static class UlidUtils
{
    public static Ulid NewWithSeed(int seed)
    {
        // Create deterministic random component
        var random = new Random(seed);
        var randomBytes = new byte[16]; // Ulid is 16 bytes total
        random.NextBytes(randomBytes);

        // Set the timestamp bits (first 6 bytes) to a fixed value if desired
        // This makes the created Ulid sortable by your seed value
        var timestamp = DateTimeOffset.Parse("2023-01-01T00:00:00Z", CultureInfo.InvariantCulture)
            .ToUnixTimeMilliseconds();
        var timestampBytes = BitConverter.GetBytes(timestamp);
        Array.Copy(timestampBytes, 0, randomBytes, 0, Math.Min(timestampBytes.Length, 6));

        return new Ulid(randomBytes);
    }
}