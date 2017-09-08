using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shamsullin.Common.Extensions
{
    /// <summary>
    /// Extensions for LINQ methods.
    /// </summary>
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            var arr = items.ToArrayEx();
            foreach (var local in arr)
            {
                action(local);
            }

            return arr;
        }

        public static T[] ForEach<T>(this T[] items, Action<T> action)
        {
            foreach (var local in items)
            {
                action(local);
            }

            return items;
        }

        /// <summary>
        /// Any-extension which handles null-pointers.
        /// </summary>
        public static bool AnyEx<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        /// <summary>
        /// Count-extension which handles null-pointers.
        /// </summary>
        public static int CountEx<T>(this IEnumerable<T> items)
        {
            return items == null ? 0 : items.Count();
        }

        /// <summary>
        /// Count-extension which handles null-pointers.
        /// </summary>
        public static int CountEx<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return items == null ? 0 : items.Count(predicate);
        }

        /// <summary>
        /// First-extension which handles null-pointers.
        /// </summary>
        public static T FirstEx<T>(this IEnumerable<T> source)
        {
            var enumerable = source as T[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.First();
            return default(T);
        }

        /// <summary>
        /// Where-extension which handles null-pointers.
        /// </summary>
        public static IEnumerable<T> WhereEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var enumerable = source as T[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Where(predicate);
            return new List<T>();
        }

        /// <summary>
        /// Count-extension which handles null-pointers and returns an array.
        /// </summary>
        public static T[] WhereArray<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.WhereEx(predicate).ToArrayEx();
        }

        /// <summary>
        /// SingleOrDefault-extension which handles null-pointers.
        /// </summary>
        public static T SingleOrDefaultEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var enumerable = source as T[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.SingleOrDefault(predicate);
            return default(T);
        }

        /// <summary>
        /// Select-extension which handles null-pointers.
        /// </summary>
        public static IEnumerable<TResult> SelectEx<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            var enumerable = source as TSource[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Select(selector);
            return new List<TResult>();
        }

        /// <summary>
        /// Select-extension which handles null-pointers and returns a list.
        /// </summary>
        public static List<TResult> SelectList<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            return source.SelectEx(selector).ToListEx();
        }

        /// <summary>
        /// Select-extension which handles null-pointers and returns an array.
        /// </summary>
        public static TResult[] SelectArray<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            return source.SelectEx(selector).ToArrayEx();
        }

        /// <summary>
        /// Union-extension which handles null-pointers and returns a list.
        /// </summary>
        public static T[] UnionArray<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstArr = first.ToArrayEx();
            var secondArr = second.ToArrayEx();
            var result = firstArr.Union(secondArr).ToArray();
            return result;
        }

        /// <summary>
        /// Select-extension which handles null-pointers.
        /// </summary>
        public static IEnumerable<TResult> SelectOrNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source?.Select(selector);
        }

        /// <summary>
        /// ToArray-extension which handles null-pointers.
        /// </summary>
        public static T[] ToArrayOrNull<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return null;
            }

            return source.ToArray();
        }

        /// <summary>
        /// Min-extension which handles null-pointers.
        /// </summary>
        public static TResult MinEx<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            var enumerable = source as TSource[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Min(selector);
            return default(TResult);
        }

        /// <summary>
        /// Max-extension which handles null-pointers.
        /// </summary>
        public static TResult MaxEx<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            var enumerable = source as TSource[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Max(selector);
            return default(TResult);
        }

        /// <summary>
        /// Average-extension which handles null-pointers.
        /// </summary>
        public static double AverageEx<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            var enumerable = source as TSource[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Average(selector);
            return 0;
        }

        /// <summary>
        /// Aggregate-extension which handles null-pointers.
        /// </summary>
        public static TSource AggregateEx<TSource>(this IEnumerable<TSource> source,
            Func<TSource, TSource, TSource> func)
        {
            var enumerable = source as TSource[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Aggregate(func);
            return default(TSource);
        }

        /// <summary>
        /// ToList-extension which handles null-pointers.
        /// </summary>
        public static List<T> ToListEx<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return new List<T>();
            return enumerable.ToList();
        }

        /// <summary>
        /// ToArray-extension which handles null-pointers.
        /// </summary>
        public static T[] ToArrayEx<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return new T[0];
            }

            return enumerable.ToArray();
        }

        /// <summary>
        /// Distinct-extension which handles null-pointers.
        /// </summary>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        public static IEnumerable<T> DistinctEx<T>(this IEnumerable<T> source)
        {
            var enumerable = source as T[] ?? source.ToArrayEx();
            if (enumerable.Any()) return enumerable.Distinct();
            return new T[0];
        }

        /// <summary>
        /// Distinct-extension which handles null-pointers and returns the specified function.
        /// </summary>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="keySelector">A transform function to apply to each element.</param>
        public static IEnumerable<TKey> DistinctFor<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.SelectEx(keySelector).DistinctEx();
        }

        /// <summary>
        /// Distinct-extension which handles null-pointers and returns.
        /// </summary>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="keySelector">A function to be used for comparison.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var keys = new HashSet<TKey>();
            return source.Where(x => keys.Add(keySelector(x)));
        }

        /// <summary>
        /// Execute for-each loop in parallel way.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="action">The action.</param>
        /// <param name="maxThreadsCount">The maximum number of threads.</param>
        /// <exception cref="ArgumentOutOfRangeException">maxThreadsCount</exception>
        public static void ForEachParallel<T>(this IEnumerable<T> items, Action<T> action, int maxThreadsCount = 64)
        {
            if (maxThreadsCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxThreadsCount));
            }

            if (maxThreadsCount == 1)
            {
                items.ForEach(action);
                return;
            }

            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<T>(items);
            for (var i = 0; i < Math.Min(maxThreadsCount, queue.Count); i++)
            {
                tasks.Add(Task.Factory.StartNew(x =>
                {
                    T item;
                    while (queue.TryDequeue(out item))
                    {
                        action(item);
                    }
                }, null));
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Execute for-each loop in parallel way using batches.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="action">The action.</param>
        /// <param name="maxThreadsCount">The maximum number of threads.</param>
        public static void ForEachParallelBatches<T>(this IEnumerable<T> items, Action<T> action, int maxThreadsCount = 64)
        {
            var listArr = items.ToArrayEx();
            var count = listArr.Length;
            if (count == 0) return;

            var chunkLength = Math.Ceiling((decimal) count/maxThreadsCount).ToInt();
            var threadsCount = Math.Ceiling((decimal) count/chunkLength).ToInt();
            int[] pendingThreads = {threadsCount};
            var wait = new AutoResetEvent(false);

            for (var i = 0; i < threadsCount; i++)
            {
                var chunk = listArr.Skip(i*chunkLength).Take(chunkLength);
                ThreadPool.QueueUserWorkItem(x =>
                {
                    foreach (var item in (IEnumerable<T>) x) action(item);
                    Interlocked.Decrement(ref pendingThreads[0]);
                    if (pendingThreads[0] == 0) wait.Set();
                }, chunk);
            }

            wait.WaitOne();
        }
    }
}