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

    // A required field and the style for its mark, the way an application writes them: a mark with a
    // space before it keeps the space.
    [Fact]
    public void ARequiredFieldWrittenInXaml_TakesTheMarksStyle()
    {
        const string Xaml = """
            <Grid xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                  xmlns:smr="https://github.com/startbits/SumireCraft.Maui">
              <Grid.Resources>
                <Style x:Key="SmrFieldRequiredMark" TargetType="Span"
                       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                  <Setter Property="TextColor" Value="Red" />
                  <Setter Property="FontAttributes" Value="Bold" />
                </Style>
              </Grid.Resources>
              <smr:SmrField Title="Name" IsRequired="True" RequiredMark=" (required)" />
            </Grid>
            """;
        var page = new Grid().LoadFromXaml(Xaml);
        var field = Assert.IsType<SmrField>(Assert.Single(page.Children));

        var spans = Title(field).FormattedText.Spans;
        Assert.Equal(["Name", " (required)"], spans.Select(span => span.Text));
        Assert.Equal(Colors.Red, spans[1].TextColor);
        Assert.Equal(FontAttributes.Bold, spans[1].FontAttributes);
    }

    private static Grid Root(SmrField field) =>
        Assert.IsType<Grid>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()), exactMatch: false);

    private static Label Title(SmrField field) => (Label)Root(field).Children[0];

    private static Label Error(SmrField field) => (Label)Root(field).Children[2];
}
