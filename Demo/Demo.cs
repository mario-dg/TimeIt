using System.Diagnostics;

using TimeIt;

using static System.Linq.Enumerable;

namespace Demo;

internal static class Demo
{
    static void Callback(string timeItString)
    {
        Debug.WriteLine(timeItString);
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

    static void Main(string[] args)
    {
        var demos = new Action[] { Demo1, Demo2, Demo3 };
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