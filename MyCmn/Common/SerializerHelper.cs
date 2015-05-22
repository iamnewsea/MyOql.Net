using System;
using System.Text;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Permissions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.IO.Compression;

namespace MyCmn
{
    /// <summary>
    /// 序列化类。
    /// </summary>
    public static partial class SerializerHelper
    {

        /// <summary>
        /// 获取二进制序列化是否被使用。
        /// </summary>
        public static bool CanBinarySerialize { get; private set; }

        /// <summary>
        /// 静态构造函数仅在设置CanBinarySerialize值中使用一次。
        /// </summary>
        static SerializerHelper()
        {
            SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
            try
            {
                sp.Demand();
                CanBinarySerialize = true;
            }
            catch (SecurityException)
            {
                CanBinarySerialize = false;
            }
        }

        /// <summary>
        /// 返回二进制编码后的Base64编码 ， 和 Base64_UnSerial 对应使用。 [★]
        /// </summary>
        /// <param name="ObjWith_Base64_UnSerial"></param>
        /// <returns></returns>
        public static string ToBase64String<T>(this T ObjWith_Base64_UnSerial)
        {
            if (ObjWith_Base64_UnSerial == null) return string.Empty;

            //兼容性处理 String 类型的。
            if (ObjWith_Base64_UnSerial.GetType().IsSimpleType()) return ObjWith_Base64_UnSerial.AsString();

            Enum enu = ObjWith_Base64_UnSerial as Enum;
            if (enu != null) return ObjWith_Base64_UnSerial.AsInt().AsString();

            // if (ObjWith_Base64_UnSerial is string && ObjWith_Base64_UnSerial.AsString().HasValue() == false) return string.Empty;

            return Convert.ToBase64String(ConvertToBytes(ObjWith_Base64_UnSerial));
        }


        /// <summary>
        /// 对 Base64_Serial 函数编码的反序列化.
        /// </summary>
        /// <param name="StringDealdWith_Base64_Serial">经过 Base64_Serial 编码的序列化文本</param>
        /// <returns></returns>
        public static T FromBase64String<T>(this string StringDealdWith_Base64_Serial)
        {
            if (StringDealdWith_Base64_Serial.HasValue() == false) return default(T);

            return (T)FromBase64String(StringDealdWith_Base64_Serial, typeof(T));
        }

        public static object FromBase64String(this string StringDealdWith_Base64_Serial, Type TypeToReturn)
        {
            //兼容性处理 String 类型的。
            if (TypeToReturn.IsSimpleType()) return ValueProc.AsType(TypeToReturn, StringDealdWith_Base64_Serial);

            if (TypeToReturn.IsSubclassOf(typeof(Enum))) return StringDealdWith_Base64_Serial.AsInt();

            //if (string.IsNullOrEmpty(StringDealdWith_Base64_Serial)) return null;

            return ConvertToObject(Convert.FromBase64String(StringDealdWith_Base64_Serial));
        }

        /// <summary>
        /// 将对象转化成二进制的数组。和 ConvertToObject 对应使用。 [★] 
        /// </summary>
        /// <param name="objectToConvert">用于转化的对象。</param>
        /// <returns>返回转化后的数组，如果CanBinarySerialize为false则返回null。</returns>
        public static byte[] ConvertToBytes(object objectToConvert)
        {
            if (CanBinarySerialize)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                using (MemoryStream ms = new MemoryStream())
                {
                    binaryFormatter.Serialize(ms, objectToConvert);
                    ms.Seek(0, SeekOrigin.Begin);
                    return Compress(ms);
                }
            }
            return null;
        }

        /// <summary>
        /// 将一个二进制的数组转化为对象，必须通过类型转化自己想得到的相应对象。如果数组为空则返回空。 
        /// 和 ConvertToBytes 对应使用。  [★] .
        /// </summary>
        /// <param name="byteArray">用于转化的二进制数组。</param>
        /// <returns>返回转化后的对象实例，如果数组为空，则返回空对象。</returns>
        public static object ConvertToObject(byte[] byteArray)
        {
            object convertedObject = null;
            if (CanBinarySerialize && byteArray != null && byteArray.Length > 0)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                using (MemoryStream ms = new MemoryStream(byteArray))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var deBytes = DeCompress(ms);
                    using (MemoryStream msOut = new MemoryStream(deBytes))
                    {
                        msOut.Seek(0, SeekOrigin.Begin);
                        convertedObject = binaryFormatter.Deserialize(msOut);
                        return convertedObject;
                    }

                }
            }
            return convertedObject;
        }


        private static byte[] Compress(Stream stream)
        {
            MemoryStream msOut = new MemoryStream();
            try
            {
                using (GZipStream gzip = new GZipStream(msOut, CompressionMode.Compress))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, 4096)) > 0)
                    {
                        gzip.Write(buffer, 0, bytesRead);
                    }
                    // 这个地方一定要Close了,gzip才会执行最后的压缩过程,不是很知道原因
                    // 是不是一定要close,Flush不知道可不可以
                    gzip.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Compress failed!", ex);
            }
            // 因为上面的close会把相关流都close掉,这里要返回一个新流
            return msOut.ToArray();
        }

        private static byte[] DeCompress(Stream stream)
        {
            MemoryStream msOut = new MemoryStream();
            try
            {
                using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = gzip.Read(buffer, 0, 4096)) > 0)
                    {
                        msOut.Write(buffer, 0, bytesRead);
                    }
                    // 这里为了安全起见,也把zip流关闭,再返回新流
                    gzip.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DeCompress failed!", ex);
            }

            // 如果这里返回原流的话,一定要把流seek到开始点
            return msOut.ToArray();
        }

        public static string ObjToXml<T>(T obj, Func<XmlDocument, string> Func)
        {
            if ((object)obj == null) return "";
            XmlSerializer serialize = new XmlSerializer(obj.GetType());
            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                serialize.Serialize(tw, obj);
            }
            string strRet = sb.ToString();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strRet);

            if (Func != null) strRet = Func(xmlDoc);
            return strRet;
        }

        public static T XmlToObj<T>(Type type, string Xml)
        {
            if (Xml.HasValue() == false) return default(T);
            XmlSerializer serialize = new XmlSerializer(type);
            using (TextReader tr = new StringReader(Xml))
            {
                return (T)serialize.Deserialize(tr);
            }
        }


    }
}