namespace Atc.Cosmos.Functions.Testing.Internal;

/// <summary>
/// The context passed to an <see cref="IInputConverter"/> during verification.
/// </summary>
/// <remarks>
/// There is no function invocation, so <see cref="FunctionContext"/> is <see langword="null"/>.
/// Converters that only read <see cref="Source"/> and <see cref="TargetType"/> are supported.
/// </remarks>
internal sealed class VerifierConverterContext(
    Type targetType,
    object source)
    : ConverterContext
{
    public override Type TargetType { get; } = targetType;

    public override object? Source { get; } = source;

    public override FunctionContext FunctionContext => null!;

    public override IReadOnlyDictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}