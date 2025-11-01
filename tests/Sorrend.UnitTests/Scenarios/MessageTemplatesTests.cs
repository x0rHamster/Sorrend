using System.Globalization;
using Sorrend.Core.Utilities.MessageTemplates;

namespace Sorrend.UnitTests.Scenarios;

public class MessageTemplatesTests
{
    [Fact]
    public void ReplacesNamedPlaceholdersWithArguments_UsingArgumentsOrder()
    {
        var result = MessageFormatter.Format(
            "What do you get if you multiply {Left} by {Right}? {Result}. That's it.",
            [6, 9, 42],
            CultureInfo.InvariantCulture);

        Assert.Equal("What do you get if you multiply 6 by 9? 42. That's it.", result);
    }

    [Fact]
    public void ReplacesIndexedPlaceholdersWithArguments_UsingPlaceholderIndexes()
    {
        var result = MessageFormatter.Format(
            "{5}, {4}, {1}, {3}, {0} or {2}",
            [23, 15, 42, 16, 8, 4],
            CultureInfo.InvariantCulture);

        Assert.Equal("4, 8, 15, 16, 23 or 42", result);
    }

    [Fact]
    public void TreatsIndexedPlaceholdersAsNamed_WhenTemplateHasNamedPlaceholders()
    {
        var result = MessageFormatter.Format(
            "{5}, {4}, {1}, {3}, {Zero} or {2}",
            [23, 15, 42, 16, 8, 4],
            CultureInfo.InvariantCulture);

        Assert.Equal("23, 15, 42, 16, 8 or 4", result);
    }

    [Theory]
    [InlineData("{First} or {Second}", "6 or 28")]
    [InlineData("{2} or {1}", "496 or 28")]
    public void IgnoresExcessArguments(string messageTemplate, string expected)
    {
        var result = MessageFormatter.Format(
            messageTemplate,
            [6, 28, 496, 8128],
            CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("{First}, {Second}, {Third} or {Fourth,8:E}", "220, 284, {Third} or {Fourth,8:E}")]
    [InlineData("{1}, {2} or {3,8:E}", "284, {2} or {3,8:E}")]
    public void LeavesPlaceholders_WithoutMatchingArguments(string messageTemplate, string expected)
    {
        var result = MessageFormatter.Format(
            messageTemplate,
            [220, 284],
            CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("{First}, {Second}, {First} or {Third}", "3, 5, 3 or 17")]
    [InlineData("{1}, {2}, {1} or {3}", "5, 17, 5 or 257")]
    public void ReplacesRepeatedPlaceholders_WithSameArguments(string messageTemplate, string expected)
    {
        var result = MessageFormatter.Format(
            messageTemplate,
            [3, 5, 17, 257],
            CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TreatsDoubleBracketsAsEscaped()
    {
        var result = MessageFormatter.Format(
            "{First}, {{Escaped}} or {Second}",
            [2, 5, 52],
            CultureInfo.InvariantCulture);

        Assert.Equal("2, {Escaped} or 5", result);
    }

    [Fact]
    public void FormatsPrimitivesToString_WithoutQuotes()
    {
        var result = MessageFormatter.Format(
            "{Bool}, {Real} or {Enum}",
            [false, -3.14, DateTimeKind.Utc],
            CultureInfo.InvariantCulture);

        Assert.Equal("False, -3.14 or Utc", result);
    }

    [Fact]
    public void QuotesStringLikePrimitives()
    {
        var result = MessageFormatter.Format(
            "{String} or {Char}",
            ["the \\ \"word\"", "e"],
            CultureInfo.InvariantCulture);

        Assert.Equal("\"the \\\\ \\\"word\\\"\" or \"e\"", result);
    }

    [Fact]
    public void HighlightsNulls()
    {
        var result = MessageFormatter.Format(
            "{Value}",
            [null],
            CultureInfo.InvariantCulture);

        Assert.Equal("<null>", result);
    }

    [Fact]
    public void FormatsObjectsToString_WithoutQuotes()
    {
        var result = MessageFormatter.Format(
            "{Formattable} or {OverriddenToString}",
            [
                new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Unspecified),
                new ToStringReturns("Towel"),
            ],
            CultureInfo.InvariantCulture);

        Assert.Equal("12/31/1999 23:59:59 or Towel", result);
    }

    [Fact]
    public void FormatsObjectsAsFullTypeNames_WhenToStringIsMissingOrBroken()
    {
        var result = MessageFormatter.Format(
            "{MissingToString}, {ToStringReturnsNull}, {ToStringReturnsEmpty} or {ToStringThrows}",
            [
                new object(),
                new ToStringReturns(null),
                new ToStringReturns(string.Empty),
                new ToStringThrows(),
            ],
            CultureInfo.InvariantCulture);

        Assert.Equal(
            "System.Object,"
            + " Sorrend.UnitTests.Scenarios.MessageTemplatesTests+ToStringReturns,"
            + " Sorrend.UnitTests.Scenarios.MessageTemplatesTests+ToStringReturns"
            + " or Sorrend.UnitTests.Scenarios.MessageTemplatesTests+ToStringThrows",
            result);
    }

    [Fact]
    public void EnumeratesEnumerables()
    {
        var result = MessageFormatter.Format(
            "{Enumerable}",
            [
                new object?[] { 47, "inflation \\ \"deflation\"", null },
            ],
            CultureInfo.InvariantCulture);

        Assert.Equal("[47, \"inflation \\\\ \\\"deflation\\\"\", <null>]", result);
    }

    [Fact]
    public void FormatsArguments_UsingFormatStrings()
    {
        var result = MessageFormatter.Format(
            "{Real:#0.00%} or {Guid:B}",
            [0.12345, Guid.Parse("382c74c3721d4f3480e557657b6cbc27")],
            CultureInfo.InvariantCulture);

        Assert.Equal("12.35% or {382c74c3-721d-4f34-80e5-57657b6cbc27}", result);
    }

    [Fact]
    public void ControlsAlignment()
    {
        var result = MessageFormatter.Format(
            "{Alignment,8} or {AlignedFormat,-8:X}",
            [142857, 19229],
            CultureInfo.InvariantCulture);

        Assert.Equal("  142857 or 4B1D    ", result);
    }

    [Fact]
    public void FormatsArguments_UsingFormatProvider()
    {
        var result = MessageFormatter.Format(
            "{Real} or {Date}",
            [
                1234.56,
                new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Unspecified),
            ],
            CultureInfo.GetCultureInfo("es-CO"));

        Assert.Equal("1234,56 or 31/12/1999 11:59:59 p. m.", result);
    }

    [Fact]
    public void IgnoresInvalidNamedPlaceholders()
    {
        var result = MessageFormatter.Format(
            "{}, {With Space}, {With.Dot}, {With-Hyphen}, {InvalidAlignment,9-}, {EmptyFormat:}, {{LeftEscaped}, {RightEscaped}} or {First}",
            [255],
            CultureInfo.InvariantCulture);

        Assert.Equal(
            "{}, {With Space}, {With.Dot}, {With-Hyphen}, {InvalidAlignment,9-}, {EmptyFormat:}, {LeftEscaped}, {RightEscaped} or 255",
            result);
    }

    [Fact]
    public void IgnoresInvalidIndexedPlaceholders()
    {
        var result = MessageFormatter.Format(
            "{-1} or {2}",
            [1980, 2016, 2556],
            CultureInfo.InvariantCulture);

        Assert.Equal("{-1} or 2556", result);
    }

    private class ToStringReturns(string? value)
    {
        public override string? ToString() => value;
    }

    private class ToStringThrows
    {
        [SuppressMessage(
            "Blocker Code Smell",
            "S3877:Exceptions should not be thrown from unexpected methods",
            Justification = "Negative tests require the implementation of incorrect behavior")]
        public override string ToString() => throw new InvalidOperationException();
    }
}
