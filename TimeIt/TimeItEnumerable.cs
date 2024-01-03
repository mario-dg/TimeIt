using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace TimeIt;

/// <summary>
/// Represents a TimeIt wrapped <see cref="IEnumerable{T}"/> object, where the enumerator progress
/// automatically changes the TimeIt visual representation without further need to manually update
/// the progress state
/// </summary>
/// <typeparam name="T">The type of objects to enumerate</typeparam>
public class TimeItEnumerable<T> : TimeIt, IEnumerable<T>
{
    private static Func<IEnumerable<T>, int> _cheapCount;
    private readonly IEnumerable<T> _enumerable;

    internal TimeItEnumerable(IEnumerable<T> e, long total = -1, long initialProgress = 0, TimeItSettings? settings = null, Action<string>? callback = null) :
        base(total != -1 ? total : GetCheapCount(e), initialProgress, settings, callback)
    {
        _enumerable = e;
    }

    /// <summary>
    /// Attempt to get a "cheap" count value for the <see cref="IEnumerable{T}"/>, where "cheap"
    /// means that the enumerable is never consumed no matter what
    /// </summary>
    /// <param name="source">The <see cref="IEnumerable{T}"/> object</param>
    /// <returns>The count value, or -1 in case the cheap count failed</returns>
    private static Func<IEnumerable<T>, int> CheapCountDelegate
    {
        get
        {
            return _cheapCount ??= GenerateGetCount();

            Func<IEnumerable<T>, int> GenerateGetCount()
            {
                var iilp = typeof(Enumerable).Assembly.GetType("System.Linq.IIListProvider`1");
                Debug.Assert(iilp != null);
                var iilpt = iilp.MakeGenericType(typeof(T));
                Debug.Assert(iilpt != null);
                var getCountMI = iilpt.GetMethod("GetCount", BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(bool) }, null);
                Debug.Assert(getCountMI != null);
                var param = Expression.Parameter(typeof(IEnumerable<T>));

                var castAndCall = Expression.Call(Expression.Convert(param, iilpt), getCountMI,
                    Expression.Constant(true));

                var body = Expression.Condition(Expression.TypeIs(param, iilpt), castAndCall,
                    Expression.Constant(-1));

                return Expression.Lambda<Func<IEnumerable<T>, int>>(body, new[] { param }).Compile();
            }
        }
    }

    /// <summary>
    /// Attempt to get a "cheap" count value for the <see cref="IEnumerable{T}"/>, where "cheap"
    /// means that the enumerable is never consumed no matter what
    /// </summary>
    /// <param name="source">The <see cref="IEnumerable{T}"/> object</param>
    /// <returns>The count value, or -1 in case the cheap count failed</returns>
    public static int GetCheapCount(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        switch (source)
        {
            case ICollection<T> collectionoft:
                return collectionoft.Count;

            case ICollection collection:
                return collection.Count;

            default:
                break;
        }

        return CheapCountDelegate(source);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>Returns an enumerator that iterates through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        // In the case of enumerable we can actually start ticking the elapsed clock at a later,
        // more precise, time, so lets do it...
        _sw.Restart();
        foreach (var t in _enumerable)
        {
            yield return t;
            Progress++;
        }
        Dispose();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}