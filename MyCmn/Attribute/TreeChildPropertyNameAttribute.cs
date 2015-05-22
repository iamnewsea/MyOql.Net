using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyCmn;

namespace MyCmn
{
    public class TreeChildPropertyNameAttribute : Attribute
    {
        public TreeChildPropertyNameAttribute(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; set; }
    }
}