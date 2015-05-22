using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections;

namespace MyCmn
{
    /// <summary>
    /// 可序列化字典类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class XmlDictionary<TKey, TValue>
            : Dictionary<TKey, TValue>, IXmlSerializable, ISerializable, IModel
    {
        public XmlDictionary()
        {
        }

        public XmlDictionary(IDictionary<TKey, TValue> dict)
            : base(dict == null ? new Dictionary<TKey, TValue>() : dict)
        {
        }

        protected XmlDictionary(SerializationInfo info, StreamingContext context)
        {
            var enu = info.GetEnumerator();
            while (enu.MoveNext())
            {
                if (enu.Name == "KeyValuePairs")
                {
                    var ary = enu.Value as KeyValuePair<TKey, TValue>[];
                    foreach (var item in ary)
                    {
                        this[item.Key] = item.Value;
                    }
                    break;
                }
            }
        }

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


        #region IXmlSerializable 成员

        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            if (reader.IsEmptyElement || !reader.Read())
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadEndElement();

                this.Add(key, value);
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion

        /// <summary>
        /// 有很大的陷井,尽量不要用,除非你能确定,所有的Key,Value 都是值类型.
        /// </summary>
        /// <returns></returns>
        public XmlDictionary<TKey, TValue> SimpleClone()
        {
            return this.MemberwiseClone() as XmlDictionary<TKey, TValue>;
        }

        public virtual object Clone()
        {
            var ret = new XmlDictionary<TKey, TValue>();
            this.Keys.All(o =>
                {
                    var itemValue = this[o];
                    if (itemValue.IsDBNull())
                    {
                        ret[o] = itemValue;
                        return true;
                    }

                    var val = itemValue as ValueType;
                    if (val != null)
                    {
                        ret[o] = itemValue;
                        return true;
                    }

                    var str = itemValue as string;
                    if (str != null)
                    {
                        ret[o] = itemValue;
                        return true;
                    }


                    ret[o] = itemValue.CloneEntity();
                    return true;
                });
            return ret;
        }
        public override bool Equals(object obj)
        {
            if (obj.IsDBNull()) return false;
            var otherObject = obj as IDictionary<TKey, TValue>;
            if (otherObject == null) return false;
            if (this.Keys.Count != otherObject.Keys.Count) return false;


            for (int i = 0; i < this.Keys.Count; i++)
            {
                var myItem = this.ElementAt(i);
                var otherItem = otherObject.ElementAt(i);
                if (myItem.Key.Equals(otherItem.Key) == false) return false;

                if (myItem.Value != null)
                {
                    if (myItem.Value.Equals(otherItem.Value) == false) return false;
                }
                else if (otherItem.Value == null) continue;
                else return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            decimal retVal = 0;
            this.All(o =>
                {
                    retVal += o.Value.GetHashCode();
                    return true;
                });

            return (int)(retVal % int.MaxValue);
        }

        public StringDict ToStringDict()
        {
            var ret = new StringDict();
            this.Keys.All(o =>
            {
                ret.Add(o.AsString(), this[o].AsString());
                return true;
            });

            return ret;
        }
    }

    public class XmlDictionaryEqual<TKey, TValue> : IEqualityComparer<XmlDictionary<TKey, TValue>>
    {
        public bool Equals(XmlDictionary<TKey, TValue> x, XmlDictionary<TKey, TValue> y)
        {
            if (x == null && y == null) return true;
            if (x == null) return false;

            return x.Equals(y);
        }

        public int GetHashCode(XmlDictionary<TKey, TValue> obj)
        {
            return obj.GetHashCode();
        }
    }
}
