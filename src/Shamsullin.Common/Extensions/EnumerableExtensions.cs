using System.Collections.Generic;
using System.Linq;

namespace Shamsullin.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || collection.Any() == false;
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any();
        }

        public static IEnumerable<T> ToListOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection.IsEmpty())
                return Enumerable.Empty<T>();

            return collection.ToList();
        }

        public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
        {
            var toReturn = new List<T>(max);

            foreach (var item in source)
            {
                toReturn.Add(item);

                if (toReturn.Count == max)
                {
                    yield return toReturn;
                    toReturn = new List<T>(max);
                }
            }

            if (toReturn.Any())
                yield return toReturn;
        }

        public static int LastIndexOf<T>(this IEnumerable<T> input, T seed)
        {
            if (input is List<T>)
                return ((List<T>) input).LastIndexOf(seed);

            int index = -1;
            int counter = 0;
            foreach (var item in input)
            {
                if (item.Equals(seed))
                    index = counter;
                counter++;
            }
            return index;
        }
    }
}