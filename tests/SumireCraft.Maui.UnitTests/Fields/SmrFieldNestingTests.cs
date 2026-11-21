using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Fields;

// How deep a field puts what it holds. Every level is a native view on each platform, measured and
// arranged with the page, and a field is written once for every input of a form, so a level added
// here is paid for many times over. The three are the field itself, the root of its template and the
// ContentPresenter .NET MAUI's templating needs; a form written by hand needs one.
public class SmrFieldNestingTests
{
    private const int ContentLevels = 3;
    private const int LabelLevels = 2;

    [Fact]
    public void TheContent_IsAtMostThreeLevelsDown()
    {
        var entry = new Entry();
        var field = new SmrField { Title = "Name", Content = entry };

        var path = VisualTree.PathTo(field, entry);

        Assert.True(path.Count - 1 <= ContentLevels, $"The content is {path.Count - 1} levels down: {string.Join(" > ", path)}.");
    }

    [Fact]
    public void TheTitleAndTheError_AreAtMostTwoLevelsDown()
    {
        var field = new SmrField { Title = "Name", ErrorText = "Required", Content = new Entry() };

        var labels = field.GetVisualTreeDescendants().OfType<Label>().ToList();

        Assert.Equal(2, labels.Count);
        Assert.All(labels, label =>
        {
            var path = VisualTree.PathTo(field, label);
            Assert.True(path.Count - 1 <= LabelLevels, $"A label is {path.Count - 1} levels down: {string.Join(" > ", path)}.");
        });
    }
}
