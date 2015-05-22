using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;

namespace MyCmn
{
    public class PowerRowJson
    {
        /// <summary>
        /// 字典的Key值保存数据库的DbName
        /// </summary>
        public Dictionary<string, MyBigInt> View { get; set; }
        /// <summary>
        /// 字典的Key值保存数据库的DbName
        /// </summary>
        public Dictionary<string, MyBigInt> Edit { get; set; }
        /// <summary>
        /// 字典的Key值保存数据库的DbName
        /// </summary>
        public Dictionary<string, MyBigInt> Delete { get; set; }

        public PowerRowJson()
        {
            this.View = new Dictionary<string, MyBigInt>();
            this.Edit = new Dictionary<string, MyBigInt>();
            this.Delete = new Dictionary<string, MyBigInt>();
        }

        public static PowerRowJson operator &(PowerRowJson Power1, PowerRowJson Power2)
        {
            if (Power1 == null) return null;
            if (Power2 == null) return null;

            PowerRowJson retVal = new PowerRowJson();

            Power1.View.Keys.Intersect(Power2.View.Keys).All(o =>
            {
                retVal.View[o] = Power1.View[o] & Power2.View[o];
                return true;
            });

            Power1.Edit.Keys.Intersect(Power2.Edit.Keys).All(o =>
            {
                retVal.View[o] = Power1.Edit[o] & Power2.Edit[o];
                return true;
            });

            Power1.Delete.Keys.Intersect(Power2.Delete.Keys).All(o =>
            {
                retVal.View[o] = Power1.Delete[o] & Power2.Delete[o];
                return true;
            });


            return retVal;
        }

        public static PowerRowJson operator |(PowerRowJson Power1, PowerRowJson Power2)
        {
            if (Power1 == null) return Power2;
            if (Power2 == null) return Power1;

            PowerRowJson retVal = new PowerRowJson();

            Power1.View.Keys.Union(Power2.View.Keys).All(o =>
            {
                retVal.View[o] = Power1.View.GetOrDefault(o) | Power2.View.GetOrDefault(o);
                return true;
            });

            return retVal;
        }


        public override string ToString()
        {
            return string.Format(@"{{View:{{{0}}},Edit:{{{1}}},Delete:{{{2}}}}}",
                string.Join(",", this.View.Select(o => o.Key + @":""" + o.Value.ToString() + @"""").ToArray()),
                string.Join(",", this.Edit.Select(o => o.Key + @":""" + o.Value.ToString() + @"""").ToArray()),
                string.Join(",", this.Delete.Select(o => o.Key + @":""" + o.Value.ToString() + @"""").ToArray())
                );
        }
    }
    /// <summary>
    /// 内容简称： ABCDRU
    /// </summary>
    public class PowerJson
    {
        //public MyBigInt Create { get; set; }
        //public MyBigInt Read { get; set; }
        //public MyBigInt Update { get; set; }
        //public MyBigInt Delete { get; set; }

        /// <summary>
        /// 页面权限
        /// </summary>
        public MyBigInt Action { get; set; }

        /// <summary>
        /// 按钮权限.
        /// </summary>
        public MyBigInt Button { get; set; }

        /// <summary>
        /// 行集权限
        /// </summary>
        public PowerRowJson Row { get; set; }

        public PowerJson()
        {
            this.Action = new MyBigInt();
            this.Button = new MyBigInt();
            this.Row = new PowerRowJson();
        }

        /// <summary>
        /// 从ToString 结果反构造.
        /// </summary>
        /// <param name="Value"></param>
        public PowerJson(string Value)
            : this()
        {
            //{Create:"0",Read:"0",Update:"0",Delete:"0",Action:"FFFFFFF",Button:"3FF,FFFFFFFF",Row:{View:{Menu:"3000,800,200001EC,0,0,7FFFF,E7CFFFFF,97FC2FE"},Edit:{},Delete:{},IsMax:false},IsMax:false}

            if (string.IsNullOrEmpty(Value))
            {
                return;
            }

            Dictionary<string, string> twoValue = ToDict(Value);//.FromJson<Dictionary<string, object>>();

            //<string, string>>();
            //this.Create = new MyBigInt(dict.GetOrDefault("Create"));
            //this.Read = new MyBigInt(dict.GetOrDefault("Read"));
            //this.Update = new MyBigInt(dict.GetOrDefault("Update"));
            //this.Delete = new MyBigInt(dict.GetOrDefault("Delete"));
            //this.Action = new MyBigInt(dict.GetOrDefault("Action"));
            //this.Button = new MyBigInt(dict.GetOrDefault("Button"));
            //this.Row = new Dictionary<string, MyBigInt>();


            this.Row = new PowerRowJson();
            var row = ToDict(twoValue.GetOrDefault("Row"));// as Newtonsoft.Json.Linq.JObject;
            if (row != null)
            {
                var rowView = ToDict(row.GetOrDefault("View"));//as JObject;
                var rowEdit = ToDict(row.GetOrDefault("Edit"));//as JObject;

                if (rowView != null)
                {
                    this.Row.View = rowView.ToDictionary(o => o.Key, o => new MyBigInt(o.Value.AsString()));
                }

                if (rowEdit != null)
                {
                    this.Row.Edit = rowEdit.ToDictionary(o => o.Key, o => new MyBigInt(o.Value.AsString()));
                }
            }

            this.Action = new MyBigInt(twoValue.GetOrDefault("Action").AsString());
            this.Button = new MyBigInt(twoValue.GetOrDefault("Button").AsString());
        }

        private Dictionary<string, string> ToDict(string Value)
        {
            //{Create:"0",Read:"0",Update:"0",Delete:"0",Action:"FFFFFFF",Button:"3FF,FFFFFFFF",Row:{View:{Menu:"3000,800,200001EC,0,0,7FFFF,E7CFFFFF,97FC2FE"},Edit:{},Delete:{},IsMax:false},IsMax:false}
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(Value)) return dict;
            Value = Value.Trim();
            if (Value == string.Empty) return dict;

            if (!Value.StartsWith("{") || !Value.EndsWith("}")) throw new Exception("非法Json");

            /*
             * 规则：
             * 1. 反斜线是转义，反斜线后面的字符可忽略规则。
             * 2. 引号是整体
             * 3. 冒号分词
             * 4. { } 算是一个整体 可以无限级。
             */
            Value = Value.Substring(1, Value.Length - 2);

            for (var i = 0; i < Value.Length; i++)
            {
                int keyEndIndex = FindNext(Value, i, ':');

                var key = Value.Substring(i, keyEndIndex - i).Trim();

                var valueEndIndex = FindNext(Value, keyEndIndex + 1, ',');
                var val = Value.Substring(keyEndIndex + 1, valueEndIndex - keyEndIndex - 1).Trim();

                i = valueEndIndex;

                if (key.StartsWith(@"""") && key.EndsWith(@"""")) key = key.Substring(1, key.Length - 2);
                if (val.StartsWith(@"""") && val.EndsWith(@"""")) val = val.Substring(1, val.Length - 2);

                dict[key] = val;
            }

            return dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="pos"></param>
        /// <param name="findChar"></param>
        /// <returns>找不到返回 Value.Length </returns>
        private static int FindNext(string Value, int pos, char findChar)
        {
            /*
             * 规则：
             * 1. 反斜线是转义，反斜线后面的字符可忽略规则。
             * 2. 双引号是整体,单引号是整体
             * 3. {} 是整体，[] 是整体。
             * 4. 冒号分词
             */

            //结束
            if (pos == Value.Length) return Value.Length;
            int ClsLevel = 0;
            int AryLevel = 0;

            bool inQuote1 = false;
            bool inQuote2 = false;

            for (int i = pos; i < Value.Length; i++)
            {
                var item = Value[i];
                if (item == '\\')
                {
                    i++;
                    continue;
                }

                if (ClsLevel == 0 && AryLevel == 0 && inQuote1 == false && inQuote2 == false)
                {
                    if (findChar == item) return i;

                    //如果查找的是 '"{}[],则返回上一个字符。
                    if (i > pos && Value[i - 1] == findChar) return i - 1;
                }

                if (inQuote1)
                {
                    if (item == '\'')
                    {
                        inQuote1 = !inQuote1;
                    }

                    continue;
                }

                if (inQuote2)
                {
                    if (item == '"')
                    {
                        inQuote2 = !inQuote2;
                    }

                    continue;
                }


                if (inQuote1 == false && inQuote2 == false)
                {
                    if (item == '\'')
                    {
                        inQuote1 = true;
                        continue;
                    }

                    if (item == '"')
                    {
                        inQuote2 = true;
                        continue;
                    }
                }

                if (item == '{')
                {
                    ClsLevel++;
                    continue;
                }

                if (item == '}')
                {
                    ClsLevel--;
                    continue;
                }

                if (item == '[')
                {
                    AryLevel++;
                    continue;
                }

                if (item == ']')
                {
                    AryLevel--;
                    continue;
                }
            }

            if (ClsLevel == 0 && AryLevel == 0 && inQuote1 == false && inQuote2 == false)
            {
                //如果查找的是 '"{}[],则返回上一个字符。
                if (Value.Length - 1 > pos && Value[Value.Length - 1] == findChar) return Value.Length - 1;
            }

            return Value.Length;
        }

        private static string GetJsonObjValue(string Value, string Key)
        {
            if (string.IsNullOrEmpty(Value) || string.IsNullOrEmpty(Key)) return string.Empty;
            //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
            var strJson = Value.Trim();
            if (string.IsNullOrEmpty(Value)) return string.Empty;

            if (!(strJson.StartsWith("{") && strJson.EndsWith("}"))) throw new Exception("非法ObjectJson");


            /*
             * 规则：
             * 1. 反斜线是转义，反斜线后面的字符可忽略规则。
             * 2. 引号是整体,{ } ,[] 算是一个整体 可以无限级。
             * 3. 冒号分词
             */
            strJson = strJson.Substring(1, strJson.Length - 2);

            for (var i = 0; i < strJson.Length; i++)
            {
                int keyEndIndex = PowerJson.FindNext(strJson, i, ':');

                var key = strJson.Substring(i, keyEndIndex - i).Trim();

                var valueEndIndex = PowerJson.FindNext(strJson, keyEndIndex + 1, ',');

                i = valueEndIndex;

                var val = strJson.Substring(keyEndIndex + 1, valueEndIndex - keyEndIndex - 1).Trim();


                if (key.StartsWith(@"""") && key.EndsWith(@"""")) key = key.Substring(1, key.Length - 2);
                if (val.StartsWith(@"""") && val.EndsWith(@"""")) val = val.Substring(1, val.Length - 2);

                if (string.Equals(key, Key, StringComparison.CurrentCultureIgnoreCase)) return val;
            }

            return string.Empty;
        }

        private static string GetJsonAryValue(string Value, int aryIndex)
        {
            if (string.IsNullOrEmpty(Value) || aryIndex < 0) return string.Empty;
            //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
            var strJson = Value.Trim();
            if (Value == string.Empty) return string.Empty;

            if (!(strJson.StartsWith("[") && strJson.EndsWith("]"))) throw new Exception("非法ArrayJson");


            /*
             * 规则：
             * 1. 反斜线是转义，反斜线后面的字符可忽略规则。
             * 2. 引号是整体,{ } ,[] 算是一个整体 可以无限级。
             */
            strJson = strJson.Substring(1, strJson.Length - 2);

            for (int i = 0, index = 0; i < strJson.Length && index <= aryIndex; i++, index++)
            {
                int keyEndIndex = PowerJson.FindNext(strJson, i, ',');

                if (index == aryIndex)
                {
                    return strJson.Substring(i, keyEndIndex - i);
                }
                else if (keyEndIndex == strJson.Length) return null;

                i = keyEndIndex;
            }

            return string.Empty;
        }

        private static int GetJsonAryCount(string Value)
        {
            if (string.IsNullOrEmpty(Value)) return 0;
            //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
            var strJson = Value.Trim();
            if (Value == string.Empty) return 0;

            if (!(strJson.StartsWith("[") && strJson.EndsWith("]"))) throw new Exception("非法ArrayJson");


            /*
             * 规则：
             * 1. 反斜线是转义，反斜线后面的字符可忽略规则。
             * 2. 引号是整体,{ } ,[] 算是一个整体 可以无限级。
             */
            strJson = strJson.Substring(1, strJson.Length - 2);

            var cont = 0;
            for (int i = 0; i < strJson.Length; i++)
            {
                i = PowerJson.FindNext(strJson, i, ',');
                cont++;
            }

            return cont;
        }

        public static string GetJsonValue(string Value, string Key)
        {
            if (Value == null || Key == null) return string.Empty;

            /*
             * 规则：
             * 1. 反斜线是转义，反斜线后面的字符可忽略规则。
             * 2. 引号是整体,{ } ,[] 算是一个整体 可以无限级。
             * 3. 冒号分词
             */
            foreach (var item in Key.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                var sect = item.Trim();
                //遍历每个部分，查找出 [0] 进行数组的获取
                for (int i = 0; i < sect.Length; i++)
                {
                    var aryBeginIndex = PowerJson.FindNext(sect, i, '[');
                    if (aryBeginIndex < sect.Length)
                    {
                        var aryEndIndex = PowerJson.FindNext(sect, aryBeginIndex, ']');

                        if (aryEndIndex == sect.Length) throw new Exception("发现Key中有不匹配的 [] ");

                        var aryIndex = Convert.ToInt32(sect.Substring(aryBeginIndex + 1, aryEndIndex - aryBeginIndex - 1));

                        if (aryBeginIndex > 0)
                        {
                            var sectKey = sect.Substring(0, aryBeginIndex).Trim();
                            if (sectKey.Length > 0)
                            {
                                Value = GetJsonObjValue(Value, sectKey);
                            }
                        }

                        Value = GetJsonAryValue(Value, aryIndex);
                        i = aryEndIndex;
                    }
                    else
                    {
                        Value = GetJsonObjValue(Value, sect);
                        break;
                    }
                }
            }
            return Value;
        }



        public PowerJson(PowerJson twoValue)
        {
            if (twoValue == null) return;

            this.Action = twoValue.Action;
            this.Button = twoValue.Button;
            this.Row = twoValue.Row;
        }
        /// <summary>
        /// 接收来自客户端的数据。
        /// </summary>
        /// <param name="dict">key权限各项，value 是该权限项的各权限位。</param>
        public PowerJson(Dictionary<string, int[]> data, Dictionary<string, int[]> dataViewRow, Dictionary<string, int[]> dataEditRow, Dictionary<string, int[]> dataDeleteRow)
        {
            if (data != null)
            {
                this.Action = MyBigInt.CreateBySqlRowIds(data.GetOrDefault("Action"));
                this.Button = MyBigInt.CreateBySqlRowIds(data.GetOrDefault("Button"));
            }

            this.Row = new PowerRowJson();
            if (dataViewRow != null)
            {
                dataViewRow.All(o =>
                {
                    this.Row.View[o.Key] = MyBigInt.CreateBySqlRowIds(o.Value);
                    return true;
                });
            }

            if (dataEditRow != null)
            {
                dataEditRow.All(o =>
                {
                    this.Row.Edit[o.Key] = MyBigInt.CreateBySqlRowIds(o.Value);
                    return true;
                });
            }

            if (dataDeleteRow != null)
            {
                dataDeleteRow.All(o =>
                {
                    this.Row.Delete[o.Key] = MyBigInt.CreateBySqlRowIds(o.Value);
                    return true;
                });
            }
        }

        public static PowerJson operator &(PowerJson Power1, PowerJson Power2)
        {
            if (Power1 == null) return null;
            if (Power2 == null) return null;

            PowerJson pj = new PowerJson();
            pj.Action = Power1.Action & Power2.Action;
            pj.Button = Power1.Button & Power2.Button;

            pj.Row = Power1.Row & Power2.Row;
            return pj;
        }

        public static PowerJson operator |(PowerJson Power1, PowerJson Power2)
        {
            if (Power1 == null) return Power2;
            if (Power2 == null) return Power1;


            PowerJson pj = new PowerJson();
            pj.Action = Power1.Action | Power2.Action;
            pj.Button = Power1.Button | Power2.Button;

            pj.Row = Power1.Row | Power2.Row;

            return pj;
        }
        public override string ToString()
        {
            return string.Format(@"{{Action:""{0}"",Button:""{1}"",Row:{2}}}",
                this.Action.AsString(),
                this.Button.AsString(),
                this.Row == null ? new PowerRowJson().ToString() : this.Row.AsString()
                );
        }
    }
}
