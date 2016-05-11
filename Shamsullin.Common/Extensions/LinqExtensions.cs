using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Shamsullin.Common.Extensions
{
	/// <summary>
	/// Расширения над стандартными методами Linq2Object.
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
		/// Расширение для метода Any, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static bool AnyEx<T>(this IEnumerable<T> items)
		{
			return items != null && items.Any();
		}

		/// <summary>
		/// Расширение для метода Count, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static int CountEx<T>(this IEnumerable<T> items)
		{
			return items == null ? 0 : items.Count();
		}

		/// <summary>
		/// Расширение для метода Count, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static int CountEx<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			return items == null ? 0 : items.Count(predicate);
		}

		/// <summary>
		/// Расширение для метода First, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static T FirstEx<T>(this IEnumerable<T> source)
		{
			var enumerable = source as T[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.First();
			return default(T);
		}

		/// <summary>
		/// Расширение для метода Where, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static IEnumerable<T> WhereEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			var enumerable = source as T[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Where(predicate);
			return new List<T>();
		}

		public static T[] WhereArray<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			return source.WhereEx(predicate).ToArrayEx();
		}

		/// <summary>
		/// Расширение для метода SingleOrDefault, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static T SingleOrDefaultEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			var enumerable = source as T[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.SingleOrDefault(predicate);
			return default(T);
		}

		/// <summary>
		/// Расширение для метода Select, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static IEnumerable<TResult> SelectEx<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			var enumerable = source as TSource[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Select(selector);
			return new List<TResult>();
		}

		public static List<TResult> SelectList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			return source.SelectEx(selector).ToListEx();
		}

		public static TResult[] SelectArray<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			return source.SelectEx(selector).ToArrayEx();
		}

		public static T[] UnionArray<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			return first.Union(second).ToArrayEx();
		}

		/// <summary>
		/// Расширение для метода Select, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static IEnumerable<TResult> SelectOrNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			if (source == null)
			{
				return null;
			}

			return source.Select(selector);
		}

		/// <summary>
		/// Расширение для метода ToArray, которое обрабатывает в т.ч. пустые коллекции.
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
		/// Расширение для метода Min, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static TResult MinEx<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			var enumerable = source as TSource[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Min(selector);
			return default(TResult);
		}

		/// <summary>
		/// Расширение для метода Max, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static TResult MaxEx<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			var enumerable = source as TSource[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Max(selector);
			return default(TResult);
		}

		/// <summary>
		/// Расширение для метода Average, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static double AverageEx<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			var enumerable = source as TSource[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Average(selector);
			return 0;
		}

		/// <summary>
		/// Расширение для метода Aggregate, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static TSource AggregateEx<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
		{
			var enumerable = source as TSource[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Aggregate(func);
			return default(TSource);
		}

		/// <summary>
		/// Расширение для метода ToList, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static List<T> ToListEx<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable == null) return new List<T>();
			return enumerable.ToList();
		}

		/// <summary>
		/// Расширение для метода ToArray, которое обрабатывает в т.ч. пустые коллекции.
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
		/// Расширение для метода Distinct, которое обрабатывает в т.ч. пустые коллекции.
		/// </summary>
		public static IEnumerable<T> DistinctEx<T>(this IEnumerable<T> source)
		{
			var enumerable = source as T[] ?? source.ToArrayEx();
			if (enumerable.Any()) return enumerable.Distinct();
			return new T[0];
		}

		/// <summary>
		/// Расширение для метода Distinct
		/// </summary>
		public static IEnumerable<TKey> DistinctFor<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.SelectEx(keySelector).DistinctEx();
		}

		/// <summary>
		/// Расширение для метода Distinct
		/// </summary>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var keys = new HashSet<TKey>();
			return source.Where(x => keys.Add(keySelector(x)));
		}

		/// <summary>
		/// Enumerates through each item in a list in parallel.
		/// </summary>
		public static void ForEachParallel<T>(this IEnumerable<T> list, Action<T> action, int maxThreadsCount = 64)
		{
			var listArr = list.ToArrayEx();
			var count = listArr.Count();
			if (count == 0) return;

			var chunkLength = Math.Ceiling((decimal)count / maxThreadsCount).ToInt();
			var threadsCount = Math.Ceiling((decimal)count / chunkLength).ToInt();
			int[] pendingThreads = { threadsCount };
			var wait = new AutoResetEvent(false);

			for (var i = 0; i < threadsCount; i++)
			{
				var chunk = listArr.Skip(i * chunkLength).Take(chunkLength);
				ThreadPool.QueueUserWorkItem(x =>
					{
						foreach (var item in (IEnumerable<T>)x) action(item);
						Interlocked.Decrement(ref pendingThreads[0]);
						if (pendingThreads[0] == 0) wait.Set();
					}, chunk);
			}

			wait.WaitOne();
		}
	}
}
