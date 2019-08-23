using System;

namespace NpgsqlProClient
{
    internal class PgFieldProperties
    {
        public PgFieldProperties(Type fieldType, bool isNullable, string fieldName, string pgColName)
        {
#pragma warning disable IDE0016
            if (fieldType == null)
                throw new ArgumentNullException(nameof(fieldType));
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("String is NULL / Empty / Whitespace.", nameof(fieldName));
            if (string.IsNullOrWhiteSpace(pgColName))
                throw new ArgumentException("String is NULL / Empty / Whitespace.", nameof(pgColName));

            ClassFieldType = fieldType;
            IsNullable = isNullable;
            ClassFieldName = fieldName;
            PgColName = pgColName;
        }

        public Type ClassFieldType;
        public bool IsNullable;
        public string ClassFieldName;
        public string PgColName;
    }
}
