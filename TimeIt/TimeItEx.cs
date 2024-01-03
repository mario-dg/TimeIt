namespace TimeIt;

/// <summary>
/// A static extension class that provides <see cref="IEnumerable{T}"/><see
/// cref="TimeItEnumerable{T}"/> wrappers
/// </summary>
public static class TimeItEx
{
    /// <summary>
    /// Wrap the provided <see cref="IEnumerable{T}"/> with a <see cref="TimeItEnumerable{T}"/> object
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate</typeparam>
    /// <param name="e">The <see cref="IEnumerable{T}"/> instance to wrap</param>
    /// <param name="total">
    /// The (optional)total number of elements of the wrapped <see cref="IEnumerable{T}"/> that will
    /// be enumerated
    /// </param>
    /// <param name="initialProgress">The (optional) initial progress value</param>
    /// <param name="settings">The (optional) visual settings overrides</param>
    /// <param name="callback">
    /// The (optional) callback that allows the output to be bound to an observable property
    /// </param>
    /// <returns>
    /// The newly instantiated <see cref="TimeItEnumerable{T}"/> wrapping the provided <see cref="IEnumerable{T}"/>
    /// </returns>
    public static TimeItEnumerable<T> TimeIt<T>(this IEnumerable<T> e, long total = -1, long initialProgress = 0, TimeItSettings? settings = null, Action<string>? callback = null) =>
    new(e, total, initialProgress, settings, callback);
}