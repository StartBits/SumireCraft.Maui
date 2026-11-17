namespace System.Runtime.CompilerServices;

/// <summary>
/// Lets records and init accessors compile for netstandard2.0, which does not have this type. The
/// compiler looks for it by name alone.
/// </summary>
internal static class IsExternalInit;
