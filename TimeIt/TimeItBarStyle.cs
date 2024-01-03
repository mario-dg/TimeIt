namespace TimeIt;

/// <summary>
/// An enumeration representing the various visual styles of a TimeIt progress bar component
/// </summary>
public enum TimeItBarStyle
{
    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '▏', '▎', '▍', '▌', '▋', '▊',
    /// '▉', '█'
    /// </summary>
    BarHorizontal,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '▁', '▂', '▃', '▄', '▅', '▆',
    /// '▇', '█'
    /// </summary>
    BarVertical,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '⣀', '⣄', '⣤', '⣦', '⣶', '⣷', '⣿'
    /// </summary>
    DotsHorizontal,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '⣀', '⣄', '⣆', '⣇', '⣧', '⣷', '⣿'
    /// </summary>
    DotsVertical,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '○', '◔', '◐', '◕', '⬤'
    /// </summary>
    Clock,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '□', '◱', '◧', '▣', '■'
    /// </summary>
    Squares1,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '□', '◱', '▨', '▩', '■'
    /// </summary>
    Squares2,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters: '□', '◱', '▥', '▦', '■'
    /// </summary>
    Squares3,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    ShortSquares,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    LongMesh,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    ShortMesh,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    Parallelogram,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    Rectangles1,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    Rectangles2,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    Circles1,

    /// <summary>
    /// A TimeIt progress bar style that uses the unicode charchters:
    /// </summary>
    Circles2,
}

internal static class TimeItBarStyleDefs
{
    internal static readonly char[][] Glyphs = {
            new[] {'▏', '▎', '▍', '▌', '▋', '▊', '▉', '█'},
            new[] {'▁', '▂', '▃', '▄', '▅', '▆', '▇', '█'},
            new[] {'⣀', '⣄', '⣤', '⣦', '⣶', '⣷', '⣿'},
            new[] {'⣀', '⣄', '⣆', '⣇', '⣧', '⣷', '⣿'},
            new[] {'○', '◔', '◐', '◕', '⬤'},
            new[] {'□', '◱', '◧', '▣', '■'},
            new[] {'□', '◱', '▨', '▩', '■'},
            new[] {'□', '◱', '▥', '▦', '■'},
            new[] {'⬜', '⬛'},
            new[] {'░', '▒', '▓', '█'},
            new[] {'░', '█'},
            new[] {'▱', '▰'},
            new[] {'▭', '◼'},
            new[] {'▯', '▮'},
            new[] {'◯', '⬤'},
            new[] {'⚪', '⚫'},
        };
}