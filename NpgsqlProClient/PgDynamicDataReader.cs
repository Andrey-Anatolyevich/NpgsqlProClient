using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace NpgsqlProClient
{
    public class PgDynamicDataReader
    {
        internal T ReadScalar<T>(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (table.Rows.Count < 1)
                throw new Exception($"{nameof(table)} doesn't have any {nameof(table.Rows)}.");

            var firstColumn = table.Columns[0];
            var firstRow = table.Rows[0];
            var valueObject = firstRow[firstColumn];

            var value = (T)Convert.ChangeType(valueObject, typeof(T));
            return value;
        }

        internal List<T> ReadMany<T>(DataTable table)
            where T : new()
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));


            var modelFields = GetPgFieldProperties(typeof(T));
            var result = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                var currentResultItem = new T();
                foreach (var fieldInfo in modelFields)
                    SetObjectFieldFromRow(currentResultItem, row, fieldInfo);

                result.Add(currentResultItem);
            }

            return result;
        }

        internal T ReadSingleOrNone<T>(DataTable table)
            where T : new()
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));


            if (table.Rows.Count <= 0)
                return default(T);
            T result = GetSingle<T>(table);

            return result;
        }

        internal T ReadSingle<T>(DataTable table)
            where T : new()
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));


            if (table.Rows.Count < 1)
                throw new Exception($"{nameof(table)} doesn't have any {nameof(table.Rows)}.");

            T result = GetSingle<T>(table);
            return result;
        }

        private T GetSingle<T>(DataTable table) where T : new()
        {
            var firstRow = table.Rows[0];

            var pgFieldProps = GetPgFieldProperties(typeof(T));
            var result = new T();
            foreach (var prop in pgFieldProps)
            {
                SetObjectFieldFromRow(result, firstRow, prop);
            }

            return result;
        }

        private void SetObjectFieldFromRow<T>(T result, DataRow firstRow, PgFieldProperties fieldProps)
            where T : new()
        {
            object value = null;
            var pgValueObject = firstRow[fieldProps.PgColName];
            // NULL
            if (pgValueObject == null || pgValueObject == DBNull.Value)
            {
                if (fieldProps.IsNullable)
                {
                    value = null;
                }
                else
                {
                    throw new Exception($"Value of column '{fieldProps.PgColName}' is NULL, but the mapped field '{fieldProps.ClassFieldName}' is NON-Nullable.");
                }
            }
            // NOT NULL
            else
            {
                if (fieldProps.IsNullable)
                {
                    if (fieldProps.ClassFieldType.IsValueType)
                    {
                        // ENUM
                        if (fieldProps.ClassFieldType.IsEnum)
                        {
                            var pgValueObjectString = pgValueObject.ToString();
                            var enumValue = Enum.Parse(fieldProps.ClassFieldType, pgValueObjectString);
                            value = Convert.ChangeType(enumValue, Nullable.GetUnderlyingType(fieldProps.ClassFieldType), CultureInfo.InvariantCulture);
                        }
                        // Basic types
                        else
                        {
                            value = Convert.ChangeType(pgValueObject, Nullable.GetUnderlyingType(fieldProps.ClassFieldType), CultureInfo.InvariantCulture);
                        }
                    }
                    else if (fieldProps.ClassFieldType == typeof(string))
                    {
                        value = Convert.ChangeType(pgValueObject, fieldProps.ClassFieldType, CultureInfo.InvariantCulture);
                    }
                    else
                        throw new NotSupportedException($"Can't process field: {fieldProps.ClassFieldName} of type {fieldProps.ClassFieldType.Name}.");
                }
                else
                {
                    // ENUM
                    if (fieldProps.ClassFieldType.IsEnum)
                    {
                        var pgValueObjectString = pgValueObject.ToString();
                        value = Enum.Parse(fieldProps.ClassFieldType, pgValueObjectString);
                    }
                    // Basic types
                    else
                    {
                        value = Convert.ChangeType(pgValueObject, fieldProps.ClassFieldType, CultureInfo.InvariantCulture);
                    }
                }
            }

            var fieldInfo = typeof(T).GetField(fieldProps.ClassFieldName);
            if (fieldInfo == null)
                throw new MissingFieldException(className: typeof(T).Name, fieldName: fieldProps.ClassFieldName);

            fieldInfo.SetValue(result, value);
        }

        private List<PgFieldProperties> GetPgFieldProperties(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));


            var pgFieldProps = new List<PgFieldProperties>();

            var readTypeFields = type.GetFields();
            foreach (var field in readTypeFields)
            {
                var pgColName = field.Name;
                var fieldType = field.FieldType;
                var fieldTypeIsNullable = false;
                var colNameAttributes = field.GetCustomAttributes(typeof(PgColumnNameAttribute), false);
                if (colNameAttributes != null && colNameAttributes.Any())
                {
                    var colNameAttribute = colNameAttributes.First() as PgColumnNameAttribute;
                    pgColName = colNameAttribute.ColumnName;
                }

                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    fieldType = Nullable.GetUnderlyingType(fieldType);
                    fieldTypeIsNullable = true;
                }

                if (fieldType == typeof(string))
                    fieldTypeIsNullable = true;


                var pgFieldProp = new PgFieldProperties(fieldType: field.FieldType
                    , isNullable: fieldTypeIsNullable, fieldName: field.Name
                    , pgColName: pgColName);
                pgFieldProps.Add(pgFieldProp);
            }

            return pgFieldProps;
        }
    }
}
