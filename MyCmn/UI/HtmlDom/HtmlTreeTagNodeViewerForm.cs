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
    public partial class HtmlTreeTagNodeViewerForm : Form
    {
        private HtmlTreeTagNode HtmlNode;

        public HtmlTreeTagNodeViewerForm()
        {
        }

        public HtmlTreeTagNodeViewerForm(HtmlTreeTagNode set)
        {
            this.HtmlNode = set;
            this.HtmlNode.ResetParent();

            if (this.HtmlNode == null) return;

            InitializeComponent();

            InitData(this.HtmlNode);
        }

        public string TreeName;


        private void InitData(object set, TreeNode parentNode = null, int Level = 0)
        {
            var node = AddNode(set, parentNode);

            if (node == null)
            {
                return;
            }

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

                }
            }

            if (parentNode == null)
            {
                var node = new TreeNode();
                node.Text = text.Slice(0, 200);
                node.Tag = value;
                this.treeView1.Nodes.Add(node);
                return node;
            }
            else
            {
                var node = new TreeNode();
                node.Text = text.Slice(0, 200);
                node.Tag = value;
                parentNode.Nodes.Add(node);
                return node;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode == null)
            {
                return;
            }

            this.treeView1.SelectedNode.ExpandAll();
        }

        private void btnLocate_Click(object sender, EventArgs e)
        {
            if (this.treeView1.Nodes.Count == 0) return;
            this.FirstItem = null;

            this.treeView1.Nodes[0].ExpandAll();
            var searchNodes = this.HtmlNode.FindDoms(this.txtQuery.Text);

            this.btnLocate.Text = "定位(" + searchNodes.Count + ")";

            new Recursion<TreeNode>().Execute(this.treeView1.Nodes.ToMyList(o => o as TreeNode), o => o.Nodes.ToMyList(n => n as TreeNode), (n, l, index) =>
                {
                    searchNodes.All(s =>
                    {
                        if (s == (n.Tag as HtmlTagNode))
                        {
                            if (this.FirstItem == null)
                            {
                                this.FirstItem = n;
                            }

                            n.ForeColor = Color.Red;
                            n.BackColor = Color.YellowGreen;
                        }
                        else
                        {
                            n.ForeColor = Color.Black;
                            n.BackColor = Color.White;
                        }
                        return true;
                    });
                    return RecursionReturnEnum.Go;
                });

            if (this.FirstItem != null)
            {
                this.treeView1.TopNode = this.FirstItem;
            }
        }


        public TreeNode FirstItem { get; set; }
    }
}
