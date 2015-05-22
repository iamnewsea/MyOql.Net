using MyCmn.Visualizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;

namespace MyCmn
{
    [DebuggerVisualizer(typeof(HtmlTreeTagNodeVisualizer))]
    [TreeChildPropertyName("Nodes")]
    [Serializable]
    public class HtmlTreeTagNode : HtmlTagNode
    {
        /// <summary>
        /// 子元素集成， 插入和添加，应该使用 AddNode 和 InsertNode
        /// </summary>
        public List<HtmlNode> Nodes { get; set; }

        [NonSerialized]
        public HtmlTreeTagNode Parent;


        public void ResetParent(HtmlTreeTagNode ParentNode = null)
        {
            if (this.Nodes == null) return;

            this.Parent = ParentNode;

            this.Nodes.All(o =>
            {
                var t = o as HtmlTreeTagNode;
                if (t == null) return true;

                t.ResetParent(this);
                return true;
            });
        }


        /// <summary>
        /// 安全添加。 自动设置子元素的 Parent
        /// </summary>
        /// <param name="SubNode"></param>
        public void AddNode(HtmlNode SubNode)
        {
            if (SubNode.Type == NodeType.TreeTag)
            {
                (SubNode as HtmlTreeTagNode).Parent = this;
            }
            this.Nodes.Add(SubNode);
        }

        /// <summary>
        /// 安全插入。自动设置子元素的 Parent
        /// </summary>
        /// <param name="index"></param>
        /// <param name="SubNode"></param>
        public void InsertNode(int index, HtmlNode SubNode)
        {
            if (SubNode.Type == NodeType.TreeTag)
            {
                (SubNode as HtmlTreeTagNode).Parent = this;
            }

            this.Nodes.Insert(index, SubNode);
        }

        public HtmlTreeTagNode()
            : base()
        {
            Type = NodeType.TreeTag;
            Nodes = new List<HtmlNode>();
        }

        public HtmlTreeTagNode(string Tag)
            : this()
        {
            this.TagName = Tag;
        }

        public HtmlTreeTagNode(HtmlTagNode tag)
            : this()
        {
            this.TagName = tag.TagName;
            this.IsSole = tag.IsSole;
            this.Attrs = tag.Attrs;
            this.Level = tag.Level;

            if (tag.Type == NodeType.TreeTag)
            {
                var tagTree = tag as HtmlTreeTagNode;
                this.Nodes = tagTree.Nodes;
            }
        }

        public static bool operator ==(HtmlTreeTagNode one, HtmlTreeTagNode other)
        {
            if (object.Equals(one, null) && object.Equals(other, null)) return true;

            if (object.Equals(one, null)) return false;
            if (object.Equals(other, null)) return false;


            if (string.Equals(one.TagName, other.TagName, StringComparison.CurrentCultureIgnoreCase) == false) return false;

            if (one.Attrs.Count != other.Attrs.Count) return false;
            if (one.Nodes.Count != other.Nodes.Count) return false;

            foreach (var atr in one.Attrs)
            {
                var otherAtr = other.Attrs.FirstOrDefault(o => string.Equals(o.Name, atr.Name, StringComparison.CurrentCultureIgnoreCase));
                if (otherAtr == null) return false;

                if (string.Equals(atr.Value, otherAtr.Value, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    return false;
                }
            }


            for (var i = 0; i < one.Nodes.Count; i++)
            {
                var oneChild = one.Nodes[i];
                var otherChild = other.Nodes[i];

                if (oneChild != otherChild) return false;
            }

            return true;
        }

        public static bool operator !=(HtmlTreeTagNode one, HtmlTreeTagNode other)
        {
            return !(one == other);
        }

        public string ToSingleNodeString()
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
                return retVal;
            }
            else retVal += ">";


            retVal += "</" + TagName + ">";

            return retVal;
        }

        public override string ToString()
        {
            StringLinker retVal = "";

            if (this.TagName.HasValue())
            {
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
                    return retVal;
                }
                else retVal += ">";
            }


            foreach (var item in Nodes)
            {
                retVal += item.ToString();
            }

            if (this.TagName.HasValue())
            {
                retVal += "</" + TagName + ">";
            }

            return retVal;
        }

        public void SetInnerText(string Txt)
        {
            this.Nodes = new List<HtmlNode> { new HtmlTextNode() { Text = Txt } };
        }



        public override object Clone()
        {
            var obj = this.MemberwiseClone() as HtmlTreeTagNode;
            obj.Attrs = this.Attrs.Select(o => o.Clone() as HtmlAttrNode).ToList();
            obj.Nodes = this.Nodes.Select(o => o.Clone() as HtmlNode).ToList();

            return obj;
        }
        public HtmlTreeTagNode FindDom(string DomSelector)
        {
            var ret = FindDoms(DomSelector);
            if (ret == null || ret.Count == 0) return null;
            return ret.First();
        }

        //private List<HtmlTreeTagNode> PointFilter(List<HtmlTreeTagNode> ranges, string selector)
        //{
        //    //:eq
        //    var eqValue = selector.TakeOut(":eq(", ")").AsInt();

        //    if (selector.Contains(":first"))
        //    {
        //        eqValue = 0;
        //    }
        //    else if (selector.Contains(":last"))
        //    {
        //        eqValue = ranges.Count - 1;
        //    }

        //    if (eqValue.HasValue())
        //    {
        //        if (eqValue >= ranges.Count)
        //        {
        //            return null;
        //        }

        //        return new List<HtmlTreeTagNode>() { ranges[eqValue] };
        //    }

        //    return ranges;
        //}


        ///// <summary>
        ///// 按选择器进行查找。
        ///// </summary>
        ///// <param name="DomSelector">支持  id(#) (#必须独立存在), class(.) ,tag , 直接子级(>) </param>
        ///// <returns></returns>
        //public List<HtmlTreeTagNode> FindDoms(string DomSelector)
        //{
        //    var currNodes = new List<HtmlTreeTagNode>() { this };
        //    //拆分成几部分。
        //    var eachSects = DomSelector.SplitWithSeperator(":first", ":last", ":eq(");
        //    for (var i = 0; i < eachSects.Count; i++)
        //    {
        //        //sect:  :first >.item a
        //        //sect:  :eq(1) >.item a
        //        var sect = eachSects[i];

        //        currNodes.All(curr =>
        //        {
        //            return true;
        //        });
        //        var findRet = curr.FindAllDoms(sect);

        //        if (findRet.HasData())
        //        {
        //            return new List<HtmlTreeTagNode>();
        //        }


        //        if (i == eachSects.Count - 1)
        //        {
        //            return findRet;
        //        }


        //        findRet.All(retNode =>
        //        {

        //            return true;
        //        });
        //    }
        //    return new List<HtmlTreeTagNode>();
        //}



        ///// <summary>
        ///// 从多个Dom树及子Dom中匹配某一个选择器。
        ///// </summary>
        ///// <param name="treeNodes"></param>
        ///// <param name="selector"></param>
        ///// <returns></returns>
        //private static List<HtmlTreeTagNode> _FindDoms(List<HtmlTreeTagNode> treeNodes, string selector)
        //{
        //    var ret = new List<HtmlTreeTagNode>();
        //    treeNodes.All(node =>
        //    {
        //        var ranges = _FindDoms(node, selector);

        //        if (ranges.HasData() == false)
        //        {
        //            return true;
        //        }

        //        ret.AddRange(ranges);
        //        return true;
        //    });
        //    return ret;
        //}

        /// <summary>
        /// 从一个Dom树及子Dom中匹配某一个选择器。
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private List<HtmlTreeTagNode> _FindDoms(string selector)
        {
            var ret = new List<HtmlTreeTagNode>();
            new Recursion<HtmlNode>().Execute(this.Nodes, o =>
            {
                if (o.Type == NodeType.TreeTag)
                {
                    return (o as HtmlTreeTagNode).Nodes;
                }
                else return null;
            }, (tag, level, currentIndex) =>
            {
                var treeTag = tag as HtmlTreeTagNode;
                if (treeTag == null || treeTag.Parent == null)
                {
                    return RecursionReturnEnum.Go;
                }

                var node = treeTag.singleMatchDom(selector, currentIndex);
                if (node == null)
                {
                    if (selector.StartsWith(">"))
                    {
                        return RecursionReturnEnum.StopSub;
                    }
                    return RecursionReturnEnum.Go;
                }

                ret.Add(node);

                if (selector.StartsWith(">"))
                {
                    return RecursionReturnEnum.StopSub;
                }

                return RecursionReturnEnum.Go;
            });
            return ret;
        }



        /// <summary>
        /// 按选择器进行查找。
        /// </summary>
        /// <param name="DomSelector">支持  id(#) (#必须独立存在), class(.) ,tag , 直接子级(>) </param>
        /// <returns></returns>
        public List<HtmlTreeTagNode> FindDoms(string DomSelector)
        {
            var currNodes = new List<HtmlTreeTagNode>() { this };
            //拆分成几部分。
            var eachSects = DomSelector.SplitWithSeperator(":first", ":last", ":eq(");
            ProcHead2Tail(eachSects);

            for (var i = 0; i < eachSects.Count; i++)
            {
                if (currNodes.HasData() == false)
                {
                    return new List<HtmlTreeTagNode>();
                }
                var sect = eachSects[i];

                var filterNodes = new List<HtmlTreeTagNode>();

                //从当前结果集中向下查找 selector
                currNodes.All(curr =>
                {
                    var findRet = curr.findDoms(sect);

                    if (findRet.HasData())
                    {
                        filterNodes.AddRange(findRet);
                    }

                    return true;
                });

                if (filterNodes.HasData() == false)
                {
                    return new List<HtmlTreeTagNode>();
                }


                if (sect.EndsWith(":first"))
                {
                    filterNodes = new List<HtmlTreeTagNode>() { filterNodes.First() };
                }
                else if (sect.EndsWith(":last"))
                {
                    filterNodes = new List<HtmlTreeTagNode>() { filterNodes.Last() };
                }
                else if (sect.Contains(":eq("))
                {
                    var eq = sect.TakeOut(":eq(", ")").AsInt();
                    if (eq < 0 || eq >= filterNodes.Count) return new List<HtmlTreeTagNode>();

                    filterNodes = new List<HtmlTreeTagNode>() { filterNodes[eq] };
                }

                if (i == eachSects.Count - 1)
                {
                    return filterNodes;
                }
                currNodes = filterNodes;
                //currNodes = getSubNodes(filterNodes);
            }
            return new List<HtmlTreeTagNode>();
        }

        private void ProcHead2Tail(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var sect = list[i];

                if (sect.StartsWith(":first"))
                {
                    list[i - 1] += ":first";
                    list[i] = list[i].Slice(":first".Length);
                }
                else if (sect.StartsWith(":last"))
                {
                    list[i - 1] += ":last";
                    list[i] = list[i].Slice(":last".Length);
                }
                else if (sect.StartsWith(":eq("))
                {
                    var p = sect.Slice(0, sect.IndexOf(")") + 1);
                    list[i - 1] += p;
                    list[i] = list[i].Slice(p.Length);
                }
            }

            if (list.Last().HasValue() == false)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        /// <summary>
        /// 从多个Dom树节点中向下查找整串选择器,开头是 :first ：last 部分。
        /// </summary>
        /// <param name="Nodes"></param>
        /// <param name="DomSelector"></param>
        /// <returns></returns>
        private List<HtmlTreeTagNode> findDoms(string DomSelector)
        {
            var currNodes = new List<HtmlTreeTagNode>() { this };

            var sects = DomSelector.SplitWithSeperator(">", " ").Select(o => o.Trim()).Where(o => o.HasValue()).ToList();

            for (int i = 0; i < sects.Count; i++)
            {
                if (currNodes.HasData() == false)
                {
                    return new List<HtmlTreeTagNode>();
                }

                var sect = sects[i];

                var filterNodes = new List<HtmlTreeTagNode>();

                currNodes.All(curr =>
                {
                    var findRet = curr._FindDoms(sect);

                    if (findRet.HasData())
                    {
                        filterNodes.AddRange(findRet);
                    }

                    return true;
                });

                if (i == sects.Count - 1)
                {
                    return filterNodes;
                }
                currNodes = filterNodes;

                //currNodes = getSubNodes(filterNodes);
            }

            return new List<HtmlTreeTagNode>();
        }


        private static List<HtmlTreeTagNode> getSubNodes(List<HtmlTreeTagNode> filterNodes)
        {
            var ret = new List<HtmlTreeTagNode>();
            filterNodes.All(o =>
            {
                ret.AddRange(o.Nodes.Where(n => n.Type == NodeType.TreeTag).Select(n => n as HtmlTreeTagNode));
                return true;
            });

            return ret;
        }


        /// <summary>
        /// 匹配单个元素
        /// </summary>
        /// <param name="DomSelector"></param>
        /// <param name="DomSelector"></param>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        public HtmlTreeTagNode singleMatchDom(string DomSelector, int currentIndex)
        {
            var tag = this;
            if (DomSelector.StartsWith(">"))
            {
                DomSelector = DomSelector.Slice(1);
            }
            var sects = DomSelector.SplitWithSeperator("#", ".", ":");
            for (var i = 0; i < sects.Count; i++)
            {
                var item = sects[i];
                if (item.StartsWith("#"))
                {
                    if (tag.GetAttributeValue("Id") == item.Slice(1))
                    {
                        continue;
                    }
                    else return null;
                }
                else if (item.StartsWith("."))
                {
                    var cls = tag.GetAttributeValue("class").AsString();
                    if ((" " + cls + " ").Contains(" " + item.Slice(1) + " "))
                    {
                        continue;
                    }
                    else return null;
                }
                else if (item.StartsWith(":"))
                {
                    tag = tag.maoHaoFilter(item, currentIndex);
                    if (tag == null)
                    {
                        return null;
                    }
                }
                else if (item == "*")
                {
                    continue;
                }
                else
                {
                    if (string.Equals(item, tag.TagName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else return null;
                }
            }
            return tag;
        }

        private HtmlTreeTagNode maoHaoFilter(string item, int currentIndex)
        {
            HtmlTreeTagNode tag = this;
            if (item == ":next")
            {
                if (currentIndex >= tag.Parent.Nodes.Count - 1) return null;

                currentIndex++;
                tag = tag.Parent.Nodes[currentIndex] as HtmlTreeTagNode;
                if (tag == null) return null;
                return tag;
            }
            else if (item == ":prev")
            {
                if (currentIndex <= 0) return null;
                currentIndex--;
                tag = tag.Parent.Nodes[currentIndex] as HtmlTreeTagNode;
                if (tag == null) return null;
                return tag;
            }


            //:contains()
            if (item.Contains(":contains("))
            {
                var inner = item.TakeOut(":contains(", ")");
                if (inner.HasValue())
                {
                    var list = new List<HtmlTreeTagNode>();

                    if (tag.GetInnerText().IndexOf(inner, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return tag;
                    }
                    return null;
                }
            }

            return tag;
        }
    }
}
