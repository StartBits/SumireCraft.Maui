using System.Reflection;

namespace SumireCraft.Maui.Sample;

/// <summary>
/// What this copy of the application was built as.
/// </summary>
public static class Build
{
    /// <summary>
    /// The build configuration, taken from the assembly rather than from a compile-time switch, so
    /// that a configuration added later reports itself without anything here being changed.
    /// </summary>
    public static string Configuration { get; } =
        typeof(Build).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
        ?? "unknown";
}
