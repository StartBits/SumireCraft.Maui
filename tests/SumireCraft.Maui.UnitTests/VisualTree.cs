namespace SumireCraft.Maui.UnitTests;

// Measures the visual tree .NET MAUI builds, with no platform underneath: what each level of a widget
// is, whatever the platform then makes of it.
internal static class VisualTree
{
    // The elements from the root down to the target, both included, by type name. Searched from the
    // root down rather than climbed from the target: what a ContentPresenter shows answers
    // GetVisualParent() with the templated view that owns it, which would leave the levels in between
    // uncounted. A target that is not under the root fails the test: a depth counted from nothing
    // would pass any limit.
    public static IReadOnlyList<string> PathTo(IVisualTreeElement root, IVisualTreeElement target)
    {
        var path = new List<string>();
        return Find(root, target, path)
            ? path
            : throw new Xunit.Sdk.XunitException($"{target.GetType().Name} is not in the visual tree of {root.GetType().Name}.");
    }

    private static bool Find(IVisualTreeElement node, IVisualTreeElement target, List<string> path)
    {
        path.Add(node.GetType().Name);
        if (ReferenceEquals(node, target))
        {
            return true;
        }

        foreach (var child in node.GetVisualChildren())
        {
            if (Find(child, target, path))
            {
                return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }
}
