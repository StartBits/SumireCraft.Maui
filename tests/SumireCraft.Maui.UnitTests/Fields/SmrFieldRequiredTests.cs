using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Fields;

// The mark a required field puts after its title: what it shows, how an application restyles it,
// and what a screen reader reads in its place.
public class SmrFieldRequiredTests
{
    [Fact]
    public void IsRequired_IsOffByDefault()
    {
        Assert.False(new SmrField().IsRequired);
    }

    [Fact]
    public void TheMark_IsAnAsteriskByDefault()
    {
        Assert.Equal("*", new SmrField().RequiredMark);
    }

    [Fact]
    public void NothingIsReadInTheMarksPlace_ByDefault()
    {
        Assert.Null(new SmrField().RequiredMarkDescription);
    }

    [Fact]
    public void TheMarksStyleKey_IsSmrFieldRequiredMarkByDefault()
    {
        Assert.Equal("SmrFieldRequiredMark", new SmrField().RequiredMarkStyleKey);
    }

    // The mark follows the title as it is, with nothing put between them, so that a title that
    // wraps keeps the mark right after its last word.

    [Fact]
    public void ARequiredField_PutsTheMarkRightAfterItsTitle()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };

        Assert.Equal("Name*", Shown(Title(field)));
        Assert.Equal("*", Mark(field)?.Text);
    }

    [Fact]
    public void AFieldNotRequired_ShowsItsTitleAlone()
    {
        var field = new SmrField { Title = "Name" };

        Assert.Equal("Name", Shown(Title(field)));
        Assert.Null(Mark(field));
    }

    [Fact]
    public void AMarkOfTheApplicationsOwn_IsShownAsItIs()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMark = " (required)" };

        Assert.Equal("Name (required)", Shown(Title(field)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NoMark_ShowsTheTitleAlone(string? mark)
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMark = mark };

        Assert.Equal("Name", Shown(Title(field)));
        Assert.Null(Mark(field));
    }

    [Fact]
    public void AFieldNoLongerRequired_DropsTheMark()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };

        field.IsRequired = false;

        Assert.Equal("Name", Shown(Title(field)));
        Assert.Null(Mark(field));
    }

    [Fact]
    public void ATitleChangedOnARequiredField_KeepsTheMark()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };

        field.Title = "Full name";

        Assert.Equal("Full name*", Shown(Title(field)));
    }

    [Fact]
    public void AMarkChanged_IsShown()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };

        field.RequiredMark = "!";

        Assert.Equal("Name!", Shown(Title(field)));
    }

    // The mark belongs to the title: with no title there is nothing to mark.

    [Fact]
    public void ARequiredFieldWithoutATitle_ShowsNoMark()
    {
        var field = new SmrField { IsRequired = true };

        Assert.False(Title(field).IsVisible);
    }

    [Fact]
    public void ARequiredFieldWithoutATitleButWithReservedSpace_KeepsABlankLine()
    {
        var field = new SmrField { IsRequired = true, ReserveTitleSpace = true };

        Assert.True(Title(field).IsVisible);
        Assert.Equal(" ", Shown(Title(field)));
        Assert.False(AutomationProperties.GetIsInAccessibleTree(Title(field)));
    }

    // By default the mark looks like the title it follows: it sets nothing of its own.

    [Fact]
    public void TheMarkByDefault_LooksLikeTheTitle()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { field };

        var mark = Mark(field)!;
        Assert.Null(mark.TextColor);
        Assert.Equal(FontAttributes.None, mark.FontAttributes);
        Assert.Null(mark.Style);
    }

    // A span takes no style class in .NET MAUI, so the mark's look is a style for spans under a key,
    // found the way a dynamic resource is: the nearest one wins, in App.xaml or a page's resources.

    [Fact]
    public void AStyleUnderTheMarksKey_ReachesTheMark()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        Assert.Equal(Colors.Red, Mark(field)!.TextColor);
        Assert.Equal(FontAttributes.Bold, Mark(field)!.FontAttributes);
    }

    [Fact]
    public void AStyleUnderTheMarksKey_LeavesTheTitleAlone()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        Assert.Null(Title(field).FormattedText.Spans[0].TextColor);
        Assert.Null(Title(field).TextColor);
    }

    [Fact]
    public void AStyleUnderTheMarksKeyOnThePage_WinsOverOneFurtherOut()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var page = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Purple), Children = { field } };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Teal), Children = { page } };

        Assert.Equal(Colors.Purple, Mark(field)!.TextColor);
    }

    [Fact]
    public void AStyleUnderTheMarksKeyFurtherOut_ReachesTheMark()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var page = new Grid { field };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Teal), Children = { new Grid { page } } };

        Assert.Equal(Colors.Teal, Mark(field)!.TextColor);
    }

    [Fact]
    public void AFieldMadeRequiredOnThePage_StillTakesTheMarksStyle()
    {
        var field = new SmrField { Title = "Name" };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        field.IsRequired = true;

        Assert.Equal(Colors.Red, Mark(field)!.TextColor);
    }

    [Fact]
    public void AStyleKeyOfTheApplicationsOwn_ReachesTheMarkInsteadOfTheDefaultOne()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkStyleKey = "AppFieldRequiredMark" };
        var resources = Resources("SmrFieldRequiredMark", Colors.Red);
        resources.Add("AppFieldRequiredMark", MarkStyle(Colors.Teal));
        _ = new Grid { Resources = resources, Children = { field } };

        Assert.Equal(Colors.Teal, Mark(field)!.TextColor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NoStyleKey_LeavesTheMarkLookingLikeTheTitle(string? key)
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkStyleKey = key };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        Assert.Null(Mark(field)!.TextColor);
    }

    // A span in .NET MAUI takes its label's font size and colour but not its family or attributes, so a
    // marked title, drawn as spans, would lose the bold or the font a style for the title gave it. The
    // field hands the title's font to the title's span, and to the mark wherever the mark's own style
    // says nothing of it.

    [Fact]
    public void AMarkedTitle_KeepsTheFontItsStyleGivesIt()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = TitleFont("SmrFieldTitle", FontAttributes.Bold), Children = { field } };

        var title = TitleSpan(field)!;
        Assert.Equal(FontAttributes.Bold, title.FontAttributes);
        Assert.Equal("Serif", title.FontFamily);
        Assert.False(title.FontAutoScalingEnabled);
    }

    [Fact]
    public void AMarkWithoutAStyle_TakesTheTitlesFont()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkStyleKey = null };
        _ = new Grid { Resources = TitleFont("SmrFieldTitle", FontAttributes.Bold), Children = { field } };

        var mark = Mark(field)!;
        Assert.Equal(FontAttributes.Bold, mark.FontAttributes);
        Assert.Equal("Serif", mark.FontFamily);
        Assert.False(mark.FontAutoScalingEnabled);
    }

    [Fact]
    public void AMarksStyle_WinsOverTheTitlesFontOnlyWhereItSaysSomething()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var resources = TitleFont("SmrFieldTitle", FontAttributes.Italic);
        resources.Add("SmrFieldRequiredMark", MarkStyle(Colors.Red));
        _ = new Grid { Resources = resources, Children = { field } };

        var mark = Mark(field)!;
        Assert.Equal(FontAttributes.Bold, mark.FontAttributes);
        Assert.Equal("Serif", mark.FontFamily);
    }

    [Fact]
    public void AMarksStyle_SaysWhatItsBaseStyleSays()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var resources = TitleFont("SmrFieldTitle", FontAttributes.Italic);
        resources.Add("SmrFieldRequiredMark", new Style(typeof(Span))
        {
            BasedOn = MarkStyle(Colors.Red),
            Setters = { new Setter { Property = Span.TextColorProperty, Value = Colors.Teal } },
        });
        _ = new Grid { Resources = resources, Children = { field } };

        Assert.Equal(FontAttributes.Bold, Mark(field)!.FontAttributes);
    }

    [Fact]
    public void AMarksStyleArrivingLater_TakesOverFromTheTitlesFont()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var page = new Grid { Resources = TitleFont("SmrFieldTitle", FontAttributes.Italic), Children = { field } };

        page.Resources.Add("SmrFieldRequiredMark", MarkStyle(Colors.Red));

        Assert.Equal(FontAttributes.Bold, Mark(field)!.FontAttributes);
    }

    [Fact]
    public void AMarksStyleTakenAway_LeavesTheMarkWithTheTitlesFont()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var resources = TitleFont("SmrFieldTitle", FontAttributes.Italic);
        resources.Add("SmrFieldRequiredMark", MarkStyle(Colors.Red));
        _ = new Grid { Resources = resources, Children = { field } };

        field.RequiredMarkStyleKey = null;

        Assert.Equal(FontAttributes.Italic, Mark(field)!.FontAttributes);
    }

    [Fact]
    public void ATitlesFontChangedLater_IsFollowedByTheTitleAndTheMark()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkStyleKey = null };
        _ = new Grid { Resources = TitleFont("AppFieldTitle", FontAttributes.Bold), Children = { field } };

        field.TitleStyleClass = "AppFieldTitle";

        Assert.Equal(FontAttributes.Bold, TitleSpan(field)!.FontAttributes);
        Assert.Equal(FontAttributes.Bold, Mark(field)!.FontAttributes);
    }

    [Fact]
    public void AFieldMadeRequiredLater_KeepsTheTitlesFont()
    {
        var field = new SmrField { Title = "Name" };
        _ = new Grid { Resources = TitleFont("SmrFieldTitle", FontAttributes.Bold), Children = { field } };

        field.IsRequired = true;

        Assert.Equal(FontAttributes.Bold, TitleSpan(field)!.FontAttributes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AStyleKeyTakenAway_TakesTheMarksStyleAway(string? key)
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        field.RequiredMarkStyleKey = key;

        Assert.Null(Mark(field)!.Style);
        Assert.Null(Mark(field)!.TextColor);
    }

    [Fact]
    public void AStyleKeyChangedToOneWithNoStyle_LeavesTheMarkLookingLikeTheTitle()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        field.RequiredMarkStyleKey = "Undefined";

        AssertLooksLikeTheTitle(field);
    }

    [Fact]
    public void AStyleKeyTakenAwayAndThenChangedToOneWithNoStyle_LeavesTheMarkLookingLikeTheTitle()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        field.RequiredMarkStyleKey = null;
        field.RequiredMarkStyleKey = "Undefined";

        AssertLooksLikeTheTitle(field);
    }

    [Fact]
    public void AStyleKeyChangedToAnotherForTheSameStyle_KeepsTheStyle()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        var style = MarkStyle(Colors.Red);
        _ = new Grid { Resources = new ResourceDictionary { { "SmrFieldRequiredMark", style }, { "AppFieldRequiredMark", style } }, Children = { field } };

        field.RequiredMarkStyleKey = null;
        field.RequiredMarkStyleKey = "AppFieldRequiredMark";

        Assert.Equal(Colors.Red, Mark(field)!.TextColor);
    }

    [Fact]
    public void AStyleKeyGivenBack_BringsTheMarksStyleBack()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };
        _ = new Grid { Resources = Resources("SmrFieldRequiredMark", Colors.Red), Children = { field } };

        field.RequiredMarkStyleKey = null;
        field.RequiredMarkStyleKey = "SmrFieldRequiredMark";

        Assert.Equal(Colors.Red, Mark(field)!.TextColor);
        Assert.Equal(FontAttributes.Bold, Mark(field)!.FontAttributes);
    }

    // A screen reader reads the title with the mark as drawn, unless the application says what to
    // read in the mark's place, in its own language: the library writes no word of its own.

    [Fact]
    public void ARequiredFieldByDefault_IsReadAsDrawn()
    {
        var field = new SmrField { Title = "Name", IsRequired = true };

        Assert.False(Title(field).IsSet(SemanticProperties.DescriptionProperty));
        Assert.True(AutomationProperties.GetIsInAccessibleTree(Title(field)));
    }

    [Fact]
    public void ADescriptionOfTheMark_IsReadInItsPlace()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkDescription = "required" };

        Assert.Equal("Name required", SemanticProperties.GetDescription(Title(field)));
    }

    [Fact]
    public void ADescriptionOfTheMarkOnAFieldNotRequired_IsNotRead()
    {
        var field = new SmrField { Title = "Name", RequiredMarkDescription = "required" };

        Assert.False(Title(field).IsSet(SemanticProperties.DescriptionProperty));
    }

    [Fact]
    public void ADescriptionOfTheMark_FollowsTheTitle()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkDescription = "required" };

        field.Title = "Full name";

        Assert.Equal("Full name required", SemanticProperties.GetDescription(Title(field)));
    }

    [Fact]
    public void AFieldNoLongerRequired_IsReadAsDrawnAgain()
    {
        var field = new SmrField { Title = "Name", IsRequired = true, RequiredMarkDescription = "required" };

        field.IsRequired = false;

        Assert.False(Title(field).IsSet(SemanticProperties.DescriptionProperty));
    }

    [Fact]
    public void ADescriptionOfTheMarkWithoutATitle_IsNotRead()
    {
        var field = new SmrField { IsRequired = true, ReserveTitleSpace = true, RequiredMarkDescription = "required" };

        Assert.False(Title(field).IsSet(SemanticProperties.DescriptionProperty));
    }

    // No style left on the mark: no colour of its own, and the font of a title made italic.
    private static void AssertLooksLikeTheTitle(SmrField field)
    {
        Title(field).FontAttributes = FontAttributes.Italic;

        var mark = Mark(field)!;
        Assert.Null(mark.Style);
        Assert.Null(mark.TextColor);
        Assert.Equal(FontAttributes.Italic, mark.FontAttributes);
    }

    private static ResourceDictionary Resources(string key, Color color) => new() { { key, MarkStyle(color) } };

    private static Style MarkStyle(Color color) => new(typeof(Span))
    {
        Setters =
        {
            new Setter { Property = Span.TextColorProperty, Value = color },
            new Setter { Property = Span.FontAttributesProperty, Value = FontAttributes.Bold },
        },
    };

    // A style for the title's class that sets its font, as an application would write one.
    private static ResourceDictionary TitleFont(string styleClass, FontAttributes attributes) => new()
    {
        new Style(typeof(Label))
        {
            Class = styleClass,
            Setters =
            {
                new Setter { Property = Label.FontAttributesProperty, Value = attributes },
                new Setter { Property = Label.FontFamilyProperty, Value = "Serif" },
                new Setter { Property = Label.FontAutoScalingEnabledProperty, Value = false },
            },
        },
    };

    private static Grid Root(SmrField field) =>
        Assert.IsType<Grid>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()), exactMatch: false);

    private static Label Title(SmrField field) => (Label)Root(field).Children[0];

    // What the title shows, whether as plain text or as spans.
    private static string? Shown(Label label) =>
        label.FormattedText is { } formatted ? string.Concat(formatted.Spans.Select(span => span.Text)) : label.Text;

    private static Span? TitleSpan(SmrField field) =>
        Title(field).FormattedText is { Spans: [var title, _] } ? title : null;

    // The mark is a span of its own after the title's, so that a style can reach it alone.
    private static Span? Mark(SmrField field) =>
        Title(field).FormattedText is { Spans: [_, var mark] } ? mark : null;
}
