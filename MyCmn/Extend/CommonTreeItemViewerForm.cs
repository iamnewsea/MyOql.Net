using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MyCmn;
using System.Collections;


namespace MyCmn.Visualizer
{
    public partial class CommonTreeItemViewerForm : Form
    {
        private object Set;

        private Color[] colors = new Color[] { Color.AliceBlue, Color.AntiqueWhite, Color.Beige, Color.GhostWhite, Color.Azure, Color.MintCream, Color.Lavender, Color.PapayaWhip, Color.Snow };
        public CommonTreeItemViewerForm()
        {
        }

        public CommonTreeItemViewerForm(object set)
        {
            this.Set = set;
            this.Type = set.GetType();
            //this.ChildProperty = this.Type.GetProperty("Nodes");

            InitializeComponent();

            InitData(set);

        }

        public string TreeName;


        private void InitData(object set, TreeNode parentNode = null, int Level = 0)
        {
            var node = AddNode(set, parentNode);

            if (node == null)
            {
                return;
            }

            var color = colors[Level % colors.Length];

            var p = set.GetType().GetProperty("Nodes");
            if (p == null) return;

            var child = p.GetValue(set, null) as IList;
            if (child != null)
            {
                for (int i = 0; i < child.Count; i++)
                {
                    var item = child[i];

                    InitData(item, node, Level++);
                }
            }
        }

        private TreeNode AddNode(object value, TreeNode parentNode = null)
        {
            string text = string.Empty;
            var dict = value as IDictionary;
            if (dict != null)
            {
                text = dict.ToJson();
            }
            else if (this.Type.IsPrimitive)
            {
                text = value.AsString();
            }
            else
            {
                if (value is HtmlTagNode)
                {
                    text = (value as HtmlTagNode).ToHtmlString();
                }
                else if (value is HtmlCloseTagNode)
                {
                    text = (value as HtmlCloseTagNode).ToString();
                }
                else if (value is HtmlTextNode)
                {
                    text = (value as HtmlTextNode).ToString();
                    if (text.HasValue() == false)
                    {
                        return null;
                    }
                    if (text.Replace("<br />", "").Replace("&nbsp;", "").Replace(Environment.NewLine, "").Replace("\t", "").Trim().HasValue() == false)
                    {
                        return null;
                    }
                }
                else
                {

                    text = value.AsString();

                    if (text == this.Type.FullName)
                    {
                        text = value.ToJson();
                    }
                }
            }

            if (parentNode == null)
            {
                return this.treeView1.Nodes.Add(text.Slice(0, 200));
            }
            else
            {
                return parentNode.Nodes.Add(text.Slice(0, 200));
            }
        }


        public Type Type { get; set; }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode == null)
            {
                return;
            }

            this.treeView1.SelectedNode.ExpandAll();
        }
 

        //public PropertyInfo ChildProperty { get; set; }
    }
}
