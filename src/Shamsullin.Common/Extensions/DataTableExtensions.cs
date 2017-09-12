using System.Collections.Generic;
using System.Data;

namespace Shamsullin.Common.Extensions
{
    public static class DataTableExtensions
    {
        public static List<Dictionary<string, object>> ToJsonList(this DataTable table)
        {
            return RowsToDictionary(table);
        }

        private static List<Dictionary<string, object>> RowsToDictionary(DataTable table)
        {
            var objs = new List<Dictionary<string, object>>();
            foreach (DataRow dr in table.Rows)
            {
                var drow = new Dictionary<string, object>();
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    drow.Add(table.Columns[i].ColumnName, dr[i]);
                }
                objs.Add(drow);
            }

            return objs;
        }
    }
}