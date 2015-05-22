using System;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Configuration;
using System.Drawing.Drawing2D;

namespace MyCmn
{
    public static partial class WebFormHelper
    {
        /// <summary>
        /// 该方法,是给 Button 类用的. [★]
        /// </summary>
        /// <param name="btn"></param>
        public static void ProcOneClick(this Button btn)
        {
            if (btn.Attributes["___ProcOneClicked"] == true.ToString()) return;

            btn.Attributes.Add(
                "onclick", btn.Attributes["onclick"] + @"this.disabled=true;"
                + (HttpContext.Current.CurrentHandler as Page).ClientScript.GetPostBackEventReference(btn, "")
                );

            btn.Attributes["___ProcOneClicked"] = true.ToString();
        }


        /// <summary>
        /// 根据 Value , 递归查找节点并删除. [★]
        /// </summary>
        /// <param name="MyTree"></param>
        /// <param name="NodeValue"></param>
        public static void RemoveNodeByValue(this TreeView MyTree, string NodeValue)
        {
            RemoveNodeByValue(MyTree.Nodes,NodeValue);
        }


        /// <summary>
        /// 根据 Value , 递归查找节点并删除. [★]
        /// </summary>
        /// <param name="MyTreeNodes"></param>
        /// <param name="NodeValue"></param>
        private static void RemoveNodeByValue(this TreeNodeCollection MyTreeNodes, string NodeValue)
        {
            for (int i = 0; i < MyTreeNodes.Count; i++)
            {
                if (MyTreeNodes[i].Value == NodeValue)
                {
                    MyTreeNodes.Remove(MyTreeNodes[i]);
                    break;
                }

                RemoveNodeByValue(MyTreeNodes[i].ChildNodes,NodeValue);
            }
        }
        /// <summary>
        /// 根据 Value 递归查找节点 [★]
        /// </summary>
        /// <param name="MyTree"></param>
        /// <param name="NodeValue"></param>
        /// <returns></returns>
        public static TreeNode FindNodeByValue(this TreeView MyTree, string NodeValue)
        {
            return FindNodeByValue(MyTree.Nodes, NodeValue);
        }

        /// <summary>
        /// 根据 Value 递归查找节点 [★]
        /// </summary>
        /// <param name="MyTreeNodes"></param>
        /// <param name="NodeValue"></param>
        /// <returns></returns>
        private static TreeNode FindNodeByValue(this TreeNodeCollection MyTreeNodes, string NodeValue)
        {
            for (int i = 0; i < MyTreeNodes.Count; i++)
            {
                if (MyTreeNodes[i].Value == NodeValue)
                {
                    return MyTreeNodes[i];
                }

                TreeNode tn = FindNodeByValue(MyTreeNodes[i].ChildNodes,NodeValue);
                if (tn != null) return tn;
            }

            return null;
        }



        /// <summary>
        /// 针对 GridView 列的显示名称过长,设置控件里文本的格式。 [★]
        /// </summary>
        /// <param name="TheWebControlWithText">要设置显示文本的控件。</param>
        public static void SetDisplayText(this WebControl TheWebControlWithText)
        {
            SetDisplayText(TheWebControlWithText, -1);
        }

        /// <summary>
        /// 针对 GridView 列的显示名称过长, 设置控件里文本的格式。 [★]
        /// </summary>
        /// <param name="TheWebControlWithText">要设置显示文本的控件。</param>
        /// <param name="DisplayLength">要设置格式化显示文本的长度。</param>
        public static void SetDisplayText(this WebControl TheWebControlWithText, int DisplayLength)
        {
            Type type = TheWebControlWithText.GetType();
            PropertyInfo piText = type.GetProperty("Text");
            if (piText == null) return;

            object objText = piText.GetValue(TheWebControlWithText, null);
            if (objText == null) return;

            MyCmn.ValueProc.DisplayText disValue = ValueProc.AsString(objText).GetDisplayText(DisplayLength);

            piText.SetValue(TheWebControlWithText, disValue.Text, null);

            if (disValue.ToolTip.HasValue() == false) return;

            PropertyInfo piWithToolTip = type.GetProperty("ToolTip");
            if (piWithToolTip != null)
            {
                piWithToolTip.SetValue(TheWebControlWithText, disValue.ToolTip, null);
            }
        }
    }
}