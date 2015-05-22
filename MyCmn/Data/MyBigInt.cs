using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace MyCmn
{
    /// <summary>
    /// 大数字表示.用于权限表示.如果用 .net 4.0 + ,可使用：System.Numerics.BigInteger 表示．
    /// </summary>
    /// <remarks>
    /// 只完成了两个逻辑运算 &amp; |,其它未实现.
    /// </remarks>
    [TypeConverter(typeof(MyBigIntConverter))]
    [Serializable]
    public partial class MyBigInt : ICloneable
    {
        public static readonly MyBigInt MaxValue = new MyBigInt() { Max = true };
        public static readonly MyBigInt Zero = new MyBigInt();

        public const char Separator = ',';
        /// <summary>
        /// 符号位，true 为正数， false 为负数。
        /// </summary>
        public bool Sign { get; set; }
        private bool _Max;
        public bool Max
        {
            get { return _Max; }
            set
            {
                _Max = value;
                if (value)
                {
                    this.Data.Clear(); this.Sign = true;
                }
            }
        }

        /// <summary>
        /// 第1个表示 低位
        /// 32,16,21 表示：21:16:32 , 每一个 uint 能表示:32个位.
        /// </summary>
        public List<uint> Data { get; private set; }

        public MyBigInt()
        {
            Max = false;
            Sign = true;
            Data = new List<uint>();
        }

        public MyBigInt(uint value)
            : this()
        {
            Data.Add(value);
        }

        public MyBigInt(int value)
            : this()
        {
            Data.Add((uint)value);
        }

        //public static MyBigInt CreateByInts(string IntWithSeparator)
        //{
        //    MyBigInt ret = new MyBigInt();
        //    if (IntWithSeparator == "MyBigInt.Max")
        //    {
        //        ret.Max = true;
        //        return ret;
        //    }

        //    if (IntWithSeparator[0] == '-')
        //    {
        //        IntWithSeparator = IntWithSeparator.Remove(0, 1);
        //        ret.Sign = false;
        //    }
        //    else ret.Sign = true;


        //    ret.data.AddRange(IntWithSeparator.Split(Separator).Reverse().Select(o => Convert.ToUInt32(o)));
        //    return ret;
        //}

        public MyBigInt(string hexValue)
            : this()
        {
            if (hexValue.HasValue() == false) return;

            if (hexValue == "Max")
            {
                this.Max = true;
                return;
            }
            if (hexValue[0] == '-')
            {
                hexValue = hexValue.Remove(0, 1);
                this.Sign = false;
            }
            else if (hexValue[0] == '+')
            {
                hexValue = hexValue.Remove(0, 1);
                this.Sign = true;
            }
            else this.Sign = true;

            if (hexValue.IndexOf(Separator) >= 0)
            {
                this.Data.AddRange(hexValue.Split(Separator).Reverse().Select(o => uint.Parse(o, System.Globalization.NumberStyles.HexNumber)));
            }
            else
            {
                hexValue = new string('0', 8 - hexValue.Length % 8) + hexValue;
                for (int i = 0; i < hexValue.Length; i = i + 8)
                {
                    this.Data.Add(uint.Parse(hexValue.Substring(i, 8), System.Globalization.NumberStyles.HexNumber));
                }
                this.Data.Reverse();
            }
            this.Tidy();
        }




        /// <summary>
        /// SqlRowId 数据库从1开始的自增值，为0 表示空返回0。
        /// 为1 返回 1 ， 为2 返回2 ， 为3 返回 4，为4 返回8 。
        /// </summary>
        /// <param name="SqlRowId"></param>
        /// <returns></returns>
        public static MyBigInt CreateBySqlRowId(uint SqlRowId)
        {
            if (SqlRowId == 0) return new MyBigInt();

            var power = SqlRowId - 1;

            MyBigInt ret = new MyBigInt();

            uint yu = (power + 1) % 32;
            uint pos0 = (power + 1) / 32;
            if (yu == 0)
            {
                pos0--;
            }

            for (var i = 0; i < pos0; i++)
            {
                ret.Data.Add(0);
            }

            if (yu == 0)
            {
                ret.Data.Add((uint)Math.Pow(2, 31));
            }
            else
            {
                ret.Data.Add((uint)Math.Pow(2, yu - 1));
            }

            ret.Tidy();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static MyBigInt operator &(MyBigInt one, MyBigInt two)
        {
            if (object.Equals(one, null)) return null;
            if (object.Equals(two, null)) return null;

            if (one.Max) return two.Clone();
            if (two.Max) return one.Clone();

            var ret = new MyBigInt();
            ret.Sign = one.Sign & two.Sign;
            for (int i = 0; i < one.Data.Count; i++)
            {
                if (two.Data.Count > i)
                {
                    ret.Data.Add(one.Data[i] & two.Data[i]);
                }
                else break;
            }
            ret.Tidy();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static MyBigInt operator |(MyBigInt one, MyBigInt two)
        {
            if (object.Equals(one, null)) return two;
            if (object.Equals(two, null)) return one;

            if (one.Max || two.Max) return new MyBigInt() { Max = true };
            if (one.IsZero()) return two;
            if (two.IsZero()) return one;

            var ret = new MyBigInt();
            ret.Sign = one.Sign | two.Sign;
            int i = 0;

            for (; i < one.Data.Count; i++)
            {
                if (two.Data.Count > i)
                {
                    ret.Data.Add(one.Data[i] | two.Data[i]);
                }
                else ret.Data.Add(one.Data[i]);
            }

            for (int j = i; j < two.Data.Count; j++)
            {
                ret.Data.Add(two.Data[j]);
            }

            ret.Tidy();
            return ret;
        }

        //public static MyBigInt operator +(MyBigInt one, MyBigInt two)
        //{
        //    MyBigInt ret = new MyBigInt();

        //    return ret;
        //}

        //public static MyBigInt operator -(MyBigInt one, MyBigInt two)
        //{
        //    MyBigInt ret = new MyBigInt();

        //    return ret;
        //}

        //public static MyBigInt operator *(MyBigInt one, MyBigInt two)
        //{
        //    MyBigInt ret = new MyBigInt();

        //    return ret;
        //}

        //public static MyBigInt operator /(MyBigInt one, MyBigInt two)
        //{
        //    MyBigInt ret = new MyBigInt();

        //    return ret;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool operator ==(MyBigInt value, MyBigInt other)
        {
            if (object.Equals(value, null) && object.Equals(other, null)) return true;
            else if (object.Equals(value, null)) return false;
            else if (object.Equals(other, null)) return false;

            if (value.Max != other.Max) return false;
            if (value.Sign != other.Sign) return false;

            value.Tidy();
            other.Tidy();
            if (value.Data.Count != other.Data.Count) return false;
            for (int i = 0; i < value.Data.Count; i++)
            {
                if (value.Data[i] != other.Data[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool operator !=(MyBigInt value, MyBigInt other)
        {
            return !(value == other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MyBigInt Tidy()
        {
            while (true)
            {
                if (Data.Count == 0) break;
                if (Data[Data.Count - 1] != 0) break;
                Data.RemoveAt(Data.Count - 1);
            }
            return this;
        }




        /// <summary>
        /// 转换为各个 2 的次幂，从低位到高位。
        /// </summary>
        /// <returns></returns>
        public List<int> ToPositions()
        {
            var list = new List<int>();
            if (Max || IsZero())
            {
                return list;
            }

            for (int i = 0; i < this.Data.Count; i++)
            {
                if (this.Data[i] == 0) continue;
                for (int j = 0; j < 32; j++)
                {
                    var p = Convert.ToUInt32(Math.Pow(2, j));
                    if (p > this.Data[i]) break;

                    if ((this.Data[i] & p) != 0)
                    {
                        list.Add(i * 32 + j + 1);
                    }
                }
            }
            return list;
        }

        //[Obsolete("Use ToPositions")]
        //public List<int> ToPowerPositions()
        //{
        //    return ToPositions();
        //}
        //public override string ToString()
        //{
        //    return ToString("X");
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Max) return "Max";

            string[] ary = null;
            if (this.Data.Count == 0)
            {
                ary = new string[] { "0" };
            }
            else
            {
                ary = Tidy().Data.ToArray().Reverse().Select(o => o.ToString("X")).ToArray();
            }
            return (this.Sign ? "" : "-") + string.Join(Separator.ToString(), ary);
        }

        public MyBigInt Clone()
        {
            var bigInt = new MyBigInt();
            if (this.Max)
            {
                bigInt.Max = true;
                return bigInt;
            }
            else
            {
                bigInt.Sign = this.Sign;
                bigInt.Data.AddRange(this.Data);
            }

            return bigInt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNull(MyBigInt value)
        {
            return object.Equals(value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            this.Tidy();
            return this.Max == false && this.Data.Count == 0;
        }

        /// <summary>
        /// 按位来一次性填充域。
        /// </summary>
        /// <param name="positionLengths"></param>
        /// <returns></returns>
        public static MyBigInt Fill1(uint positionLengths)
        {
            List<int> list = new List<int>();
            for (int i = 1; i <= positionLengths; i++)
            {
                list.Add(i);
            }
            return MyBigInt.CreateBySqlRowIds(list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete("")]
        public uint GetPositionLength()
        {
            if (Max) return 0;
            if (IsZero()) return 0;
            var len = this.Data.Count;
            if (len == 1) return this.ToPositions().Max().AsUInt();
            else
            {
                var maxOne = new MyBigInt();
                maxOne.Data.Add(this.Data.Last());
                return ((len - 1) * 32 + maxOne.ToPositions().Max()).AsUInt();
            }
        }

        //public static MyBigInt operator ^(MyBigInt one, MyBigInt two)
        //{
        //    if (one.Max || two.Max) return new MyBigInt() { Max = true };
        //    if (one.IsZero() && two.IsZero()) return one;

        //    MyBigInt ret = new MyBigInt();
        //    ret.Sign = one.Sign ^ two.Sign;
        //    int i = 0;
        //    for (; i < one.data.Count; i++)
        //    {
        //        if (two.data.Count > i)
        //        {
        //            ret.data.Add(one.data[i] ^ two.data[i]);
        //        }
        //        else ret.data.Add(0 ^ one.data[i]);
        //    }
        //    for (int j = i; j < two.data.Count; j++)
        //    {
        //        ret.data.Add(0 ^ two.data[i]);
        //    }
        //    ret.Tidy();
        //    return ret;
        //}

        /// <summary>
        /// 设置指定位置的符号 , 如果为true , 则设置为 1 , 否则设置为 0
        /// </summary>
        /// <param name="position">从1开始. 单个最大32.</param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public MyBigInt SetFlag(int position, bool sign)
        {
            if (this.Max) return this;
            if (position <= 0) return this;

            int yu = (position - 1) % 32;
            int len = Math.Ceiling((double)position / 32.0).AsInt() - 1;

            Console.WriteLine(0 - yu - 1);

            for (int i = this.Data.Count; i <= len; i++)
            {
                this.Data.Add(0);
            }

            if (sign)
            {
                this.Data[len] = this.Data[len] | (uint)Math.Pow(2, yu);
            }
            else
            {
                //整数求负＝整数求补＝按位求反＋１　
                //所以：按位求反＝整数求负－１。
                this.Data[len] = (uint)(this.Data[len] & (0 - (uint)Math.Pow(2, yu) - 1));
            }

            this.Tidy();
            return this;
        }

        /// <summary>
        /// 从当前数中 减去 NotContains 中的各个位. 如果当前数不存在 NotContains 的位, 则忽略 .
        /// </summary>
        /// <param name="notContains"></param>
        /// <returns></returns>
        public MyBigInt Minus(MyBigInt notContains)
        {
            notContains.ToPositions().All(o =>
                {
                    this.SetFlag(o, false);
                    return true;
                });
            return this;
        }

        public static implicit operator MyBigInt(DBNull value)
        {
            return new MyBigInt();
        }
        public static implicit operator MyBigInt(string hexValue)
        {
            return new MyBigInt(hexValue.AsString());
        }
        public static implicit operator string(MyBigInt value)
        {
            return value.ToString();
        }


        public override int GetHashCode()
        {
            int ret = 0;
            if (this.Data == null) return ret;
            this.Data.ForEach(o => ret = ((ret + o) % int.MaxValue).AsInt());
            if (Sign == false) ret = 0 - ret;
            return ret;
        }

        public override bool Equals(object obj)
        {
            if (obj.IsDBNull()) return false;
            MyBigInt other = null;

            Func<bool> GetNumber = () =>
                {

                    other = obj as MyBigInt;
                    if (other != null) return true;

                    {
                        var val = obj as string;
                        if (val != null) { other = new MyBigInt(val); return true; }
                    }

                    {
                        var ival = obj as int?;
                        if (ival != null) { other = new MyBigInt(ival.Value); return true; }
                    }
                    {
                        var uval = obj as uint?;
                        if (uval != null) { other = new MyBigInt(uval.Value); return true; }
                    }
                    return false;
                };

            if (GetNumber() && other != null)
            {
                if (this.IsZero() && other.IsZero()) return true;
                if (this.Max && other.Max) return true;

                if (this.Sign != other.Sign) return false;
                if (other.Data == null) return false;

                if (this.Data.Count != other.Data.Count) return false;

                for (int i = 0; i < this.Data.Count; i++)
                {
                    if (this.Data[i] != other.Data[i]) return false;
                }

                return true;
            }

            return false;
        }

        object ICloneable.Clone()
        {
            return new MyBigInt(this.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerPositions"></param>
        /// <returns></returns>
        public static MyBigInt CreateBySqlRowIds(IEnumerable<int> sqlRpwIds)
        {
            var c = new MyBigInt();
            if (sqlRpwIds == null) return c;

            sqlRpwIds.All(o => { c = (c | MyBigInt.CreateBySqlRowId(Convert.ToUInt32(o))); return true; });

            return c;
        }
    }


    //public partial class MyBigInt
    //{
    //    [Obsolete("请使用：CreateBySqlRowId")]
    //    public static MyBigInt CreateByBitPositon(uint SqlRowId)
    //    {
    //        return CreateBySqlRowId(SqlRowId);
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="powerPositions"></param>
    //    /// <returns></returns>
    //    [Obsolete("请使用：CreateBySqlRowIds")]
    //    public static MyBigInt CreateByBitPositons(IEnumerable<int> powerPositions)
    //    {
    //        return CreateBySqlRowIds(powerPositions);
    //    }
    //}
}
