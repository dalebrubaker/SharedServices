using System;
using System.Collections.Generic;
using BruSoftware.SharedServices.ExtensionMethods;
using FluentAssertions;
using Xunit;

namespace BruSoftware.SharedServicesTests;

public class ExtensionsTests
{
    [Fact]
    public void IsAllDefaultTest()
    {
        var values = new List<double>
        {
            0,
            0
        };
        var isAllDefault = values.IsAllDefault();
        Assert.True(isAllDefault);
        var values2 = new List<double>
        {
            0,
            1
        };
        var isAllDefault2 = values2.IsAllDefault();
        Assert.False(isAllDefault2);
    }

    [Fact]
    public void MinuteOfDayTests()
    {
        var timestamp0 = DateTime.Now.Date;
        var minute0 = timestamp0.ToMinuteOfDay();
        minute0.Should().Be(0);
        var timestamp1 = timestamp0.AddMinutes(1);
        var minute1 = timestamp1.ToMinuteOfDay();
        minute1.Should().Be(1);
        var timestamp1439 = timestamp0.AddMinutes(-1);
        var minute1439 = timestamp1439.ToMinuteOfDay();
        minute1439.Should().Be(1439);
    }

    [Fact]
    public void RemoveElementTests()
    {
        var array1 = new[] { 0 };
        array1.Length.Should().Be(1);
        var result1 = array1.RemoveElement(0);
        result1.Length.Should().Be(0);

        var array2 = new[] { 0, 1 };
        array2.Length.Should().Be(2);
        var result2A = array2.RemoveElement(0);
        result2A.Length.Should().Be(1);
        result2A[0].Should().Be(1);
        var result2B = array2.RemoveElement(1);
        result2B.Length.Should().Be(1);
        result2B[0].Should().Be(0);
    }

    [Fact]
    public void SplitOnDigitsTest()
    {
        var str = "Test";
        var list = str.SplitOnDigits();
        list.Count.Should().Be(1);
        list[0].Should().Be(str);

        str = "Test123";
        list = str.SplitOnDigits();
        list.Count.Should().Be(2);
        list[0].Should().Be("Test");
        list[1].Should().Be("123");

        str = "";
        list = str.SplitOnDigits();
        list.Count.Should().Be(0);

        str = "12Test345ing";
        list = str.SplitOnDigits();
        list.Count.Should().Be(4);
        list[0].Should().Be("12");
        list[1].Should().Be("Test");
        list[2].Should().Be("345");
        list[3].Should().Be("ing");
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 0, 4, 1, 0)]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 0, 5, 3, 2)]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 0, 5, 6, 5)]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 0, 5, 0, 0)]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 1, 4, 3, 2)]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 1, 4, 5, 4)]
    public void GetLowerBound_ShouldReturnCorrectIndex(int[] array, int first, int last, int value, int expectedIndex)
    {
        // Act
        var result = array.LowerBound(first, last, value);

        // Assert
        result.Should().Be(expectedIndex);
    }

    [Fact]
    public void GetLowerBound_ShouldThrowArgumentException_WhenFirstIsNegative()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };

        // Act
        Action act = () => array.LowerBound(-1, 5, 3);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("First index is out of range. (Parameter 'first')");
    }

    [Fact]
    public void GetLowerBound_ShouldThrowArgumentException_WhenLastIsGreaterThanArrayLength()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };

        // Act
        Action act = () => array.LowerBound(0, 6, 3);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Last index is out of range. (Parameter 'last')");
    }

    [Theory]
    [InlineData("This is a test (with a parenthesized string)", "with a parenthesized string")]
    [InlineData("No parentheses here", "")]
    [InlineData("Nested (parentheses (inside) string)", "parentheses (inside) string")]
    [InlineData("(Only one parenthesized string)", "Only one parenthesized string")]
    [InlineData("Multiple (parentheses) in (one) string", "parentheses")]
    [InlineData("Empty parentheses ()", "")]
    public void PullParenthesizedString_ShouldReturnCorrectString(string input, string expected)
    {
        // Act
        var (result, remainingLine) = input.PullParenthesizedString(true);

        // Assert
        result.Should().Be(expected);
        if (result.Length > 0)
        {
            var resultWithParens = "(" + result + ")";
            var remainingLineExpected = input.Replace(resultWithParens, "").TrimEnd();
            remainingLine.Should().Be(remainingLineExpected);
        }
    }

    [Theory]
    [InlineData("This is a test [with a bracketed string]", "with a bracketed string")]
    [InlineData("No brackets here", "")]
    [InlineData("Nested [brackets [inside] string]", "brackets [inside] string")]
    [InlineData("[Only one bracketed string]", "Only one bracketed string")]
    [InlineData("Multiple [brackets] in [one] string", "brackets")]
    [InlineData("Empty brackets []", "")]
    public void PullBracketedString_ShouldReturnCorrectString(string input, string expected)
    {
        // Act
        var (result, remainingLine) = input.PullBracketedString(true);

        // Assert
        result.Should().Be(expected);
        if (result.Length > 0)
        {
            var resultWithBrackets = "[" + result + "]";
            var remainingLineExpected = input.Replace(resultWithBrackets, "").TrimEnd();
            remainingLine.Should().Be(remainingLineExpected);
        }
    }
}