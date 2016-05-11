using System;
using System.Data;

namespace Shamsullin.Common.Extensions
{
    public static class DataReaderExtensions
    {
	    public static DataTable ToDataTable(this IDataReader reader)
	    {
			var result = new DataTable();
			result.Load(reader);
			return result;
	    }

	    public static bool HasColumn(this IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            
            return false;
        }

	    public static object GetValue(this IDataReader reader, string columnName)
	    {
		    if (reader.HasColumn(columnName))
		    {
			    try
			    {
				    return reader[columnName];
			    }
			    catch
			    {
				    return null;
			    }
		    }
		    return null;
	    }

	    public static T GetValue<T>(this IDataReader reader, string columnName)
        {
            return GetValue(reader, columnName).To<T>();
        }

		[Obsolete]
		public static float ReadFloatPointValue(this IDataReader reader, string fieldName)
		{
			if (reader[fieldName] is DBNull) return 0;
			return float.Parse(reader[fieldName].ToString());
		}
    }
}
