using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Text;
using System.Web.Routing;
using System.ComponentModel;
using System.Threading;
using System.Web.Mvc;
using System.Reflection;


namespace MyCmn
{
    public static partial class MyHelper
    {
        private static Dictionary<string, object> Sync_Objects = new Dictionary<string, object>();
        private static object _Sync_Object = new object();

        /// <summary>
        /// FullUnionKey 请以类为前缀，要保持唯一。
        /// </summary>
        /// <param name="FullUnionKey"></param>
        /// <returns></returns>
        public static object GetLockObject(string FullUnionKey)
        {
            if (FullUnionKey == null) return null;

            lock (_Sync_Object)
            {
                if (Sync_Objects.ContainsKey(FullUnionKey) == false)
                {
                    Sync_Objects.Add(FullUnionKey, new object());
                }
            }

            return Sync_Objects[FullUnionKey];
        }

        public static HtmlTreeTagNode LoadHtmlNode2Tree(this List<HtmlNode> Nodes)
        {
            var tree = new HtmlTreeTagNode();

            //var tree = new CommonTreeItem<HtmlNode>(null);

            var currentTree = tree;

            var level = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (level < 0) break;
                var node = Nodes[i];
                node.Level = level;
                if (node.Type == HtmlNode.NodeType.Tag)
                {
                    var tagNode = (node as HtmlTagNode);

                    if (tagNode.IsSole)
                    {
                        var item = new HtmlTreeTagNode(tagNode);
                        item.Parent = currentTree;
                        currentTree.AddNode(item);
                    }
                    else
                    {
                        var item = new HtmlTreeTagNode(tagNode);
                        item.Parent = currentTree;
                        currentTree.AddNode(item);
                        level++;
                        currentTree = item;
                    }
                }
                else if (node.Type == HtmlNode.NodeType.CloseTag)
                {
                    level--;
                    currentTree = currentTree.Parent;
                    continue;
                }
                else if (node.Type == HtmlNode.NodeType.Note || node.Type == HtmlNode.NodeType.Text)
                {
                    //item.Parent = currentTree;
                    currentTree.AddNode(node);
                }
            }

            return tree;
        }


        public static string BeginTag(this HtmlHelper html, HtmlTextWriterTag tag, object Attributes)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var type = Attributes.GetType();

            foreach (var item in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object val = ValueProc.GetMyValue(item, Attributes);// item.GetValue(Attributes);
                dict[item.Name] = val == null ? null : val.AsString();
            }
            foreach (var item in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object val = ValueProc.GetMyValue(item, Attributes);// item.GetValue(Attributes);
                dict[item.Name] = val == null ? null : val.AsString();
            }
            return BeginTag(html, tag, dict);
        }
        public static string BeginTag(this HtmlHelper html, HtmlTextWriterTag tag, Dictionary<string, string> Attributes)
        {
            HtmlTagNode nd = new HtmlTagNode();
            nd.TagName = tag.ToString();
            if (tag.IsIn(
                HtmlTextWriterTag.Br,
                HtmlTextWriterTag.Img,
                HtmlTextWriterTag.Input,
                HtmlTextWriterTag.Link,
                HtmlTextWriterTag.Nobr,
                HtmlTextWriterTag.Meta)
                )
            {
                nd.IsSole = true;
            }
            else nd.IsSole = false;

            if (Attributes != null)
            {
                foreach (var item in Attributes)
                {
                    HtmlAttrNode atr = new HtmlAttrNode();
                    atr.Name = item.Key;
                    string val = item.Value.AsString();
                    if (val.HasValue())
                    {
                        atr.Value = val;
                    }

                    nd.Attrs.Add(atr);
                }
            }

            return nd.ToString();
        }


        public static string EndTag(this HtmlHelper html, HtmlTextWriterTag tag)
        {
            if (tag.IsIn(HtmlTextWriterTag.Br, HtmlTextWriterTag.Img, HtmlTextWriterTag.Input, HtmlTextWriterTag.Link,
                HtmlTextWriterTag.Nobr, HtmlTextWriterTag.Meta)
                )
            {
                return "";
            }
            return new HtmlCloseTagNode() { TagName = tag.ToString() }.ToString();
        }

        ///// <summary>
        ///// 如果当前语言是英文,则显示英文值,否则显示中文值.
        ///// </summary>
        ///// <remarks>
        ///// 只支持中英文.
        ///// </remarks>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="Lang"></param>
        ///// <param name="ZHRes"></param>
        ///// <param name="EnRes"></param>
        ///// <returns></returns>
        //public static T GetRes<T>(this LangEnum Lang, T ZHRes, T EnRes)
        //{
        //    if (MyHelper.Lang == LangEnum.En) return EnRes;

        //    return ZHRes;
        //}



        //private static object _syncObject = new object();
        //private static IRes GetResEvent = null;
        //private static bool FindResEvent = true;
        //public static StringLinker GetRes(this string Key)
        //{
        //    if (GetResEvent == null && !FindResEvent)
        //    {
        //        lock (_syncObject)
        //        {
        //            if (GetResEvent == null)
        //            {
        //                GetResEvent = Activator.CreateInstance(Type.GetType(Configuration.WebConfigurationManager.AppSettings["ResEvent"])) as IRes;
        //                FindResEvent = true;
        //            }

        //        }
        //    }

        //    if (GetResEvent != null)
        //    {
        //        return GetResEvent.GetRes(Key);
        //    }
        //    else return Key;
        //}

        /// <summary>
        ///  得到 Control 的 HTML 解析部分. [★]
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        public static string ToRenderString(this Control con)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter strWrit = new StringWriter(sb);
            HtmlTextWriter htmlWrit = new HtmlTextWriter(strWrit);

            con.RenderControl(htmlWrit);
            return sb.ToString();
        }

    }
}
