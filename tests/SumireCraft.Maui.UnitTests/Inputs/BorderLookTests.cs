using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Inputs;

// Android and iOS draw a border's sides where they are told; Windows mirrors a view laid out right to
// left by itself. What those two are told follows .NET MAUI, whose layouts put a Margin's or a
// Padding's left at the right of a view laid out right to left, and whose Border there keeps its
// corners where they are.
public class BorderLookTests
{
    private static readonly BorderLook Look = new(new Thickness(8, 1, 2, 3), new CornerRadius(4, 5, 6, 7), Colors.Teal);

    [Fact]
    public void LaidOutLeftToRight_IsAsGiven()
    {
        Assert.Equal(Look, Look.LaidOut(EffectiveFlowDirection.Explicit));
    }

    [Fact]
    public void LaidOutRightToLeft_HasTheLeftThicknessAtTheRight()
    {
        var laidOut = Look.LaidOut(EffectiveFlowDirection.RightToLeft);

        Assert.Equal(new Thickness(2, 1, 8, 3), laidOut.Thickness);
    }

    [Fact]
    public void LaidOutRightToLeft_KeepsItsCornersAndColour()
    {
        var laidOut = Look.LaidOut(EffectiveFlowDirection.RightToLeft | EffectiveFlowDirection.Explicit);

        Assert.Equal(Look.CornerRadius, laidOut.CornerRadius);
        Assert.Equal(Look.Color, laidOut.Color);
    }
}
