using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace NpgsqlProClient
{
    public class PgClientCommand
    {
        internal PgClientCommand(PgClient pgClinet
            , PgDynamicDataReader dynamicDataReader)
        {
            _pgClinet = pgClinet;
            _pgDataReader = dynamicDataReader;

            Params = new List<NpgsqlParameter>();
        }

        private PgClient _pgClinet;
        private PgDynamicDataReader _pgDataReader;

        internal string FuncName;
        internal List<NpgsqlParameter> Params;

        public PgClientCommand OnFunc(string funcName)
        {
            if (string.IsNullOrWhiteSpace(funcName))
                throw new ArgumentException("String is NULL / Empty / Whitespace.", nameof(funcName));


            FuncName = funcName;
            return this;
        }

        public PgClientCommand WithParam(string paramName, NpgsqlDbType paramType, object value)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new ArgumentException("String is NULL / Empty / Whitespace.", nameof(paramName));


            var newParam = new NpgsqlParameter(parameterName: paramName, parameterType: paramType);
            newParam.Value = value ?? DBNull.Value;
            Params.Add(newParam);

            return this;
        }

        public void QueryVoid()
        {
            _pgClinet.ExecuteNonQuery(FuncName, Params);
        }
        public T QueryScalar<T>()
        {
            var table = _pgClinet.ExecuteQueryOnFunc(FuncName, Params);
            T innerResult = _pgDataReader.ReadScalar<T>(table);
            return innerResult;
        }

        public T QuerySingle<T>() where T : new()
        {
            var table = _pgClinet.ExecuteQueryOnFunc(FuncName, Params);
            T result = _pgDataReader.ReadSingle<T>(table);
            return result;
        }

        public T QuerySingleOrNone<T>() where T : new()
        {
            var table = _pgClinet.ExecuteQueryOnFunc(FuncName, Params);
            T result = _pgDataReader.ReadSingleOrNone<T>(table);
            return result;
        }

        public IEnumerable<T> QueryMany<T>() where T : new()
        {
            var table = _pgClinet.ExecuteQueryOnFunc(FuncName, Params);
            var innerResult = _pgDataReader.ReadMany<T>(table);
            return innerResult;
        }
    }
}
