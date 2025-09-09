using LanguageExt;

namespace Selfix.Shared.Types;

public sealed record Page<T>(Iterable<T> Items, uint Total, uint Skipped);