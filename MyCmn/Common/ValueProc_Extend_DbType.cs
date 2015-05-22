using System;
using System.Data;
using System.ComponentModel;

namespace MyCmn
{
    public static partial class ValueProc
    {
        /// <summary>
        /// 判断类型是否是数据类型.包含枚举
        /// </summary>
        /// <remarks>
        /// 数据类型包括:
        ///           "System.Int32",
        ///           "System.Int64",
        ///           "System.Int16",
        ///           "System.Decimal",
        ///           "System.Byte",
        ///           "System.Double",
        ///           "System.Float",
        ///           "System.Single",
        ///           "System.UInt32",
        ///           "System.UInt64",
        ///           "System.UInt16",
        ///           "System.UInt64"
        ///
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumberType(this Type type)
        {
            if (type.IsEnum || type.FullName == "System.Enum") return true;

            var code = Type.GetTypeCode(type);
            return IsNumberType(code);

            //return type.FullName.IsIn(
            //        "System.Int32",
            //        "System.Int64",
            //        "System.Int16",
            //        "System.Decimal",
            //        "System.Byte",
            //        "System.Double",
            //        "System.Float",
            //        "System.Single",
            //        "System.UInt32",
            //        "System.UInt64",
            //        "System.UInt16",
            //        "System.UInt64"
            //    );
        }

        /// <summary>
        /// 仅从 TypeCode 判断是否是数字型，枚举返回false 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsNumberType(this TypeCode code)
        {
            if (code == TypeCode.Byte) return true;
            if (code == TypeCode.Decimal) return true;
            if (code == TypeCode.Double) return true;
            if (code == TypeCode.Int16) return true;
            if (code == TypeCode.Int32) return true;
            if (code == TypeCode.Int64) return true;
            if (code == TypeCode.UInt16) return true;
            if (code == TypeCode.UInt32) return true;
            if (code == TypeCode.UInt64) return true;
            if (code == TypeCode.SByte) return true;
            if (code == TypeCode.Single) return true;

            return false;
        }



        /// <summary>
        /// 判断类型是否是简单类型.
        /// </summary>
        /// <remarks>
        /// 枚举, 基元类型, 另外包括:
        ///           "System.String",
        ///           "System.Char",
        ///           "System.Boolean",
        ///           "System.DateTime",
        ///           "System.Guid",
        ///           "System.Decimal"
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type type)
        {
            if (type.IsPrimitive) return true;
            if (type.IsEnum) return true;

            var code = Type.GetTypeCode(type);
            if (code == TypeCode.DBNull || code == TypeCode.Empty || code == TypeCode.Object)
            {
                return false;
            }
            return true;

            //return type.FullName.IsIn(
            //        "System.String",
            //        "System.DateTime",
            //        "System.Guid",
            //        "System.Enum",
            //        "System.Decimal"
            //    );
        }


        /// <summary>
        /// 判断 DbType 是否是时间类型.
        /// </summary>
        /// <remarks>
        ///      DbType.Date,
        ///      DbType.DateTime,
        ///      DbType.DateTime2,
        ///      DbType.DateTimeOffset,
        ///      DbType.Time 
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool DbTypeIsDateTime(this DbType type)
        {
            return type.IsIn(
                DbType.Date,
                DbType.DateTime,
                DbType.DateTime2,
                DbType.DateTimeOffset,
                DbType.Time);
        }


        /// <summary>
        /// DbType 是否是数据类型.
        /// </summary>
        /// <remarks>
        ///        DbType.Byte,
        ///        DbType.Currency,
        ///        DbType.Decimal,
        ///        DbType.VarNumeric,
        ///        DbType.Double,
        ///        DbType.Int16,
        ///        DbType.Int32,
        ///        DbType.Int64,
        ///        DbType.SByte,
        ///        DbType.Single,
        ///        DbType.UInt16,
        ///        DbType.UInt32,
        ///        DbType.UInt64 
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool DbTypeIsNumber(this DbType type)
        {
            return type.IsIn(
                DbType.Byte,
                DbType.Currency,
                DbType.Decimal,
                DbType.VarNumeric,
                DbType.Double,
                DbType.Int16,
                DbType.Int32,
                DbType.Int64,
                DbType.SByte,
                DbType.Single,
                DbType.UInt16,
                DbType.UInt32,
                DbType.UInt64);
        }

        /// <summary>
        /// 判断DbType 是否是 字符串.
        /// </summary>
        /// <remarks>
        ///       DbType.AnsiString,
        ///       DbType.AnsiStringFixedLength,
        ///       DbType.String,
        ///       DbType.StringFixedLength 
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool DbTypeIsString(this DbType type)
        {
            return type.IsIn(
                DbType.AnsiString,
                DbType.AnsiStringFixedLength,
                DbType.String,
                DbType.StringFixedLength);
        }

        public static DbType GetDbType(this Type type)
        {
            if (type.IsNullableType())
            {
                type = type.GetGenericArguments()[0];
            }

            var code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Char:
                    return DbType.UInt16;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.String:
                    return DbType.String;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    return DbType.Object;
                case TypeCode.Object:
                    var typeName = type.FullName;
                    if (typeName == "System.Byte[]") return DbType.Binary;
                    if (typeName == "System.Guid") return DbType.Guid;
                    if (typeName == "MyCmn.MyDate") return DbType.DateTime;
                    if (typeName == "System.Text.StringBuilder") return DbType.String;
                    if (typeName == "MyCmn.StringLinker") return DbType.String;
                    return DbType.Object;
                default:
                    break;
            }

            return DbType.Object;
        }

        public static Type GetCsType(this DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString: return typeof(string);
                case DbType.AnsiStringFixedLength: return typeof(string);
                case DbType.Binary: return typeof(byte[]);
                case DbType.Boolean: return typeof(bool);
                case DbType.Byte: return typeof(byte);
                case DbType.Currency: return typeof(decimal);
                case DbType.Date: return typeof(DateTime);
                case DbType.DateTime: return typeof(DateTime);
                case DbType.DateTime2: return typeof(DateTime);
                case DbType.DateTimeOffset: return typeof(DateTime);
                case DbType.Decimal: return typeof(decimal);
                case DbType.Double: return typeof(double);
                case DbType.Guid: return typeof(Guid);
                case DbType.Int16: return typeof(Int16);
                case DbType.Int32: return typeof(int);
                case DbType.Int64: return typeof(Int64);
                case DbType.Object: return typeof(object);
                case DbType.SByte: return typeof(sbyte);
                case DbType.Single: return typeof(Single);
                case DbType.String: return typeof(string);
                case DbType.StringFixedLength: return typeof(string);
                case DbType.Time: return typeof(TimeSpan);
                case DbType.UInt16: return typeof(UInt16);
                case DbType.UInt32: return typeof(UInt32);
                case DbType.UInt64: return typeof(UInt64);
                case DbType.VarNumeric: return typeof(decimal);
                case DbType.Xml: return typeof(System.Xml.XmlDocument);
                default:
                    break;
            }
            return typeof(string);
        }
    }
}
