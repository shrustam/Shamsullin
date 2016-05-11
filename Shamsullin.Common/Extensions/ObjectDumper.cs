using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Shamsullin.Common.Extensions
{
	public static class DumperExtension
	{
		public static string Dump(this object obj, int depth = 4, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			return ObjectDumper.Dump(obj, depth, bindingFlags);
		}
	}

	public class ObjectDumper
	{
		private readonly List<int> _hashListOfFoundElements = new List<int>();
		private readonly StringBuilder _stringBuilder = new StringBuilder();
		private int _level;

		public static string Dump(object element, int depth,
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			return new ObjectDumper().DumpElement(element, 8, depth, bindingFlags);
		}

		private string DumpElement(object element, int indentSize, int depth, BindingFlags bindingFlags)
		{
			if (_level > depth) return string.Empty;

			if (element == null || element is ValueType || element is string)
				Write(FormatValue(element), indentSize);
			else
			{
				var objectType = element.GetType();
				if (!typeof (IEnumerable).IsAssignableFrom(objectType))
				{
					Write("{{{0}}}", indentSize, objectType.FullName);
					_hashListOfFoundElements.Add(element.GetHashCode());
					_level++;
				}

				var enumerableElement = element as IEnumerable;
				if (enumerableElement != null)
				{
					foreach (var item in enumerableElement)
					{
						if (item is IEnumerable && !(item is string))
						{
							_level++;
							DumpElement(item, indentSize, depth, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
							_level--;
						}
						else
						{
							if (!AlreadyTouched(item))
								DumpElement(item, indentSize, depth, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
							else
								Write("{{{0}}} <-- bidirectional reference found", indentSize, item.GetType().FullName);
						}
					}
				}
				else
				{
					var members = element.GetType().GetMembers(bindingFlags)
						.Where(memberInfo => memberInfo.Name != "LockItem")
						.OrderBy(memberInfo => memberInfo.Name)
						.ToArray();

					foreach (var memberInfo in members)
					{
						var fieldInfo = memberInfo as FieldInfo;
						var propertyInfo = memberInfo as PropertyInfo;
						if (fieldInfo == null && propertyInfo == null) continue;

						Type type;
						if (fieldInfo != null)
						{
							if (fieldInfo.Name.Contains("k__BackingField")) continue;
							type = fieldInfo.FieldType;
						}
						else
						{
							type = propertyInfo.PropertyType;
						}

						object value;
						try
						{
							value = fieldInfo != null ? fieldInfo.GetValue(element) : propertyInfo.GetValue(element, null);
						}
						catch (Exception e)
						{
							value = string.Format("{0}: {1}", e.GetType().FullName, e.Message);
						}

						if (value == null || type.IsValueType || type == typeof (string))
							Write("{0}: {1}", indentSize, memberInfo.Name, FormatValue(value));
						else
						{
							_level++;
							if (_level <= depth)
							{
								var isEnumerable = typeof (IEnumerable).IsAssignableFrom(type);
								Write("{0}: {1}", indentSize, memberInfo.Name, isEnumerable ? "( )" : "{ }");

								var alreadyTouched = !isEnumerable && AlreadyTouched(value);
								if (!alreadyTouched)
									DumpElement(value, indentSize, depth, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
								else
									Write("{{{0}}} <-- bidirectional reference found", indentSize, value.GetType().FullName);
							}

							_level--;
						}
					}
				}

				if (!typeof (IEnumerable).IsAssignableFrom(objectType)) _level--;
			}

			return _stringBuilder.ToString();
		}

		private bool AlreadyTouched(object value)
		{
			if (value == null) return true;
			var hash = value.GetHashCode();
			return _hashListOfFoundElements.Any(t => t == hash);
		}

		private void Write(string value, int indentSize, params object[] args)
		{
			var space = new string(' ', _level*indentSize);
			if (args != null) value = string.Format(value, args);
			_stringBuilder.AppendLine(space + value);
		}

		private static string FormatValue(object o)
		{
			if (o == null) return "null";
			if (o is DateTime) return ((DateTime) o).ToShortDateString();
			if (o is string) return string.Format("\"{0}\"", o);
			if (o is int) return ((int) o).ToString(CultureInfo.InvariantCulture);
			if (o is float) return ((float) o).ToString(CultureInfo.InvariantCulture);
			if (o is double) return ((double) o).ToString(CultureInfo.InvariantCulture);
			if (o is decimal) return ((decimal) o).ToString(CultureInfo.InvariantCulture);
			if (o is ValueType) return o.ToString();
			if (o is IEnumerable) return "( )";
			return "{ }";
		}
	}
}