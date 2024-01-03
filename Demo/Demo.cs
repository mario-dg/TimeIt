using System.Diagnostics;

using TimeIt;

using static System.Linq.Enumerable;

namespace Demo;

internal static class Demo
{
    static void Callback(string timeItString)
    {
        Console.WriteLine(timeItString);
    }

    static void Demo1()
    {
        foreach (var i in Range(0, 100).TimeIt(settings: new TimeItSettings { Description = "regular" }, callback: Callback))
        {
            Thread.Sleep(100);
        }
    }

    static void Demo2()
    {
        foreach (var i in Range(0, 100).TimeIt(settings: new TimeItSettings { Description = "smaller", Width = 100 }, callback: Callback))
        {
            Thread.Sleep(100);
        }
    }

    static void Demo3()
    {
        foreach (var i in Range(0, 100).TimeIt(settings: new TimeItSettings
        {
            Description = "without progress bar",
            Width = 50,
            Elements = TimeItElement.Description | TimeItElement.ProgressCount
                | TimeItElement.Time | TimeItElement.Rate
        },
            callback: Callback))
        {
            Thread.Sleep(100);
        }
    }

    static void Demo4()
    {
        var yaap = Range(0, 2000).TimeIt(settings: new TimeItSettings
        {
            Description = "detect stalls",
            Width = 100,
            SmoothingFactor = 0.5,
        },
        callback: Callback);

        foreach (var i in yaap)
        {
            if (i == 900)
            {
                yaap.State = TimeItState.Paused;
                Thread.Sleep(5000);
                yaap.State = TimeItState.Running;
                continue;
            }

            if (i == 1900)
            {
                yaap.State = TimeItState.Stalled;
                Thread.Sleep(5000);
                yaap.State = TimeItState.Running;
                continue;
            }

            Thread.Sleep(10);
        }
    }

    static void Demo5()
    {
        using (var mre = new ManualResetEvent(false))
        using (var allReady = new CountdownEvent(10))
        {
            var threads = Range(0, 10).Select(ti => new Thread(() =>
            {
                var r = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
                var y = Range(0, 200).TimeIt(settings: new TimeItSettings
                { Description = $"thread{ti}" },
                callback: Callback);
                allReady.Signal();
                mre.WaitOne();
                foreach (var _ in y)
                {
                    Thread.Sleep(r.Next(90, 110) / (ti + 1));
                }
            })).ToList();

            foreach (var t in threads)
            {
                t.Start();
            }

            allReady.Wait();

            mre.Set();
            foreach (var t in threads)
            {
                t.Join();
            }
        }
    }

    static void Demo6()
    {
        var rnd = new Random();
        foreach (var i in Range(0, 30232).TimeIt(settings: new TimeItSettings
        {
            Description = "many items",
            Width = 100,
            Elements = TimeItElement.Description | TimeItElement.ProgressCount
                | TimeItElement.Time | TimeItElement.Rate
        },
            callback: Callback))
        {
            Thread.Sleep(rnd.Next(100, 300));
        }
    }

    static void Main(string[] args)
    {
        var demos = new Action[] { /*Demo1, Demo2, Demo3, Demo4, Demo5,*/ Demo6 };
        var startDemo = args.Length > 0
            ? (int.TryParse(args[0], out var tmp) ? tmp : 1)
            : 1;
        var lastDemo = args.Length > 0 ? startDemo : demos.Length;

        for (var i = startDemo - 1; i < lastDemo; i++)
        {
            demos[i]();
        }
    }
}