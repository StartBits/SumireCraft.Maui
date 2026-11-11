using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Fields;

// Styles written the way an application writes them in XAML, {AppThemeBinding} included, which has
// no public way to be made in code.
[Collection(XamlLoadingCollection.Name)]
public class SmrFieldXamlTests
{
    // The implicit style for every label in the Styles.xaml that `dotnet new maui` writes.
    [Fact]
    public void TheTemplatesStyleForEveryLabel_LeavesTheErrorRed()
    {
        const string Xaml = """
            <Grid xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
              <Grid.Resources>
                <Style TargetType="Label">
                  <Setter Property="TextColor" Value="{AppThemeBinding Light=Black, Dark=White}" />
                  <Setter Property="FontFamily" Value="OpenSansRegular" />
                </Style>
              </Grid.Resources>
            </Grid>
            """;
        var page = new Grid().LoadFromXaml(Xaml);
        var field = new SmrField { ErrorText = "Required" };
        var other = new Label();

        page.Add(field);
        page.Add(other);

        Assert.Equal(Colors.Black, other.TextColor);
        Assert.Equal(Color.FromArgb("#B3261E"), Error(field).TextColor);
        Assert.Equal("OpenSansRegular", Error(field).FontFamily);
    }

    private static Label Error(SmrField field)
    {
        var root = Assert.IsType<Grid>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()), exactMatch: false);
        return (Label)root.Children[2];
    }
}
