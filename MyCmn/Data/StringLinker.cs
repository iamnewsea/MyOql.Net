using System;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MyCmn
{
    /// <summary>
    /// StringBuilder 替代方案. 内部采用 StringBuilder .
    /// 可以 和 string 互转, 可以 += .
    /// | 操作符, 返加第一个有效数据.
    /// </summary>
    [Serializable]
    public partial class StringLinker : IConvertible, IEquatable<string>, ICloneable, IComparable<string>
    {
        public StringLinker() { }
        public StringLinker(string StringOne)
        {
            this.sb.Append(StringOne);
        }

        public static implicit operator StringBuilder(StringLinker StringOne)
        {
            return StringOne.sb;
        }
        public static implicit operator string(StringLinker StringOne)
        {
            return StringOne.ToString();
        }

        public static implicit operator StringLinker(string StringOne)
        {
            var retVal = new StringLinker();
            retVal += StringOne;
            return retVal;
        }
        internal StringBuilder sb = new StringBuilder();
        public static StringLinker operator +(StringLinker StringOne, string AddString)
        {
            StringOne.sb.Append(AddString);
            return StringOne;
        }

        public static StringLinker operator |(StringLinker StringOne, string OrString)
        {
            if (StringOne.HasValue()) return StringOne;
            else return new StringLinker(OrString);
        }

        public static StringLinker operator |(string StringOne, StringLinker OrString)
        {
            if (StringOne.HasValue()) return new StringLinker(StringOne);
            else return OrString;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public int Length
        {
            get
            {
                return sb.Length;
            }
        }

        public bool HasValue() { return Length > 0; }

        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            if (HasValue() == false) return false;
            return (this.ToString() as IConvertible).ToBoolean(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToByte(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToChar(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToDateTime(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToDecimal(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToDouble(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToInt64(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToSByte(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToSingle(provider);
        }


        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToType(conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToUInt16(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToUInt32(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToUInt64(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return (this.ToString() as IConvertible).ToString(provider);
        }

        public virtual bool Equals(string other)
        {
            if (this.sb == null) return other == null;
            if (other == null) return this.sb == null;

            return string.Equals(this.sb.ToString(), other);
        }

        public object Clone()
        {
            return new StringLinker(this.sb.ToString());
        }

        // 摘要:
        //     比较当前对象和同一类型的另一对象。
        //
        // 参数:
        //   other:
        //     与此对象进行比较的对象。
        //
        // 返回结果:
        //     一个值，指示要比较的对象的相对顺序。返回值的含义如下：值含义小于零此对象小于 other 参数。零此对象等于 other。大于零此对象大于 other。
        public virtual int CompareTo(string other)
        {
            if (this.sb == null)
            {
                if (other == null) return 0;
                else return -1;
            }
            else if (other == null)
            {
                return 1;
            }
            else
            {
                return this.sb.ToString().CompareTo(other);
            }
        }
    }


    public class MyStringLinerJsonNetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.FullName == "MyCmn.StringLinker") return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            else if (reader.TokenType == JsonToken.String) return new StringLinker(reader.Value.AsString());
            else return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.AsString());
        }
    }
    public class MyStringBuilderJsonNetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.FullName == "System.Text.StringBuilder") return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            else if (reader.TokenType == JsonToken.String) return new StringBuilder(reader.Value.AsString());
            else return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.AsString());
        }
    }
}
