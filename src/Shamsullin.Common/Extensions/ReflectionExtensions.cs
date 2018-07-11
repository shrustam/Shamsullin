using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Shamsullin.Common.Extensions
{
    public static class ReflectionExtensions
    {
        private static readonly Dictionary<string, PropertyInfo[]> Properties = new Dictionary<string, PropertyInfo[]>();

        public static PropertyInfo[] GetPropertiesEx(this Type type)
        {
            if (Properties.ContainsKey(type.FullName)) return Properties[type.FullName];
            var locker = string.Concat("GetPropertiesEx_", type.FullName);
            lock (locker.SyncRoot())
            {
                if (Properties.ContainsKey(type.FullName)) return Properties[type.FullName];
                var result = type.GetProperties();
                Properties.Add(type.FullName, result);
                return result;
            }
        }

        public static string GetPropertyName<T, TType>(Expression<Func<T, TType>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var operand = unaryExpression.Operand;
                return ((MemberExpression) operand).Member.Name;
            }

            throw new NotSupportedException();
        }

        public static string GetPropertyName<T, TType>(this T obj, Expression<Func<T, TType>> expression)
        {
            return GetPropertyName(expression);
        }

        /// <summary>
        /// E.g. AProperty.Items[5].Key
        /// </summary>
        public static object GetPropertyValue<T>(this T obj, string propertyName)
        {
            if (!propertyName.Contains("."))
            {
                return obj.GetPropertyValueSimple(propertyName);
            }

            object result = obj;
            var fullPath = propertyName.Split('.');
            foreach (var name in fullPath)
            {
                if (result == null) return null;
                if (name.Contains('[') && name.Contains(']'))
                {
                    var rgx = new Regex(@"([a-zA-Z]+?)\[([0-9]+?)]");
                    var matches = rgx.Matches(name);
                    var arrayName = matches[0].Groups[1].Value;
                    var index = matches[0].Groups[2].Value.ToInt();
                    var array = result.GetType()
                        .GetPropertiesEx()
                        .FirstOrDefault(x => x.Name == arrayName)
                        .With(x => x.GetValue(result, null)) as object[];
                    if (array == null) return null;
                    result = array[index];
                }
                else
                {
                    result = result.GetType()
                        .GetPropertiesEx()
                        .FirstOrDefault(x => x.Name == name)
                        .With(x => x.GetValue(result, null));
                }
            }

            return result;
        }

        private static object GetPropertyValueSimple<T>(this T obj, string propertyName)
        {
            var properties = typeof (T).GetPropertiesEx();
            return properties.FirstOrDefault(x => x.Name == propertyName).With(x => x.GetValue(obj, null));
        }

        public static object Map(this object source, Type destType)
        {
            if (source == null) return null;
            var result = Activator.CreateInstance(destType);
            var sourceProperties = source.GetType().GetPropertiesEx();
            var destProperties = destType.GetPropertiesEx();

            foreach (var destProperty in destProperties.WhereEx(x => x.SetMethod != null))
            {
                var sourceProperty =
                    sourceProperties.FirstOrDefault(x => x.Name.ToLower() == destProperty.Name.ToLower());
                if (sourceProperty != null)
                {
                    var value = sourceProperty.GetValue(source, null);
                    if (value == null) continue;
                    destProperty.SetValue(result, value, null);
                }
            }

            return result;
        }

        public static TDest Map<TDest>(this object source, Action<TDest> action = null) where TDest : class, new()
        {
            if (source == null) return null;
            var result = (TDest) source.Map(typeof (TDest));
            if (action != null) action(result);
            return result;
        }

        public static TDest Map<TDest>(this Dictionary<string, object> source, Action<TDest> action = null)
            where TDest : class, new()
        {
            if (source == null) return null;
            var result = new TDest();
            var destProperties = typeof (TDest).GetPropertiesEx();

            foreach (var destProperty in destProperties.WhereEx(x => x.SetMethod != null))
            {
                if (source.ContainsKey(destProperty.Name))
                {
                    var value = source[destProperty.Name];
                    if (value == null) continue;
                    destProperty.SetValue(result, value.To(destProperty.PropertyType), null);
                }
            }

            if (action != null) action(result);
            return result;
        }

        public static TDest MapFileds<TDest>(this object source, Action<TDest> action = null) where TDest : class, new()
        {
            if (source == null) return null;
            var result = new TDest();
            var sourceProperties = source.GetType().GetFields();
            var destProperties = typeof (TDest).GetFields();

            foreach (var destProperty in destProperties)
            {
                var sourceProperty =
                    sourceProperties.FirstOrDefault(x => x.Name.ToLower() == destProperty.Name.ToLower());
                if (sourceProperty != null)
                {
                    var value = sourceProperty.GetValue(source);
                    if (value == null) continue;
                    destProperty.SetValue(result, value);
                }
            }

            if (action != null) action(result);
            return result;
        }

        public static IList<T> Create<T>(this DataTable dataTable) where T : new()
        {
            return dataTable.Rows.Cast<DataRow>().SelectEx(x => x.Create<T>()).ToList();
        }

        public static T Create<T>(this DataRow dataRow, Action<T> action = null) where T : new()
        {
            var result = new T();
            foreach (var property in typeof (T).GetPropertiesEx().WhereEx(x => x.SetMethod != null))
            {
                if (dataRow.Table.Columns.Contains(property.Name))
                {
                    if (dataRow[property.Name] is DBNull) continue;
                    var value = dataRow[property.Name].To(property.PropertyType);
                    property.SetValue(result, value, null);
                }
            }

            if (action != null) action(result);
            return result;
        }

        public static T Create<T>(this IDataReader reader, Action<T> action = null) where T : new()
        {
            var result = new T();
            foreach (var property in typeof (T).GetPropertiesEx().WhereEx(x => x.SetMethod != null))
            {
                property.SetValue(result, reader.GetValue(property.Name).To(property.PropertyType), null);
            }

            if (action != null) action(result);
            return result;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> items, bool useSqlDateTime) where T : new()
        {
            var result = new DataTable();
            foreach (var property in typeof (T).GetPropertiesEx())
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                if (underlyingType != null)
                {
                    result.Columns.Add(property.Name, underlyingType);
                }
                else
                {
                    result.Columns.Add(property.Name, property.PropertyType);
                }
            }

            foreach (var item in items)
            {
                var row = result.NewRow();
                foreach (var property in typeof (T).GetPropertiesEx())
                {
                    var value = property.GetValue(item, null);
                    if (useSqlDateTime && value is DateTime)
                    {
                        row[property.Name] = ((DateTime) value) < SqlDateTime.MinValue.Value
                            ? SqlDateTime.MinValue.Value
                            : value;
                    }
                    else
                    {
                        row[property.Name] = value ?? DBNull.Value;
                    }
                }
                result.Rows.Add(row);
            }

            return result;
        }
    }
}