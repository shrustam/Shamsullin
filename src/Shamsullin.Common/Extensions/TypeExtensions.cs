using System;
using System.Linq;

namespace Shamsullin.Common.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsSubclassOfGeneric(this Type type, Type generic)
        {
            var toCheck = type;
            while (toCheck != null && toCheck != typeof (object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (cur.IsGenericType && generic.GetGenericTypeDefinition() == cur.GetGenericTypeDefinition())
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        public static bool IsNonAbstractSubclassOf(this Type type, Type baseType)
        {
            if (baseType.IsInterface)
            {
                return !type.IsAbstract && type.GetInterfaces().Any(x => x == baseType);
            }

            return !type.IsAbstract && type.IsSubclassOf(baseType);
        }

        public static bool IsNonAbstractSubclassOf<T>(this Type type)
        {
            return IsNonAbstractSubclassOf(type, typeof (T));
        }
    }
}