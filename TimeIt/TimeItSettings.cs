using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeIt;

/// <summary>
/// An enumeration representing the various TimeIt progress bar elements
/// </summary>
public enum TimeItElement
{
    /// <summary>
    /// The description prefix visual TimeIt element
    /// </summary>
    Description = 0x1,

    /// <summary>
    /// The numerical percent (e.g. 99%) visual TimeIt element
    /// </summary>
    ProgressPercent = 0x2,

    /// <summary>
    /// The graphical progress bar visual TimeIt element
    /// </summary>
    ProgressBar = 0x4,

    /// <summary>
    /// The progress count (e.g. 199/200) visual TimeIt elements
    /// </summary>
    ProgressCount = 0x8,

    /// <summary>
    /// The elapsed/total time visual TimeIt elements
    /// </summary>
    Time = 0x10,

    /// <summary>
    /// The rate visual TimeIt element
    /// </summary>
    Rate = 0x20,

    /// <summary>
    /// A special or'd value representing all elements of TimeIt
    /// </summary>
    All = Description | ProgressPercent | ProgressBar | ProgressCount | Time | Rate,
}

/// <summary>
/// The current state of the <see cref="TimeIt"/> instance
/// </summary>
public enum TimeItState
{
    /// <summary>
    /// The TimeIt instance is running (progressing)
    /// </summary>
    Running,

    /// <summary>
    /// The TimeIt instance is paused
    /// </summary>
    Paused,

    /// <summary>
    /// The TimeIt instance is stalled
    /// </summary>
    Stalled,
}

public class TimeItSettings
{
    /// <summary>
    /// Specifies a prefix for the progress bar text that should be used to uniquely identify the
    /// progress bar meaning/content to the user.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A flags or'd value specifying which visual elements will be presented to the user
    /// </summary>
    public TimeItElement Elements { get; set; } = TimeItElement.All;

    /// <summary>
    /// Leave the progress bar visually on screen once it is done/closed
    /// </summary>
    public bool Leave { get; set; } = true;

    /// <summary>
    /// If set, will be used to scale the <see cref="TimeIt.Progress"/> and <see
    /// cref="TimeIt.Total"/> values, using the International System of Units standard. (kilo, mega, etc.)
    /// </summary>
    public bool MetricAbbrevations { get; set; }

    /// <summary>
    /// Exponential moving average smoothing factor for speed estimates. Ranges from 0 (average
    /// speed) to 1 (current/instantaneous speed) [default: 0.3].
    /// </summary>
    public double SmoothingFactor { get; set; }

    /// <summary>
    /// The select visual style for the progress bar, only taken into account when <see
    /// cref="UseASCII"/> is set to false (which is the default)
    /// </summary>
    public TimeItBarStyle Style { get; set; }

    /// <summary>
    /// used to name the unit unit of progress. [default: 'it']
    /// </summary>
    public string UnitName { get; set; } = "it";

    /// <summary>
    /// If set, and <see cref="MetricAbbreviations"/> is set to false, will be used to scale the
    /// <see cref="TimeIt.Progress"/> and <see cref="TimeIt.Total"/> values
    /// </summary>
    public double? UnitScale { get; set; }

    /// <summary>
    /// Use only ASCII charchters (notably the '#' charchter as the progress bar 'progress' glyph
    /// </summary>
    public bool UseASCII { get; set; }

    /// <summary>
    /// Constrain the progress bar to a specific width, when not specified, the progress bar will
    /// default to 150 characters wide
    /// </summary>
    public int? Width { get; set; }
}