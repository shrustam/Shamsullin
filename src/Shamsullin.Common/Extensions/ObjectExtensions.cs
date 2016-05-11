using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Shamsullin.Common.Extensions
{
    public static class ObjectExtensions
	{
		public static TResult With<T, TResult>(this T @this, Func<T, TResult> func) where T : class
		{
			if (@this == null) return default(TResult);
			return func(@this);
		}

		public static bool GreaterThan(this object value1, object value2)
		{
			if (value1 == null) return false;
			if (value2 == null) return true;
			var cValue1 = (IComparable)value1.To(value1.GetType());
			var cValue2 = (IComparable)value2.To(value1.GetType());
			return cValue1.CompareTo(cValue2) > 0;
		}

		public static bool GreaterOrEqual(this object value1, object value2)
		{
			if (value1 == null) return false;
			if (value2 == null) return true;
			var cValue1 = (IComparable)value1.To(value1.GetType());
			var cValue2 = (IComparable)value2.To(value1.GetType());
			return cValue1.CompareTo(cValue2) >= 0;
		}

		public static bool LessThan(this object value1, object value2)
		{
			if (value1 == null) return true;
			if (value2 == null) return false;
			var cValue1 = (IComparable)value1.To(value1.GetType());
			var cValue2 = (IComparable)value2.To(value1.GetType());
			return cValue1.CompareTo(cValue2) < 0;
		}

		public static bool LessOrEqual(this object value1, object value2)
		{
			if (value1 == null) return true;
			if (value2 == null) return false;
			var cValue1 = (IComparable)value1.To(value1.GetType());
			var cValue2 = (IComparable)value2.To(value1.GetType());
			return cValue1.CompareTo(cValue2) <= 0;
		}

	    public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
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
                return (T)formatter.Deserialize(stream);
            }
        }
        
        public static T To<T>(this object obj)
        {
            return obj.To(default(T));
        }

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
            if (type == typeof(string))
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

        public static object To(this object obj, Type type)
        {
            if (obj == null || obj == DBNull.Value)
			{
				if (type.IsValueType) return Activator.CreateInstance(type);
				return null;
            }
            if (obj.GetType() == type)
            {
                return obj;
            }
            if (type == typeof(string))
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
				if (type == typeof(decimal))
				{
					return (T)(object)obj.ToDecimal();
				}

			    return (T) Convert.ChangeType(obj, type);
		    }
		    catch
		    {
			    return defaultValue;
		    }
	    }

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

        public static decimal ToDecimal(this object obj)
        {
            if (obj != null)
            {
                decimal decValue;
                if (obj is decimal)
                {
                    return (decimal) obj;
                }
                if (
                    decimal.TryParse(
                        obj.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                        out decValue))
                {
                    return decValue;
                }
            }
            return 0M;
        }

        public static float ToSingle(this object obj)
        {
            if (obj != null)
            {
                float sinValue;
                if (obj is float)
                {
                    return (float)obj;
                }
                if (
                    float.TryParse(
                        obj.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                        out sinValue))
                {
                    return sinValue;
                }
            }
            return 0f;
        }

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

        public static int ToInt(this object obj)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                int intValue;
                if (obj is int)
                {
                    return (int) obj;
                }
                if (int.TryParse(obj.ToString(), out intValue))
                {
                    return intValue;
                }
            }
            return 0;
        }

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
