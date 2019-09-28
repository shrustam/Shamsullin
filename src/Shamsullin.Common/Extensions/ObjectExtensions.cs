using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Shamsullin.Common.Extensions
{
    /// <summary>
    /// Extension applicable for all objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns the default value if the input object is null.
        /// </summary>
        /// <param name="this">The input object.</param>
        /// <param name="func">The function to apply.</param>
        /// <returns>Results of function or default value</returns>
        public static TResult With<T, TResult>(this T @this, Func<T, TResult> func) where T : class
        {
            if (@this == null) return default(TResult);
            return func(@this);
        }

        /// <summary>
        /// Checks if one object is greater than another.
        /// </summary>
        /// <param name="value1">The object 1.</param>
        /// <param name="value2">The object 2.</param>
        /// <returns>Result of the comparison</returns>
        public static bool GreaterThan(this object value1, object value2)
        {
            if (value1 == null) return false;
            if (value2 == null) return true;
            var cValue1 = (IComparable) value1.To(value1.GetType());
            var cValue2 = (IComparable) value2.To(value1.GetType());
            return cValue1.CompareTo(cValue2) > 0;
        }

        /// <summary>
        /// Checks if one object is greater or equal than another.
        /// </summary>
        /// <param name="value1">The object 1.</param>
        /// <param name="value2">The object 2.</param>
        /// <returns>Result of the comparison</returns>
        public static bool GreaterOrEqual(this object value1, object value2)
        {
            if (value1 == null) return false;
            if (value2 == null) return true;
            var cValue1 = (IComparable) value1.To(value1.GetType());
            var cValue2 = (IComparable) value2.To(value1.GetType());
            return cValue1.CompareTo(cValue2) >= 0;
        }

        /// <summary>
        /// Checks if one object is less than another.
        /// </summary>
        /// <param name="value1">The object 1.</param>
        /// <param name="value2">The object 2.</param>
        /// <returns>Result of the comparison</returns>
        public static bool LessThan(this object value1, object value2)
        {
            if (value1 == null) return true;
            if (value2 == null) return false;
            var cValue1 = (IComparable) value1.To(value1.GetType());
            var cValue2 = (IComparable) value2.To(value1.GetType());
            return cValue1.CompareTo(cValue2) < 0;
        }

        /// <summary>
        /// Checks if one object is less or equal than another.
        /// </summary>
        /// <param name="value1">The object 1.</param>
        /// <param name="value2">The object 2.</param>
        /// <returns>Result of the comparison</returns>
        public static bool LessOrEqual(this object value1, object value2)
        {
            if (value1 == null) return true;
            if (value2 == null) return false;
            var cValue1 = (IComparable) value1.To(value1.GetType());
            var cValue2 = (IComparable) value2.To(value1.GetType());
            return cValue1.CompareTo(cValue2) <= 0;
        }

        /// <summary>
        /// Deeply clones the specified object using serialization.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Deeply cloned object</returns>
        /// <exception cref="ArgumentException">The type must be serializable. - source</exception>
        public static T Clone<T>(this T source)
        {
            if (!typeof (T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        /// <typeparam name="T">The destination generic type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Converted value</returns>
        public static T To<T>(this object obj)
        {
            return obj.To(default(T));
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        /// <typeparam name="T">The destination generic type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Converted value</returns>
        public static T To<T>(this object obj, T defaultValue)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return defaultValue;
            }
            if (obj is T)
            {
                return (T) obj;
            }
            var type = typeof (T);
            if (type == typeof (string))
            {
                return (T) obj;
            }
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return To(obj, defaultValue, underlyingType);
            }
            return To(obj, defaultValue, type);
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The destination type.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Converted value</returns>
        public static object To(this object obj, Type type, object defaultValue)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return defaultValue;
            }
            if (obj.GetType() == type)
            {
                return obj;
            }
            if (type == typeof (string))
            {
                return obj;
            }
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return To(obj, defaultValue, underlyingType);
            }
            return To(obj, defaultValue, type);
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The destination type.</param>
        /// <returns>Converted value</returns>
        public static object To(this object obj, Type type)
        {
            if (obj == null || obj == DBNull.Value)
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
                return null;
            }
            if (obj.GetType() == type)
            {
                return obj;
            }
            if (type == typeof (string))
            {
                return obj;
            }
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return To(obj, new object(), underlyingType);
            }
            return To(obj, Activator.CreateInstance(type), type);
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        /// <typeparam name="T">The destination generic type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="type">The destination type.</param>
        /// <returns>Converted value </returns>
        private static T To<T>(object obj, T defaultValue, Type type)
        {
            if (type.IsEnum)
            {
                if (obj is decimal)
                {
                    return (T) Enum.Parse(type, obj.ToString());
                }
                if (obj is string)
                {
                    return (T) Enum.Parse(type, (string) obj);
                }
                if (obj is long)
                {
                    return (T) Enum.Parse(type, obj.ToString());
                }
                if (Enum.IsDefined(type, obj))
                {
                    return (T) Enum.Parse(type, obj.ToString());
                }
                return defaultValue;
            }
            try
            {
                if (type == typeof (Guid))
                {
                    return (T) (object) new Guid(obj.ToString());
                }
                if (type == typeof (float))
                {
                    return (T) (object) obj.ToSingle();
                }
                if (type == typeof (decimal))
                {
                    return (T) (object) obj.ToDecimal();
                }

                return (T) Convert.ChangeType(obj, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Converts an object to decimal. If conversion is impossible returns 0.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Converted value</returns>
        public static decimal ToDecimal(this object obj)
        {
            if (obj == null) return 0;
            if (obj is decimal) return (decimal)obj;
            if (obj is double) return new decimal((double)obj);
            if (obj is int) return (int)obj;
            if (obj is long) return (long)obj;

            var valueStr = obj.ToString();
            if (string.IsNullOrWhiteSpace(valueStr)) return 0;
            var result = new decimal(double.Parse(obj.ToString()));
            return result;
        }

        /// <summary>
        /// Converts an object to single. If conversion is impossible returns 0.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Converted value</returns>
        public static float ToSingle(this object obj)
        {
            if (obj != null)
            {
                float sinValue;
                if (obj is float)
                {
                    return (float) obj;
                }
                if (float.TryParse(obj.ToString().Replace(
                    CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                    CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                    out sinValue))
                {
                    return sinValue;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Converts an object to single. If conversion is impossible returns 0.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Converted value</returns>
        public static double ToDouble(this object obj)
        {
            if (obj != null)
            {
                double dbValue;
                if (obj is double)
                {
                    return (double) obj;
                }
                if (
                    double.TryParse(
                        obj.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                            CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                        out dbValue))
                {
                    return dbValue;
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Converts an object to bool. If conversion is impossible returns false.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Converted value</returns>
        public static bool ToBool(this object obj)
        {
            bool boolValue;
            int intValue;
            if (obj == null || obj == DBNull.Value)
            {
                return false;
            }
            if (obj is bool)
            {
                return (bool) obj;
            }
            if (bool.TryParse(obj.ToString(), out boolValue))
            {
                return boolValue;
            }
            return (int.TryParse(obj.ToString(), out intValue) && (intValue != 0));
        }

        /// <summary>
        /// Converts the string representation of a number to an integer.
        /// </summary>
        /// <param name="obj">The input object.</param>
        /// <param name="deft">The default value.</param>
        /// <returns>Converted value</returns>
        public static int ToInt(this object obj, int deft = 0)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                int intValue;
                if (obj is int)
                {
                    return (int) obj;
                }

                IFormatProvider provider = CultureInfo.InvariantCulture;
                var objectAsString = Convert.ToString(obj, provider);
                if (int.TryParse(objectAsString, NumberStyles.Any, provider, out intValue))
                {
                    return intValue;
                }
            }
            return deft;
        }

        /// <summary>
        /// Converts specified object to the string. Empty string returns in case of null value.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Converted value</returns>
        public static string ToStr(this object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return string.Empty;
            }
            if (obj is string)
            {
                return (string) obj;
            }
            return obj.ToString();
        }
    }
}