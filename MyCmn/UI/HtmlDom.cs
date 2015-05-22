using MyCmn.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;

namespace MyCmn
{
    [DebuggerDisplay("{this.ToString()}")]
    [Serializable]
    public class HtmlNode : ICloneable
    {
        public enum NodeType
        {
            Tag,
            Text,
            Note,
            CloseTag,
            TreeTag,
        }
        public NodeType Type { get; set; }
        public int Level { get; set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public static bool operator ==(HtmlNode one, HtmlNode other)
        {
            if (object.Equals(one, null) && object.Equals(other, null)) return true;
            if (object.Equals(one, null) || object.Equals(other, null)) return false;

            if (one.Type != other.Type) return false;

            switch (one.Type)
            {
                case NodeType.Tag:
                    return (one as HtmlTagNode) == (other as HtmlTagNode);
                case NodeType.Text:
                    return (one as HtmlTextNode) == (other as HtmlTextNode);
                case NodeType.Note:
                    return (one as HtmlNoteNode) == (other as HtmlNoteNode);
                case NodeType.CloseTag:
                    return (one as HtmlCloseTagNode) == (other as HtmlCloseTagNode);
                case NodeType.TreeTag:
                    return (one as HtmlTreeTagNode) == (other as HtmlTreeTagNode);
                default:
                    break;
            }
            return false;
        }

        public static bool operator !=(HtmlNode one, HtmlNode other)
        {
            return !(one == other);
        }

        public string GetInnerText()
        {
            if (this.Type == NodeType.Text) return (this as HtmlTextNode).Text.Replace("&nbsp;", " ");

            HtmlTreeTagNode tree = null;
            if (this.Type == NodeType.TreeTag)
            {
                tree = this as HtmlTreeTagNode;

                var sl = new StringLinker();
                new Recursion<HtmlNode>().Execute(tree.Nodes, o =>
                {
                    if (o.Type == NodeType.TreeTag)
                    {
                        return (o as HtmlTreeTagNode).Nodes;
                    }
                    else return null;
                }, o =>
                {
                    if (o.Type == NodeType.Text)
                    {
                        sl += (o as HtmlTextNode).Text;
                    }
                    return RecursionReturnEnum.Go;
                });
                return sl.ToString().Replace("&nbsp;", " ");
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    [DebuggerDisplay("{this.ToString()}")]
    [Serializable]
    public class HtmlTextNode : HtmlNode
    {
        public HtmlTextNode()
        {
            Type = NodeType.Text;
        }

        public HtmlTextNode(string Text)
            : this()
        {
            this.Text = Text;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public static bool operator ==(HtmlTextNode one, HtmlTextNode other)
        {
            if (object.Equals(one, null) && object.Equals(other, null)) return true;
            if (object.Equals(one, null) || object.Equals(other, null)) return false;

            return one.Text == other.Text;
        }

        public static bool operator !=(HtmlTextNode one, HtmlTextNode other)
        {
            return !(one == other);
        }
    }

    [DebuggerDisplay("{this.ToString()}")]
    [Serializable]
    public class HtmlNoteNode : HtmlNode
    {
        public HtmlNoteNode()
        {
            Type = NodeType.Note;
        }
        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }

        public static bool operator ==(HtmlNoteNode one, HtmlNoteNode other)
        {
            if (object.Equals(one, null) && object.Equals(other, null)) return true;
            if (object.Equals(one, null) || object.Equals(other, null)) return false;

            return one.Text == other.Text;
        }

        public static bool operator !=(HtmlNoteNode one, HtmlNoteNode other)
        {
            return !(one == other);
        }
    }

    [Serializable]
    public class HtmlAttrNode : ICloneable
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否是自闭合
        /// </summary>
        public bool IsSole
        {
            get
            {
                return Value == null;
            }
        }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 值的引号
        /// </summary>
        public char ValueQuote { get; set; }

        public HtmlAttrNode()
        {
            ValueQuote = '"';
        }

        public override string ToString()
        {
            if (IsSole) return Name;
            if (Value.HasValue() == false) return Name;

            else return string.Format(@"{0}={2}{1}{2}", Name, Value, ValueQuote);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    //[Serializable]
    //public class HtmlTagBase : HtmlNode
    //{
    //}

    /// <summary>
    /// 重载了 == 
    /// </summary>
    [DebuggerDisplay("{this.ToString()}")]
    [Serializable]
    public class HtmlTagNode : HtmlNode
    {

        public HtmlTagNode(string Tag)
            : this()
        {
            IsSole = false;
            this.TagName = Tag;
        }


        public HtmlTagNode()
        {
            this.Type = NodeType.Tag;
            _IsSole = false;
            Attrs = new List<HtmlAttrNode>();
        }

        public List<HtmlAttrNode> Attrs { get; set; }

        private bool _IsSole;
        public bool IsSole
        {
            get { return _IsSole; }
            set
            {
                if (string.IsNullOrEmpty(this.TagName))
                {
                    HtmlTextWriterTag webTag = new HtmlTextWriterTag();
                    if (Enum.TryParse(this.TagName, true, out webTag))
                    {
                        if (webTag.IsIn(
                            HtmlTextWriterTag.Br,
                            HtmlTextWriterTag.Img,
                            HtmlTextWriterTag.Input,
                            HtmlTextWriterTag.Link,
                            HtmlTextWriterTag.Nobr,
                            HtmlTextWriterTag.Meta)
                            )
                        {
                            _IsSole = true;
                            return;
                        }
                    }
                }

                _IsSole = value;
                return;
            }
        }

        private string _TagName;
        public string TagName
        {
            get { return _TagName; }
            set
            {
                _TagName = value;
                HtmlTextWriterTag webTag = new HtmlTextWriterTag();
                if (Enum.TryParse(this.TagName, true, out webTag))
                {
                    if (webTag.IsIn(
                        HtmlTextWriterTag.Br,
                        HtmlTextWriterTag.Img,
                        HtmlTextWriterTag.Input,
                        HtmlTextWriterTag.Link,
                        HtmlTextWriterTag.Nobr,
                        HtmlTextWriterTag.Meta)
                        )
                    {
                        _IsSole = true;
                        return;
                    }
                }
            }
        }

        public string this[string key]
        {
            get { return GetAttributeValue(key); }
            set { SetAttributeValue(key, value); }
        }

        public string GetAttributeValue(string AttrName)
        {
            var ret = this.Attrs.FirstOrDefault(o => string.Equals(o.Name, AttrName, StringComparison.CurrentCultureIgnoreCase));
            if (ret == null) return null;
            if (ret.Value == null) return null;
            if (ret.Value.Length == 0) return string.Empty;
            if (ret.Value.Length == 1) return ret.Value;

            if (ret.Value[0] == '"' || ret.Value[0] == '\'') return ret.Value.Substring(1, ret.Value.Length - 2);

            return ret.Value;
        }

        public bool SetAttributeValue(string AttrName, string Value)
        {
            var ret = this.Attrs.FirstOrDefault(o => string.Equals(o.Name, AttrName, StringComparison.CurrentCultureIgnoreCase));
            if (ret == null)
            {
                this.Attrs.Add(new HtmlAttrNode() { Name = AttrName, Value = Value });
                return true;
            }
            ret.Value = Value;
            ret.ValueQuote = '"';
            return true;
        }

        public bool RemoveAttribute(string AttrName)
        {
            if (AttrName.HasValue() == false) return false;
            if (this.Attrs == null) return false;
            this.Attrs.RemoveAll(o => string.Equals(o.Name, AttrName));
            return true;
        }


        public bool RemoveClassName(string Value)
        {
            if (Value.HasValue() == false) return false;
            var css = this.GetAttributeValue("class");
            if (css.HasValue() == false) return false;

            var cls = css.AsString().MySplit(' ').ToList();

            cls = cls.Minus(Value.MySplit(' ').ToList()).ToList();
            cls = cls.Distinct().ToList();

            this.SetAttributeValue("class", cls.Join(" "));
            return true;
        }

        public bool SetStyle(string key, string val)
        {
            if (key.HasValue() == false) return false;
            if (val.HasValue() == false) return false;

            key = key.Replace(':', '\0').Replace(';', '\0');
            val = val.Replace(':', '\0').Replace(';', '\0');

            var style = this.GetAttributeValue("style").AsString().MySplit(';');
            var cls = new StringDict(style.Select(o =>
            {
                var kv = o.MySplit(':');
                return new KeyValuePair<string, string>(kv[0], kv[1]);
            }));

            cls[key] = val;

            this.SetAttributeValue("style", cls.Select(o => o.Key + ":" + o.Value).Join(";"));
            return true;
        }

        public string[] GetCsss()
        {
            var css = this.GetAttributeValue("class");
            if (css.HasValue() == false) return new string[0];
            return css.MySplit(' ').ToArray();
        }

        public bool HasClassName(string css)
        {
            return GetCsss().Contains(css);
        }

        public bool AddClassName(string Value)
        {
            if (Value.HasValue() == false) return false;

            var css = this.GetAttributeValue("class");
            var cls = css.AsString().MySplit(' ').ToList();
            if (cls.Contains(Value)) return false;

            cls.AddRange(Value.MySplit(' ').ToList());
            cls = cls.Distinct().ToList();

            this.SetAttributeValue("class", cls.Join(" "));
            return true;
        }


        public static bool operator ==(HtmlTagNode one, HtmlTagNode other)
        {
            if (object.Equals(one, null) && object.Equals(other, null)) return true;

            if (object.Equals(one, null)) return false;
            if (object.Equals(other, null)) return false;

            if (string.Equals(one.TagName, other.TagName, StringComparison.CurrentCultureIgnoreCase) == false) return false;

            if (one.Attrs.Count != other.Attrs.Count) return false;

            foreach (var atr in one.Attrs)
            {
                var otherAtr = other.Attrs.FirstOrDefault(o => string.Equals(o.Name, atr.Name, StringComparison.CurrentCultureIgnoreCase));
                if (otherAtr == null) return false;

                if (string.Equals(atr.Value, otherAtr.Value, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(HtmlTagNode one, HtmlTagNode other)
        {
            return !(one == other);
        }


        public string ToHtmlString()
        {
            string retVal = "";
            if (Attrs.Count == 0)
            {
                retVal = string.Format(@"<{0}", TagName);
            }
            else
            {
                retVal = string.Format(@"<{0} {1}", TagName,
                    string.Join(" ", Attrs.Select(o => o.ToString()).ToArray()));
            }

            if (IsSole)
            {
                retVal += " />";
            }
            else retVal += ">";

            return retVal;
        }

        public override string ToString()
        {
            return ToHtmlString();
        }

        public override object Clone()
        {
            var obj = this.MemberwiseClone() as HtmlTagNode;
            obj.Attrs = new List<HtmlAttrNode>();

            foreach (var item in this.Attrs)
            {
                obj.Attrs.Add(item.Clone() as HtmlAttrNode);
            }

            return obj;
        }


        /// <summary>
        /// 取子项并转换为 HtmlTreeTagNode，负数从右边取。
        /// </summary>
        /// <param name="NodeIndex"></param>
        /// <returns></returns>
        public HtmlTreeTagNode GetChild(int NodeIndex)
        {
            if (this.Type != NodeType.TreeTag)
            {
                return null;
            }

            var nodes = (this as HtmlTreeTagNode).Nodes;

            if (NodeIndex < 0)
            {
                NodeIndex = nodes.Count + NodeIndex;
            }

            return nodes[NodeIndex] as HtmlTreeTagNode;
        }
    }


    public class HtmlCloseTagNode : HtmlNode
    {
        public HtmlCloseTagNode()
        {
            Type = NodeType.CloseTag;
        }

        public HtmlCloseTagNode(string TagName)
            : this()
        {
            this.TagName = TagName;
        }

        public string TagName { get; set; }

        public override string ToString()
        {
            return string.Format(@"</{0}>", TagName);
        }

        public static bool operator ==(HtmlCloseTagNode one, HtmlCloseTagNode other)
        {
            if (object.Equals(one, null) && object.Equals(other, null)) return true;
            if (object.Equals(one, null) || object.Equals(other, null)) return false;

            return one.TagName == other.TagName;
        }

        public static bool operator !=(HtmlCloseTagNode one, HtmlCloseTagNode other)
        {
            return !(one == other);
        }

    }

}
