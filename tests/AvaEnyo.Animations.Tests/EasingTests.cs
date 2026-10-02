using Xunit;

namespace AvaEnyo.Animations.Tests;

public class EasingTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void Curves_Match_Enyo_Formulas(double n)
    {
        Assert.Equal(n, EnyoEasing.Linear.Ease(n), 9);
        Assert.Equal(System.Math.Pow(n, 3), EnyoEasing.CubicIn.Ease(n), 9);
        Assert.Equal(System.Math.Pow(n - 1, 3) + 1, EnyoEasing.CubicOut.Ease(n), 9);
        Assert.Equal(n == 1 ? 1 : 1 - System.Math.Pow(2, -10 * n), EnyoEasing.ExpoOut.Ease(n), 9);
        var expected = n * 2 < 1 ? System.Math.Pow(2 * n, 2) / 2 : -(((2 * n) - 1) * ((2 * n) - 3) - 1) / 2;
        Assert.Equal(expected, EnyoEasing.QuadInOut.Ease(n), 9);
    }

    [Fact]
    public void Curves_Start_At_Zero_And_End_At_One()
    {
        foreach (var e in new[] { EnyoEasing.Linear, EnyoEasing.CubicIn, EnyoEasing.CubicOut, EnyoEasing.ExpoOut, EnyoEasing.QuadInOut })
        {
            Assert.Equal(0, e.Ease(0), 9);
            Assert.Equal(1, e.Ease(1), 9);
        }
    }
}
