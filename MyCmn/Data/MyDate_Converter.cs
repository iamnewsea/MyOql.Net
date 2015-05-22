using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MyCmn
{
    public class MyDateConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType.FullName == "System.DateTime") return true;
            if (sourceType.FullName == "System.String") return true;
            if (sourceType.FullName == "System.DBNull") return true;
            if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition().FullName == "System.Nullable`1")
            {
                var args = sourceType.GetGenericArguments();
                if (args.Length == 1 && args[0].FullName == "System.DateTime") return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.FullName == "System.DateTime") return true;
            if (destinationType.FullName == "System.String") return true;
            if (destinationType.FullName == "System.DBNull") return true;
            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition().FullName == "System.Nullable`1")
            {
                var args = destinationType.GetGenericArguments();
                if (args.Length == 1 && args[0].FullName == "System.DateTime") return true;
            }


            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.IsDBNull()) return new MyDate(DateTime.MinValue);
            {
                var str = value as string;
                if (str != null) return new MyDate(str.AsDateTime());
            }
            {
                var val = value as DateTime?;
                if (val.HasValue) return new MyDate(val.Value);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value.IsDBNull()) return string.Empty;

            if (destinationType.FullName == "System.String")
            {
                return ((MyDate)value).ToString();
            }
            if (destinationType.FullName == "System.DateTime")
            {
                return ((MyDate)value).date;
            }

            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition().FullName == "System.Nullable`1")
            {
                var args = destinationType.GetGenericArguments();
                if (args.Length == 1 && args[0].FullName == "System.DateTime")
                {
                    return ((MyDate)value).date;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class MyDateJsonNetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.FullName == "MyCmn.MyDate") return true;
            if (objectType == typeof(MyDate?))
                return true;

            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                bool nullable = objectType.IsNullableType();
                //Type t = (nullable) ? Nullable.GetUnderlyingType(objectType) : objectType;
                if (!nullable)
                {
                    return MyDate.MinValue;
                    //throw new GodError(string.Format(@"Cannot convert null value to {0}.", objectType.FullName));
                }

                return null;
            }

            if (reader.TokenType == JsonToken.Date)
            {
                return reader.Value.AsMyDate();
            }

            if (reader.TokenType != JsonToken.String)
                throw new GodError(string.Format(@"Unexpected token parsing date. Expected String, got {0}.", reader.TokenType));

            return reader.Value.AsMyDate();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string text = null;

            if (value is MyDate)
            {
                text = ((MyDate)value).ToString();
            }
            else
            {
                throw new GodError(string.Format(@"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.", value.GetType().FullName));
            }

            writer.WriteValue(text);

        }
    }
}
