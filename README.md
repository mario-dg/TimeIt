# TimeIt

TimeIt is a heavily adapted package of the straight up port of Yaap; a python venerable [tqdm](https://github.com/tqdm/tqdm) to .NET / CLR

The reason for this fork is that I wanted to use Yaap in a WPF project and needed the functionality of calculating the remaining time 
and rate of the progress. But I needed to be able to use it in a MVVM scenario, without console output, so
I removed all console output and added a `Action<string>` Callback, which is called with the progress string.

This allows users of this package to use it in a MVVM scenario, where the progress is displayed in a GUI.

## What does it do

Similar to Yaap, TimeIt can make .NET loops, `IEnumerable`s  and more show a smart progress meter.


The most dead simple way of starting with TimeIt is to add it via the nuget package and 

```c#
using TimeIt;

void printCallback(string progressString)
{
    // Do something with the progress string, e.g. print it
    Debug.WriteLine(progressString);
}

foreach (var i in Enumerable.Range(0, 1000).TimeIt(callback: printCallback)) {
    Thread.Sleep(10);
}
```

Will print the progress bar on each line like this:

```
76%|████████████████████████████         | 7568/10000 [00:07s<00:10s, 229.00it/s]
```

In a MVVM scenario, TimeIt could be used like following:

```c#
using TimeIt;

// Some property that implements INotifyPropertyChanged and is bound to the GUI
public string ProgressString { get; set; }

void bindingCallback(string progressString)
{
    ProgressString = progressString;
    OnPropertyChanged(nameof(ProgressString));
}

foreach (var i in Enumerable.Range(0, 1000).TimeIt(callback: bindingCallback)) {
    Thread.Sleep(10);
}
```

## What Else

Yaap has the following features:

* Easy wrapping of `IEnumerable<T>` with a Yaap progress bar
* Manual (non `IEnumetable<T>`) progress updates
* Low latency (~30ns) overhead imposed on the thread bumping the progress value
* Zero allocation (post construction) / Very little allocation during construction
* Elapsed time tracking
* Total Time Prediction
* Rate Prediction
* Metric Abbreviation for counts (K/M/G...)
* Nested / Multiple concurrent progress bars
* Butter Smooth Progress bars, by predicting the  progress from the rate
* Configurable Appearance:
  * Fancy Unicode / ASCII bars
  * Prefix text
  * Turn selected elements on/off
* Constant Width Progress Bars

## Docs

Full documentation is provided here

## Examples

See the [Demo](Demo) project for a fancy demo that covers most of what Yaap can do and how it can be optimized

You can either run the demo project with `dotnet run` to run all the demos sequentially or invoke specific demos with `dotnet run <n>` where `<n>` is the number of the demo to run...