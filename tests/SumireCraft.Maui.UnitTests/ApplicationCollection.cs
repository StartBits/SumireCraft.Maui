namespace SumireCraft.Maui.UnitTests;

/// <summary>
/// Runs the test classes that make an <see cref="Application"/> on their own, with nothing else
/// alongside.
/// </summary>
/// <remarks>
/// An application makes itself <see cref="Application.Current"/> when it is created, and a view
/// with no application among its parents looks for implicit styles there. Any other test running
/// at the same time would see this one's styles. A test class in this collection sets
/// <see cref="Application.Current"/> back to null when it is done. Nothing else runs alongside it,
/// so it may load XAML as well.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApplicationCollection
{
    public const string Name = "Application";
}
