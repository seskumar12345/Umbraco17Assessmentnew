using Umbraco17Assessment.Services;
using Xunit;

namespace Umbraco17Assessment.Tests;

/// <summary>
/// Unit tests for ReadingTimeService.
/// Run with: dotnet test  (from the solution root or this project folder)
/// </summary>
public sealed class ReadingTimeServiceTests
{
    // Helper: create the service with a round WPM for easy arithmetic
    private static ReadingTimeService Svc(int wpm = 100) => new(wpm);

    [Fact]
    public void Calculate_NullText_ReturnsZero()
    {
        Assert.Equal(0, Svc().Calculate(null));
    }

    [Fact]
    public void Calculate_WhitespaceText_ReturnsZero()
    {
        Assert.Equal(0, Svc().Calculate("   "));
    }

    [Fact]
    public void Calculate_SingleWord_ReturnsOne()
    {
        Assert.Equal(1, Svc().Calculate("Hello"));
    }

    [Fact]
    public void Calculate_ExactlyOneMinute_ReturnsOne()
    {
        // 100 words at 100 WPM → exactly 1 minute
        string text = string.Join(" ", Enumerable.Repeat("word", 100));
        Assert.Equal(1, Svc(wpm: 100).Calculate(text));
    }

    [Fact]
    public void Calculate_OneWordOver_CeilsToTwo()
    {
        // 101 words at 100 WPM → ceiling(1.01) = 2
        string text = string.Join(" ", Enumerable.Repeat("word", 101));
        Assert.Equal(2, Svc(wpm: 100).Calculate(text));
    }

    [Fact]
    public void Calculate_HtmlIsStrippedBeforeCounting()
    {
        // 100 words each wrapped in <p><strong>…</strong></p> — should still be 100 words
        string word = "<p><strong>word</strong></p>";
        string text = string.Join(" ", Enumerable.Repeat(word, 100));
        Assert.Equal(1, Svc(wpm: 100).Calculate(text));
    }

    [Fact]
    public void Calculate_TwoFiftyWords_At100Wpm_ReturnsThree()
    {
        // ceiling(250 / 100) = 3
        string text = string.Join(" ", Enumerable.Repeat("word", 250));
        Assert.Equal(3, Svc(wpm: 100).Calculate(text));
    }

    [Theory]
    [InlineData("~1 min read", 50)]    // ceiling(50/100)=1
    [InlineData("~2 min read", 150)]   // ceiling(150/100)=2
    [InlineData("~3 min read", 201)]   // ceiling(201/100)=3
    public void GetLabel_FormatsCorrectly(string expected, int wordCount)
    {
        string text = string.Join(" ", Enumerable.Repeat("word", wordCount));
        Assert.Equal(expected, Svc(wpm: 100).GetLabel(text));
    }

    [Fact]
    public void Constructor_ThrowsOn_ZeroWpm()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReadingTimeService(0));
    }

    [Fact]
    public void Constructor_ThrowsOn_NegativeWpm()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReadingTimeService(-1));
    }
}
