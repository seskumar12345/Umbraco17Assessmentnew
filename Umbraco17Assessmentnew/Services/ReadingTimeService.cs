using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Composing;
using Umbraco17Assessmentnew.Services;

namespace Umbraco17Assessmentnew.Services;

/// <summary>
/// Calculates reading time at a configurable words-per-minute rate.
/// Registered as a Singleton in Program.cs (stateless, thread-safe).
/// </summary>
public sealed partial class ReadingTimeService : IReadingTimeService
{
    // Average adult silent reading speed (Rayner et al., 2016)
    private const int DefaultWpm = 238;
    private readonly int _wpm;

    public ReadingTimeService() : this(DefaultWpm) { }

    /// <summary>Allows injecting a custom WPM in unit tests without mocking.</summary>
    public ReadingTimeService(int wordsPerMinute)
    {
        if (wordsPerMinute <= 0)
            throw new ArgumentOutOfRangeException(nameof(wordsPerMinute), "Must be > 0.");
        _wpm = wordsPerMinute;
    }

    /// <inheritdoc />
    public int Calculate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        // Strip HTML tags before counting words
        string plain = HtmlTagRegex().Replace(text, " ");
        int wordCount = plain
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        int minutes = (int)Math.Ceiling((double)wordCount / _wpm);
        return Math.Max(minutes, 1);
    }

    /// <inheritdoc />
    public string GetLabel(string? text)
    {
        int minutes = Calculate(text);
        return minutes == 1 ? "~1 min read" : $"~{minutes} min read";
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();
}
