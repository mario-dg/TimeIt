using System.Collections.Concurrent;
using System.Text;

namespace TimeIt;

internal static class TimeItRegistry
{
    internal static ThreadLocal<Stack<TimeIt>> TimeItStack =
        new(() => new Stack<TimeIt>());

    private const int INTERVAL_MS = 50;
    private static readonly StringBuilder _buffer = new(350 * 10);
    private static readonly IDictionary<int, TimeIt> _instances = new ConcurrentDictionary<int, TimeIt>();
    private static readonly object _threadLock = new();
    private static char[] _chars = new char[350 * 10];
    private static int _isRunning;
    private static int _maxTimeItPosition;
    private static Thread _monitorThread;
    private static int _totalLinesAddedAfterTimeIt;
    private static bool _wasCursorHidden;

    private static bool IsRunning
    {
        get => _isRunning == 1;
        set => Interlocked.Exchange(ref _isRunning, value ? 1 : 0);
    }

    internal static void AddInstance(TimeIt timeIt)
    {
        lock (_threadLock)
        {
            _instances.Add(GetOrSetId(timeIt), timeIt);

            if (IsRunning)
            {
                // in the case we are on windows has already been red-pilled So repaint() and bye-bye
                RepaintTimeIt(timeIt);
                return;
            }

            // If we are just starting up the monitoring thread, we've just potentially red-pilled
            // the windows console, so we can repaint now
            RepaintTimeIt(timeIt);

            IsRunning = true;

            _monitorThread = new(UpdateTimeIts)
            {
                Name = "timeit-updater"
            };
            _monitorThread.Start();
        }
    }

    internal static void RemoveInstance(TimeIt timeIt)
    {
        lock (_threadLock)
        {
            // Repaint just before removing for cosmetic purposes: In case we didn't have a recent
            // update to the progress bar, it might be @ 100% "in reality" but not visually.... This
            // call will close that gap
            if (timeIt.Settings.Leave)
            {
                RepaintTimeIt(timeIt);
            }
            else
            {
                ClearTimeIt(timeIt);
            }

            _instances.Remove(timeIt.Id);
            // Unfortunately, we need to mark that we've drawn this TimeIt for the last time while
            // still holding the console lock...
            timeIt.IsDisposed = true;

            if (_instances.Count > 0)
                return;

            IsRunning = false;

            _totalLinesAddedAfterTimeIt = 0;
            _monitorThread?.Join();
        }
    }

    static void AppendTimeItToBuffer(TimeIt timeIt, StringBuilder buffer)
    {
        timeIt.Repaint(buffer);
    }

    static void ClearTimeIt(TimeIt timeIt)
    {
        timeIt.Repaint(_buffer);
        // Looks silly eh? The reason is we don't want to bother to understand how many printable
        // characters are in the buffer so we simply backspace _buffer.Count and we know for sure
        // we've deleted the entire line
        _buffer.Append('\b', _buffer.Length);
        SpillBuffer();
    }

    static int GetOrSetId(TimeIt timeIt)
    {
        return timeIt.Id = _instances.Count;
    }

    static void RepaintTimeIt(TimeIt timeIt)
    {
        _buffer.Clear();
        AppendTimeItToBuffer(timeIt, _buffer);
        SpillBuffer();
    }

    static void SpillBuffer()
    {
        if (_buffer.Length > _chars.Length)
            Array.Resize(ref _chars, _buffer.Length);
        _buffer.CopyTo(0, _chars, 0, _buffer.Length);
    }

    static void UpdateTimeIts()
    {
        while (IsRunning)
        {
            _buffer.Clear();
            foreach (var y in _instances.Values)
            {
                if (!y.NeedsRepaint)
                    continue;

                AppendTimeItToBuffer(y, _buffer);

                if (_buffer.Length > 0)
                    SpillBuffer();

                Thread.Sleep(INTERVAL_MS);
            }
        }
    }
}