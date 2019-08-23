using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace NpgsqlProClient
{
    public class PgClient
    {
        public PgClient(string pgSqlConnectionString)
        {


            _connectionString = pgSqlConnectionString;
            _pgDynamicDataReader = new PgDynamicDataReader();
        }

        private readonly string _connectionString;
        private PgDynamicDataReader _pgDynamicDataReader;

        public PgClientCommand NewCommand()
        {
            return new PgClientCommand(this, _pgDynamicDataReader);
        }

        internal void ExecuteNonQuery(string funcName, List<NpgsqlParameter> parameters = null)
        {
            ReportCall(funcName);


            using (var conn = GetNewOpenConnection())
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = funcName;

                if (parameters != null)
                    parameters.ForEach(param => cmd.Parameters.Add(param));

                cmd.ExecuteNonQuery();
            }
        }

        internal DataTable ExecuteQueryOnFunc(string funcName, List<NpgsqlParameter> parameters = null)
        {
            ReportCall(funcName);

            var result = new DataTable();
            using (var conn = GetNewOpenConnection())
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = funcName;

                if (parameters != null)
                    parameters.ForEach(param => cmd.Parameters.Add(param));

                using (var pgSqlReader = cmd.ExecuteReader())
                {
                    result.Load(pgSqlReader);
                }
            }
            return result;
        }

        internal T ExecuteScalarFunc<T>(string funcName, List<NpgsqlParameter> parameters = null)
        {
            ReportCall(funcName);

            using (var conn = GetNewOpenConnection())
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = funcName;

                if (parameters != null)
                    parameters.ForEach(param => cmd.Parameters.Add(param));

                object resultObject = cmd.ExecuteScalar();
                var result = (T)resultObject;
                return result;
            }
        }

        private NpgsqlConnection GetNewOpenConnection()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new Exception("ConnectionString is NULL / Empty / Whitespace.");


            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private void ReportCall(string command)
        {
            var bg = Console.BackgroundColor;
            var fg = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Thread.Sleep(10);
            Console.WriteLine($"PgClient reports: Storage call: {command}");
            Thread.Sleep(10);

            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }
    }

}
