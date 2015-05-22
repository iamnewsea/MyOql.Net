using System;
using System.Collections.Generic;
using MyCmn.Visualizer;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Linq;

namespace MyCmn
{
    /// <summary>
    /// 通用树对象
    /// </summary>
    [DebuggerVisualizer(typeof(CommonTreeItemVisualizer))]
    [Serializable]
    public class CommonTreeDict : Dictionary<string, object>, IModel
    {
        /// <summary>
        /// 取字典里的 Id 值。
        /// </summary>
        public int Id { get { if (this.ContainsKey("Id")) return this["Id"].AsInt(); else return 0; } }

        /// <summary>
        /// 取字典里的 Name 值。
        /// </summary>
        public string Name { get { if (this.ContainsKey("Name")) return this["Name"].AsString(); else return string.Empty; } }

        public List<CommonTreeDict> Children { get; set; }

        public CommonTreeDict()
        {
            this.Children = new List<CommonTreeDict>();
        }

        public virtual object Clone()
        {
            var ret = new CommonTreeDict();
            var theObj = (CloneDict() as XmlDictionary<string, object>);
            theObj.Keys.All(o =>
                {
                    ret[o] = theObj[o];
                    return true;
                });

            if (this.Children != null)
            {
                this.Children.All(item =>
                    {
                        ret.Children.Add(item.Clone() as CommonTreeDict);
                        return true;
                    });
            }
            return ret;
        }

        public object CloneDict()
        {
            var ret = new Dictionary<string, object>();
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


        protected CommonTreeDict(SerializationInfo info, StreamingContext context)
        {
            this.Children = info.GetValue("Children", typeof(List<CommonTreeDict>)) as List<CommonTreeDict>;
            var kv = info.GetValue("KeyValuePairs", typeof(KeyValuePair<string, object>[])) as KeyValuePair<string, object>[];

            if (kv != null && kv.Length > 0)
            {
                foreach (var item in kv)
                {
                    this[item.Key] = item.Value;
                }
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Children", Children);
            base.GetObjectData(info, context);
        }
    }
}
