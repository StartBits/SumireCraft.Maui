using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Badges;

// How deep a badge puts what it marks, and its badge. Every level is a native view on each platform,
// measured and arranged with the page, and a badge may sit on every row of a list, so a level added
// here is paid for many times over. The three above the content are the badge view itself, its
// layout and the ContentPresenter .NET MAUI's templating needs.
public class SmrBadgeViewNestingTests
{
    private const int ContentLevels = 3;
    private const int TextLevels = 3;

    [Fact]
    public void TheContent_IsAtMostThreeLevelsDown()
    {
        var button = new Button { Text = "Inbox" };
        var badge = new SmrBadgeView { Text = "7", Content = button };

        var path = VisualTree.PathTo(badge, button);

        Assert.True(path.Count - 1 <= ContentLevels, $"The content is {path.Count - 1} levels down: {string.Join(" > ", path)}.");
    }

    [Fact]
    public void TheBadgesText_IsAtMostThreeLevelsDown()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button { Text = "Inbox" } };

        var label = Assert.Single(badge.GetVisualTreeDescendants().OfType<Label>());

        var path = VisualTree.PathTo(badge, label);
        Assert.True(path.Count - 1 <= TextLevels, $"The badge's text is {path.Count - 1} levels down: {string.Join(" > ", path)}.");
    }
}
