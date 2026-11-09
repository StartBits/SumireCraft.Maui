namespace SumireCraft.Maui.UnitTests;

/// <summary>
/// Serialises the test classes that call <c>LoadFromXaml</c>. .NET MAUI's <c>XamlLoader</c> writes to
/// a <see cref="Dictionary{TKey, TValue}"/> without a lock, so two loads at once can fail. Test
/// classes run in parallel; a page loading on the UI thread does not.
/// </summary>
[CollectionDefinition(Name)]
public sealed class XamlLoadingCollection
{
    public const string Name = "XamlLoading";
}
