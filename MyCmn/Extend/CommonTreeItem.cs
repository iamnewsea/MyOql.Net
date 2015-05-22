//using System;
//using System.Collections.Generic;
//using MyCmn.Visualizer;
//using System.Diagnostics;
//using System.Runtime.Serialization;
//using System.Linq;
//using System.Collections;

//namespace MyCmn
//{
//    /// <summary>
//    /// 通用树对象
//    /// </summary>
//    [DebuggerVisualizer(typeof(CommonTreeItemVisualizer))]
//    [Serializable]
//    public class CommonTreeItem<T> : IModel
//        where T : class
//    {
//        /// <summary>
//        /// 取字典里的 Id 值。
//        /// </summary>
//        public int Id { get; set; }

//        /// <summary>
//        /// 取字典里的 Name 值。
//        /// </summary>
//        public string Name { get; set; }

//        public List<CommonTreeItem<T>> Children { get; set; }

//        public T Entity { get; set; }

//        public CommonTreeItem<T> Parent { get; set; }

//        public CommonTreeItem(T entity)
//        {
//            this.Children = new List<CommonTreeItem<T>>();
//            this.Entity = entity;
//        }

//        public object Clone()
//        {
//            var ret = new CommonTreeItem<T>(this.Entity);
//            ret.Parent = this.Parent.Clone() as CommonTreeItem<T>;
//            ret.Children.AddRange(this.Children.Select(o => o.Clone() as CommonTreeItem<T>));
//            return ret;
//        }

//        public override string ToString()
//        {
//            var lst = new List<string>();
//            if (this.Id > 0)
//            {
//                lst.Add(this.Id.ToString());
//            }
//            if (this.Name.HasValue())
//            {
//                lst.Add(this.Name);
//            }
//            if (this.Entity != null)
//            {
//                if (this.Entity is IDictionary)
//                {
//                    lst.Add(this.Entity.ToJson());
//                }
//                else
//                {
//                    lst.Add(this.Entity.ToString());
//                }
//            }

//            return lst.Join(",");
//        }

//        public string RenderString(Func<CommonTreeItem<T>, string> EachFunc)
//        {
//            return EachFunc(this);
//        }
//    }
//}
