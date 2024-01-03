using System.Diagnostics;
using System.Text;

namespace TimeIt;

public class TimeIt : IDisposable
{
    internal readonly Stopwatch _sw;
    private const double TOLERANCE = 1e-6;

    private static readonly char[] _asciiBarStyle = { '#' };
    private static readonly string[] _metricUnits = { "", "k", "M", "G", "T", "P", "E", "Z" };
    private static readonly long _swTicksIn1Hour = Stopwatch.Frequency * 3600;
    private static bool _unicodeNotWorky;
    private readonly string _description;
    private readonly int _maxGlyphWidth;
    private readonly string _progressCountFmt;
    private readonly double _repaintProgressIncrement;
    private readonly char[] _selectedBarStyle;
    private readonly double _smoothingFactor;
    private readonly string _unitName;
    private readonly bool _useMetricAbbreviations;
    private Action<string>? _callback;
    private int _forceRepaint;
    private int _isDisposed;
    private long _lastProgress;
    private long _lastRepaintTicks;
    private double _nextRepaintProgress;
    private double _rate;
    private TimeItState _state;
    private TimeSpan _totalTime;

    public TimeIt(long total, long initialProgress = 0, TimeItSettings? settings = null, Action<string>? callback = null)
    {
        int epilogueLen;
        if (total < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(total), "cannot be negative");
        }

        if (initialProgress < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialProgress), "cannot be negative");
        }

        Settings = settings ?? new();
        Total = total;
        Progress = initialProgress;

        _unitName = Settings.UnitName;
        _description = Settings.Description;
        _useMetricAbbreviations = Settings.MetricAbbrevations;
        _smoothingFactor = Settings.SmoothingFactor;
        _callback = callback;

        _selectedBarStyle = Settings.UseASCII || _unicodeNotWorky ? _asciiBarStyle : TimeItBarStyleDefs.Glyphs[(int)Settings.Style];

        if (Settings.MetricAbbrevations)
        {
            var (abbrevTotal, suffix) = GetMetricAbbreviation(total);
            _progressCountFmt = $"{{0,3}}{{1}}/{abbrevTotal}{suffix}";
            epilogueLen = "|123K/999K".Length;
        }
        else
        {
            var totalChars = CountDigits(Total);
            _progressCountFmt = $"{{0,{totalChars}}}/{total}";
            epilogueLen = 1 + totalChars * 2 + 1;
        }

        const string EPILOGUE_SAMPLE = " [11:22s<33:44s, 123.45/s]";

        epilogueLen += EPILOGUE_SAMPLE.Length + Settings.UnitName.Length;

        var capturedWidth = Settings.Width ?? 100;

        var prologueCount = (string.IsNullOrWhiteSpace(Settings.Description) ? 0 : Settings.Description.Length) + 7;

        _maxGlyphWidth = capturedWidth - prologueCount - epilogueLen;

        _repaintProgressIncrement = 1;

        _nextRepaintProgress =
            Progress / _repaintProgressIncrement * _repaintProgressIncrement +
            _repaintProgressIncrement;

        _rate = double.NaN;
        _totalTime = TimeSpan.MaxValue;
        _sw = Stopwatch.StartNew();

        Parent = TimeItRegistry.TimeItStack.Value.Count == 0
            ? null : TimeItRegistry.TimeItStack.Value.Peek();

        TimeItRegistry.AddInstance(this);

        if (Parent != null)
        {
            Parent.Child = this;
        }

        TimeItRegistry.TimeItStack.Value.Push(this);
    }

    /// <summary>
    /// The elapsed amount of time this operation has taken so far
    /// </summary>
    /// <value>The elapsed time.</value>
    public TimeSpan ElapsedTime { get; private set; }

    /// <summary>
    /// Unique identifier for this instance
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The current progress value of the progress bar <remarks>Always between 0 .. <see cref="Total"/></remarks>
    /// </summary>
    public long Progress { get; set; }

    /// <summary>
    /// The visual settings used for this instance
    /// </summary>
    /// <value>The settings.</value>
    public TimeItSettings Settings { get; }

    /// <summary>
    /// The current <see cref="TimeItState"/> of the instance
    /// </summary>
    /// <value>The state.</value>
    public TimeItState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            ForceRepaint = true;
        }
    }

    /// <summary>
    /// The maximal value of the progress bar which represents 100% <remarks>When the value is not
    /// supplied, only basic statistics will be displayed</remarks>
    /// </summary>
    public long Total { get; }

    /// <summary>
    /// The predicted total amount of time this operation will take
    /// </summary>
    /// <value>The total time.</value>
    public TimeSpan TotalTime { get; private set; }

    internal bool IsDisposed
    {
        get => _isDisposed == 1;
        set => Interlocked.Exchange(ref _isDisposed, value ? 1 : 0);
    }

    internal bool NeedsRepaint
    {
        get
        {
            var updateSpan = Stopwatch.Frequency;
            var swElapsedTicks = _sw.ElapsedTicks;

            if (swElapsedTicks >= _swTicksIn1Hour || _totalTime.Ticks >= TimeSpan.TicksPerHour)
                updateSpan = Stopwatch.Frequency * 60;

            if (ForceRepaint)
            {
                ForceRepaint = false;
                return true;
            }

            if (_lastRepaintTicks + updateSpan < swElapsedTicks)
                return true;

            if ((Progress + NestedProgress) >= _nextRepaintProgress)
            {
                _nextRepaintProgress += _repaintProgressIncrement;
                return true;
            }

            return false;
        }
    }

    private TimeIt Child { get; set; }

    private bool ForceRepaint
    {
        get => _forceRepaint == 1;
        set => Interlocked.Exchange(ref _forceRepaint, value ? 1 : 0);
    }

    private double NestedProgress =>
            Child == null ?
                0 :
                (Child.Progress + Child.NestedProgress) / Child.Total;

    private TimeIt Parent { get; }

    private bool ShouldShoveDescription => (Settings.Elements & TimeItElement.Description) != 0;

    private bool ShouldShoveProgressBar => (Settings.Elements & TimeItElement.ProgressBar) != 0;

    private bool ShouldShoveProgressCount => (Settings.Elements & TimeItElement.ProgressCount) != 0;

    private bool ShouldShoveProgressPercent => (Settings.Elements & TimeItElement.ProgressPercent) != 0;

    private bool ShouldShoveRate => (Settings.Elements & TimeItElement.Rate) != 0;

    private bool ShouldShoveTime => (Settings.Elements & TimeItElement.Time) != 0;

    /// <summary>
    /// Releases all resources used by the progress bar
    /// </summary>
    public void Dispose()
    {
        TimeItRegistry.TimeItStack.Value.Pop();
        TimeItRegistry.RemoveInstance(this);
    }

    internal void Repaint(StringBuilder buffer)
    {
        // Capture progress while repainting
        var progress = Progress;
        var nestedProgress = NestedProgress;
        var elapsedTicks = _sw.ElapsedTicks;

        (_rate, _totalTime) = RecalculateRateAndTotalTime();

        if (ShouldShoveDescription)
        {
            ShoveDescription();
        }

        if (ShouldShoveProgressPercent)
        {
            ShoveProgressPercentage();
        }

        if (ShouldShoveProgressBar)
        {
            ShoveProgressBar();
        }

        if (ShouldShoveProgressCount)
        {
            ShoveProgressTotals();
        }

        buffer.Append(' ');
        //[{{0}}<{{1}}, {{2}}{unitName}/s]

        // At least one of Time|Rate is turned on?
        if ((Settings.Elements & (TimeItElement.Rate | TimeItElement.Time)) != 0)
        {
            buffer.Append('[');
        }

        ShoveTime();
        if (ShouldShoveTime)
        {
            buffer.Append(", ");
        }

        if (ShouldShoveRate)
        {
            ShoveRate();
        }

        if ((Settings.Elements & (TimeItElement.Rate | TimeItElement.Time)) != 0)
        {
            buffer.Append(']');
        }
        _callback?.Invoke(buffer.ToString().Trim());

        _lastProgress = progress;
        _lastRepaintTicks = elapsedTicks;

        (double rate, TimeSpan totalTime) RecalculateRateAndTotalTime()
        {
            // If we're "told" not to smooth out the rate/total time prediciton, we just use the
            // whole thing for the progress calc, otherwise we continuously sample the last rate
            // update since the previous rate and smooth it out using EMA/SmoothingFactor
            double rate;
            if (Math.Abs(_smoothingFactor) < TOLERANCE)
            {
                rate = ((double)progress * Stopwatch.Frequency) / elapsedTicks;
            }
            else
            {
                var dProgress = progress - _lastProgress;
                var dTicks = elapsedTicks - _lastRepaintTicks;

                var lastRate = ((double)dProgress * Stopwatch.Frequency) / dTicks;
                rate = _lastRepaintTicks == 0 ? lastRate : _smoothingFactor * lastRate + (1 - _smoothingFactor) * _rate;
            }

            var totalTime = rate <= 0 ? TimeSpan.MaxValue : new TimeSpan((long)(Total * TimeSpan.TicksPerSecond / rate));
            // In case rate is so slow, we are overflowing
            if (totalTime.Ticks < 0)
                totalTime = TimeSpan.MaxValue;
            return (rate, totalTime);
        }

        void ShoveDescription()
        {
            if (string.IsNullOrWhiteSpace(_description))
            {
                return;
            }
            buffer.Append(_description);
            buffer.Append(": ");
        }

        void ShoveProgressPercentage()
        {
            buffer.AppendFormat("{0,3}%", (int)(((progress + nestedProgress) / Total) * 100));
        }

        void ShoveProgressBar()
        {
            buffer.Append('|');

            var numChars = _selectedBarStyle.Length > 1
                ? RenderComplexProgressGlyphs()
                : RenderSimpleProgressGlyphs();
            var numSpaces = _maxGlyphWidth - numChars;
            if (numSpaces > 0)
                buffer.Append(' ', numSpaces);
            buffer.Append('|');

            int RenderSimpleProgressGlyphs()
            {
                var numGlypchChars = (int)((progress * _maxGlyphWidth) / Total);
                buffer.Append(_selectedBarStyle[0], numGlypchChars);
                return numGlypchChars;
            }

            int RenderComplexProgressGlyphs()
            {
                var blocks = ((progress + nestedProgress) * (_maxGlyphWidth * _selectedBarStyle.Length)) / Total;
                Debug.Assert(blocks >= 0);
                var completeBlocks = (int)(blocks / _selectedBarStyle.Length);
                buffer.Append(_selectedBarStyle[_selectedBarStyle.Length - 1], completeBlocks);
                var lastCharIdx = (int)(blocks % _selectedBarStyle.Length);

                if (lastCharIdx == 0)
                {
                    return completeBlocks;
                }

                buffer.Append(_selectedBarStyle[lastCharIdx]);
                return completeBlocks + 1;
            }
        }

        void ShoveProgressTotals()
        {
            if (_useMetricAbbreviations)
            {
                var (abbrevNum, suffix) = GetMetricAbbreviation(Progress);
                buffer.AppendFormat(_progressCountFmt, abbrevNum, suffix);
            }
            else
            {
                buffer.AppendFormat(_progressCountFmt, Progress);
            }
        }

        void ShoveTime()
        {
            WriteTimes(buffer, new TimeSpan((elapsedTicks * TimeSpan.TicksPerSecond) / Stopwatch.Frequency), _totalTime);
        }

        void ShoveRate()
        {
            if (_useMetricAbbreviations)
            {
                var (abbrevNum, suffix) = GetMetricAbbreviation(_rate);
                if (abbrevNum < 100)
                    buffer.AppendFormat("{0:F2}{1}{2}/s", abbrevNum, suffix, _unitName);
                else
                    buffer.AppendFormat("{0}{1}{2}/s", (int)abbrevNum, suffix, _unitName);
            }
            else
            {
                if (_rate < 100)
                    buffer.AppendFormat("{0:F2}{1}/s", _rate, _unitName);
                else
                    buffer.AppendFormat("{0}{1}/s", (int)_rate, _unitName);
            }
        }
    }

    static int CountDigits(long number)
    {
        var digits = 0;
        while (number != 0)
        {
            number /= 10;
            digits++;
        }
        return digits;
    }

    static (long num, string abbrev) GetMetricAbbreviation(long num)
    {
        for (var i = 0; i < _metricUnits.Length; i++)
        {
            if (num < 1000)
            {
                return (num, _metricUnits[i]);
            }

            num /= 1000;
        }
        throw new ArgumentOutOfRangeException(nameof(num), "is too large");
    }

    static (double num, string abbrev) GetMetricAbbreviation(double num)
    {
        for (var i = 0; i < _metricUnits.Length; i++)
        {
            if (num < 1000)
            {
                return (num, _metricUnits[i]);
            }

            num /= 1000;
        }

        throw new ArgumentOutOfRangeException(nameof(num), "is too large");
    }

    static void WriteTimes(StringBuilder buffer, TimeSpan elapsed, TimeSpan remaining)
    {
        Debug.Assert(elapsed.Ticks >= 0);
        Debug.Assert(remaining.Ticks >= 0);
        var (edays, ehours, eminutes, eseconds, _) = elapsed;
        var (rdays, rhours, rminutes, rseconds, _) = remaining;

        if (edays + rdays > 0)
        {
            // Print days formatting
        }
        else if (ehours + rhours > 0)
        {
            if (elapsed == TimeSpan.MaxValue)
                buffer.Append("--:--?<");
            else
                buffer.AppendFormat("{0:D2}:{1:D2}m<", ehours, eminutes);
            if (remaining == TimeSpan.MaxValue)
                buffer.Append("--:--?");
            else
                buffer.AppendFormat("{0:D2}:{1:D2}m", rhours, rminutes);
        }
        else
        {
            if (elapsed == TimeSpan.MaxValue)
            {
                buffer.Append("--:--?<");
            }
            else
            {
                buffer.AppendFormat("{0:D2}:{1:D2}s<", eminutes, eseconds);
            }

            if (remaining == TimeSpan.MaxValue)
            {
                buffer.Append("--:--?");
            }
            else
            {
                buffer.AppendFormat("{0:D2}:{1:D2}s", rminutes, rseconds);
            }
        }
    }
}