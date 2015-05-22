using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyCmn;

namespace MyCmn
{
    /// <summary>
    /// 命令行参数分析
    /// </summary>
    public class CmdArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CommandSign">命令分隔符,如 - , / </param>
        /// <param name="CommandString">命令字符串</param>
        public CmdArgs(string CommandString, char CommandSign)
        {
            CmdSign = CommandSign;
            Data = new Dictionary<string, string[]>();
            var load = new HtmlCharLoad(CommandString.ToCharArray());
            load.LoadCmdArgs(CommandSign)
                .All(o =>
                         {
                             Data[o.Cmd] = o.Args;
                             return true;
                         });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CommandSign">命令分隔符,如 - , / </param>
        /// <param name="args">命令行参数. 只取 以 CommandSign 开始的部分.</param>
        public CmdArgs(string[] args, char CommandSign)
        {
            CmdSign = CommandSign;
            Data = new Dictionary<string, string[]>();

            for (int i = 0; i < args.Length; i++)
            {
                string cmd = args[i];

                if (cmd.StartsWith(CmdSign.ToString()))
                {
                    var list = new List<string>();
                    for (i++; i < args.Length; i++)
                    {
                        string arg = args[i];
                        if (arg.StartsWith(CmdSign.ToString()))
                        {
                            i--;
                            break;
                        }
                        list.Add(arg);
                    }

                    Data[cmd.Substring(1)] = list.ToArray();
                }
            }
        }

        /// <summary>
        /// 命令分隔符.
        /// </summary>
        protected char CmdSign;

        /// <summary>
        /// 分隔出的各个部分.
        /// </summary>
        public Dictionary<string, string[]> Data;

        public override string ToString()
        {
            if (Data == null || Data.Count == 0) return string.Empty;

            var sl = new StringLinker();
            for (int i = 0; i < Data.Keys.Count; i++)
            {
                string key = Data.Keys.ElementAt(i);
                if (i > 0) sl += " ";
                sl += CmdSign + key + " " + string.Join(" ", Data[key]);
            }

            return sl;
        }

        /// <summary>
        /// 把Data的各个部分赋给某实体, 按属性名匹配.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retVal"></param>
        /// <returns></returns>
        public T ToModel<T>(T retVal) where T : class
        {
            Type type = typeof(T);
            {
                PropertyInfo[] ps = type.GetProperties();
                foreach (string key in Data.Keys)
                {
                    PropertyInfo p = ps.FirstOrDefault(o => o.Name == key);

                    if (p == null) continue;

                    if (p.PropertyType.IsArray)
                    {
                        p.SetValue(retVal, Data[key], null);
                    }
                    else
                    {
                        p.SetValue(retVal, ValueProc.AsType(p.PropertyType, Data[key][0]), null);
                    }
                }
            }


            return retVal;
        }
    }
}