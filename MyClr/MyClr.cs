using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using MyCmn;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public partial class MyClr
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlDouble GetClrSimilar(SqlString Value1, SqlString Value2)
    {
        if (Value1.IsNull) return 0.0;
        if (Value2.IsNull) return 0.0;

        // 在此处放置代码
        return MyCmn.MyHelper.GetSimilar(Value1.Value, Value2.Value, false, 64);
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBoolean BitContains(SqlString MyBigIntValue, SqlInt32 Row)
    {
        if (MyBigIntValue.IsNull) return false;
        // 在此处放置代码
        return (new MyBigInt(MyBigIntValue.Value) & MyBigInt.CreateBySqlRowId(Convert.ToUInt32(Row.Value))) != MyBigInt.Zero;
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString BitAnd(SqlString MyBigIntValue, SqlString BigIntOther)
    {
        if (MyBigIntValue.IsNull || BigIntOther.IsNull) return new SqlString();
        // 在此处放置代码
        return (new MyBigInt(MyBigIntValue.Value) & new MyBigInt(BigIntOther.Value)).ToString();
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString BitOr(SqlString MyBigIntValue, SqlString BigIntOther)
    {
        if (MyBigIntValue.IsNull || BigIntOther.IsNull) return new SqlString();
        // 在此处放置代码
        return (new MyBigInt(MyBigIntValue.Value) | new MyBigInt(BigIntOther.Value)).ToString();
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString NewBigInt(SqlInt64 RowId)
    {
        if (RowId.IsNull) return new MyBigInt().ToString();
        // 在此处放置代码
        return MyBigInt.CreateBySqlRowId((uint)RowId.Value).ToString();
    }

    //这个特性定义了一个sql表值函数，此函数返回的表的定义为：String nvarchar(200)
    //并且指定了填充这个表的行的方法是FillRow 方法
    //注意这个方法返回的一定是一个IEnumerable类型的，并且为公开，静态，这个方法的入参就是sql函数的入参
    [SqlFunction(TableDefinition = "Value int", FillRowMethodName = "FillRowWithInt")]
    public static IEnumerable BigIntToIds(SqlString MyBigIntValue)
    {
        if (MyBigIntValue.IsNull) return new int[] { };
        //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
        return new MyBigInt(MyBigIntValue.Value).ToPositions();
    }


    [SqlFunction(TableDefinition = "Value nvarchar(max)", FillRowMethodName = "FillRowWithString")]
    public static SqlString GetSplitItem(SqlString Value, SqlString split, int SqlIndex)
    {
        if (Value.IsNull || split.IsNull) return string.Empty;

        int index = 0;
        if (SqlIndex == 0)
        {
            return null;
        }
        else if (SqlIndex > 0)
        {
            index = SqlIndex - 1;
        }
        else
        {
            index = SqlIndex;
        }

        //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
        var ret = Value.Value.Split(split.Value.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (index >= ret.Length) return null;

        if (index < 0)
        {
            index = ret.Length + index;

            if (index < 0) return null;

            return ret[index];
        }

        return ret[index];
    }

    [Serializable]
    public class SplitModel
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    [SqlFunction(TableDefinition = "Id int,Value nvarchar(max)", FillRowMethodName = "FillRow_SplitModel")]
    public static IEnumerable Split(SqlString Value, SqlString split)
    {
        var list = new List<SplitModel>();

        if (Value.IsNull || split.IsNull) return list;
        //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
        var ary = Value.Value.Split(split.Value.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < ary.Length; i++)
        {
            var row = new SplitModel();
            row.Id = (i + 1);
            row.Value = ary[i];

            list.Add(row);
        }

        return list;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="Key">支持 a.b 多级获取，也可以a[0].b ，[0].a.b 取数组的值进行获取</param>
    /// <returns></returns>
    [SqlFunction]
    public static SqlString GetJsonValue(SqlString Value, SqlString Key)
    {
        if (Value.IsNull || Key.IsNull) return new SqlString();
        //返回一个string 数组，这个数组符合IEnumerable接口，当然你也可以返回hashtable等类型。
        var strJson = Value.Value.Trim();
        if (!((strJson.StartsWith("{") && strJson.EndsWith("}")) ||
            (strJson.StartsWith("[") && strJson.EndsWith("]"))
            )) throw new Exception("非法Json");


        return PowerJson.GetJsonValue(Value.Value, Key.Value);
    }



    //填充返回表的行的方法，这个方法有一定的规定：
    //一定是空返回的void类型，并且入参的第一个必须为object，其后面的参数都必须为out类型
    //参数的类型，个数和顺序由返回表的列结构决定！（在TableDefinition = " String nvarchar(200)"中定义的表结构）
    public static void FillRowWithInt(object row, out int Value)
    {
        //这个object 其实就是GetStrings(string x,char y)函数返回的迭代，这样你直接赋值给那个列就可以了。
        Value = (int)row;
    }

    //填充返回表的行的方法，这个方法有一定的规定：
    //一定是空返回的void类型，并且入参的第一个必须为object，其后面的参数都必须为out类型
    //参数的类型，个数和顺序由返回表的列结构决定！（在TableDefinition = " String nvarchar(200)"中定义的表结构）
    public static void FillRowWithString(object row, out string Value)
    {
        //这个object 其实就是GetStrings(string x,char y)函数返回的迭代，这样你直接赋值给那个列就可以了。
        Value = row.ToString();
    }
    //填充返回表的行的方法，这个方法有一定的规定：
    //一定是空返回的void类型，并且入参的第一个必须为object，其后面的参数都必须为out类型
    //参数的类型，个数和顺序由返回表的列结构决定！（在TableDefinition = " String nvarchar(200)"中定义的表结构）
    public static void FillRow_SplitModel(object row, out int Id, out string Value)
    {
        //这个object 其实就是GetStrings(string x,char y)函数返回的迭代，这样你直接赋值给那个列就可以了。
        var rowData = row as SplitModel;

        Id = rowData.Id;
        Value = rowData.Value;
    }
}

