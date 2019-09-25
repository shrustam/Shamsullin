using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Shamsullin.Common.Extensions;

namespace Shamsullin.Common
{
    public abstract class BaseRepository
    {
        /// <summary>
        /// Имя строки подключения.
        /// </summary>
        protected abstract string ConnectionStringName { get; }

        public static string ApplicationName { get; set; }

        /// <summary>
        /// Строка подключения.
        /// </summary>
        protected static string GetConnectionString(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            if (connectionString.Contains("Application Name")) return connectionString;
            return $"{ConfigurationManager.ConnectionStrings[name].ConnectionString.TrimEnd(new[] {';'})};Application Name={ApplicationName}";
        }

        public virtual int ExecuteNonQuery(string query, params SqlParameter[] args)
        {
            using (var connection = GetConnection(ConnectionStringName))
            using (var command = CreateSqlCommand(connection, query))
            {
                command.Parameters.AddRange(args);
                return AdjustParameters(command).ExecuteNonQuery();
            }
        }

        public int InsertOrUpdate<T>(T record, string tableName, Expression<Func<T, object>>[] keyFields,
            params Expression<Func<T, object>>[] excludeFields)
        {
            var columns = new List<string>();
            var sqlParameters = new List<SqlParameter>();
            var keys = keyFields.SelectArray(x => record.GetPropertyName(x));
            var exclude = excludeFields.SelectArray(x => record.GetPropertyName(x));

            foreach (var property in typeof (T).GetProperties().WhereEx(x => !exclude.Contains(x.Name)))
            {
                columns.Add(property.Name);
                var value = property.GetValue(record, null);
                var param = string.Concat("@", property.Name);
                sqlParameters.Add(new SqlParameter(param, value));
            }

            var nonNullColumns =
                columns.WhereArray(x => sqlParameters.Any(y => y.ParameterName.TrimStart('@') == x && y.Value != null));
            var insertQuery = $"INSERT INTO {tableName} ({nonNullColumns.Aggregate((x, y) => x+","+y)}) VALUES ({nonNullColumns.Select(x => $"@{x}").Aggregate((x, y) => x+","+y)})";

            var updateQuery = $"UPDATE {tableName} SET {columns.Select(x => string.Format("{0}=@{0}", x)).AggregateEx((x, y) => x+","+y)} WHERE {keys.Select(x => string.Format("{0}=@{0}", x)).AggregateEx((x, y) => x+" AND "+y)}";

            var resultQuery = $"{updateQuery} IF @@ROWCOUNT=0 {insertQuery}";
            using (var connection = GetConnection(ConnectionStringName))
            using (var command = CreateSqlCommand(connection, resultQuery))
            {
                command.Parameters.AddRange(sqlParameters.ToArray());
                return AdjustParameters(command).ExecuteNonQuery();
            }
        }

        public int Insert<T>(T record, string tableName, params Expression<Func<T, object>>[] excludeFields)
        {
            var columns = new StringBuilder();
            var values = new StringBuilder();
            var sqlParameters = new List<SqlParameter>();
            var exclude = excludeFields.SelectArray(x => record.GetPropertyName(x));

            foreach (var property in typeof (T).GetProperties().WhereEx(x => !exclude.Contains(x.Name)))
            {
                var value = property.GetValue(record, null);
                if (value != null)
                {
                    columns.AppendFormat("{0},", property.Name);
                    var param = string.Concat("@", property.Name);
                    values.AppendFormat("{0},", param);
                    sqlParameters.Add(new SqlParameter(param, value));
                }
            }

            var query = $"INSERT INTO {tableName} ({columns.ToString().TrimEnd(new[] {','})}) VALUES ({values.ToString().TrimEnd(new[] {','})})";
            using (var connection = GetConnection(ConnectionStringName))
            using (var command = CreateSqlCommand(connection, query))
            {
                command.Parameters.AddRange(sqlParameters.ToArray());
                return AdjustParameters(command).ExecuteNonQuery();
            }
        }

        public static DataTable GetData(SqlCommand command, params SqlParameter[] args)
        {
            command.Parameters.AddRange(args);
            var dataAdapter = new SqlDataAdapter(AdjustParameters(command));
            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            var result = dataSet.Tables[0];
            return result;
        }

        public virtual DataTable GetData(string query)
        {
            using (var connection = GetConnection(ConnectionStringName))
            using (var command = CreateSqlCommand(connection, query))
            {
                return GetData(command);
            }
        }

        public virtual DataTable GetData(string queryFormat, params object[] args)
        {
            using (var connection = GetConnection(ConnectionStringName))
            using (var command = CreateSqlCommand(connection, string.Format(queryFormat, args)))
            {
                return GetData(command);
            }
        }

        public virtual DataTable GetData(string query, params SqlParameter[] args)
        {
            using (var connection = GetConnection(ConnectionStringName))
            using (var command = CreateSqlCommand(connection, query))
            {
                return GetData(command, args);
            }
        }

        public static IList<T> Create<T>(DataTable dataTable) where T : new()
        {
            return dataTable.Create<T>();
        }

        protected virtual SqlConnection GetConnection()
        {
            return GetConnection(ConnectionStringName);
        }

        protected static SqlConnection GetConnection(string connectionStringName)
        {
            var connection = new SqlConnection(GetConnectionString(connectionStringName));
            if (connection.State != ConnectionState.Open) connection.Open();
            return connection;
        }

        /// <summary>
        /// Создать SqlCommand для хранимой процедуры.
        /// </summary>
        public static SqlCommand CreateStoredProcCommand(SqlConnection connection, string spName)
        {
            var result = CreateSqlCommand(connection, spName);
            result.CommandType = CommandType.StoredProcedure;
            return result;
        }

        /// <summary>
        /// Создать SqlCommand для запроса.
        /// </summary>
        public static SqlCommand CreateSqlCommand(SqlConnection connection, string query)
        {
            if (connection.State != ConnectionState.Open) connection.Open();
            var result = new SqlCommand(query, connection)
            {
                CommandTimeout = ConfigurationManager.AppSettings["CommandTimeout"].ToInt(),
                CommandType = CommandType.Text
            };
            return result;
        }

        /// <summary>
        /// Заменяет все null на DBNull.
        /// </summary>
        public static SqlCommand AdjustParameters(SqlCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");
            foreach (SqlParameter parameter in command.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
                else if (parameter.DbType == DbType.DateTime && (DateTime) parameter.Value == DateTime.MinValue)
                {
                    parameter.Value = SqlDateTime.MinValue;
                }
            }

            return command;
        }

        /// <summary>
        /// Преобразовать массив идентификаторов в XML формат (для SQL запросов с использованием оператора IN).
        /// </summary>
        /// <param name="identifiers">Массив идентификаторов.</param>
        /// <returns>XML представление переданных идентификаторов.</returns>
        public static string GetXml(IEnumerable<int> identifiers)
        {
            var filters = identifiers.ToList().SelectEx(
                x => $@"<Item identifier=""{x}"" />");
            return $"<Items>{filters.AggregateEx((x, y) => x+y)}</Items>";
        }

        /// <summary>
        /// Протоколировать выполняемую SQL команду.
        /// </summary>
        public static void ProtocolSqlCommand(SqlCommand commandToProtocol)
        {
            var text = new StringBuilder();
            text.Append(commandToProtocol.CommandText);
            commandToProtocol.Parameters.Cast<SqlParameter>().
                OrderByDescending(x => x.ParameterName.Length).
                ToList().ForEach(x => ReplaceSqlParameter(text, x));
            Console.WriteLine(text.ToString());
        }

        protected static void ReplaceSqlParameter(StringBuilder text, SqlParameter parameter)
        {
            var value = string.Empty;
            if (new[] {SqlDbType.Structured}.Contains(parameter.SqlDbType))
            {
                var table = parameter.Value as DataTable;
                if (table != null)
                {
                    var ids = table.Rows.Cast<DataRow>().SelectEx(x => x["Id"]);
                    value = ids.Aggregate(value, (x, y) => x +$"SELECT {y} Id UNION ");
                    value = $"({value.Substring(0, value.Length-6)}) t";
                }
            }
            else if (parameter.Value == DBNull.Value || parameter.SqlValue == null)
            {
                value = "NULL";
            }
            else if (new[] {DbType.String, DbType.Guid}.Contains(parameter.DbType))
            {
                value = $"'{parameter.SqlValue}'";
            }
            else if (new[] {DbType.Boolean}.Contains(parameter.DbType))
            {
                value = parameter.Value.To<int>().ToStr();
            }
            else if (new[] {DbType.Date, DbType.DateTime, DbType.DateTime2}.Contains(parameter.DbType))
            {
                value = $"'{parameter.Value.To<DateTime>().ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            else
            {
                value = parameter.SqlValue.ToString();
            }

            text.Replace(parameter.ParameterName, value);
        }
    }
}