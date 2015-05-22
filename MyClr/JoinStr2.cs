using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlTypes;
using System.Data.Sql;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

//ms-help://MS.SQLCC.v10/MS.SQLSVR.v10.zh-CHS/s10de_2devguide/html/5a188b50-7170-4069-acad-5de5c915f65d.htm


[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedAggregate(
    Microsoft.SqlServer.Server.Format.UserDefined, //use clr serialization to serialize the intermediate result
    IsInvariantToNulls = true,//optimizer property
    IsInvariantToDuplicates = false,//optimizer property
    IsInvariantToOrder = false,//optimizer property
    MaxByteSize = -1)//maximum size in bytes of persisted value
    ]
public class JoinStr2 : Microsoft.SqlServer.Server.IBinarySerialize
{
    /// <summary>
    /// The variable that holds the intermediate result of the concatenation
    /// </summary>
    private List<string> intermediateResult;
    private string joinString;
    /// <summary>
    /// Initialize the internal data structures
    /// </summary>
    public void Init()
    {
        intermediateResult = new List<string>();
    }

    /// <summary>
    /// Accumulate the next value, nop if the value is null
    /// </summary>
    /// <param name="value"></param>
    public virtual void Accumulate(SqlString value, SqlString joinString)
    {
        this.joinString = joinString.IsNull ? "," : joinString.Value;
        
        if (value.IsNull) return;
        if (value.Value == null) return;
        if (value.Value.Length == 0) return;

        intermediateResult.Add(value.Value);
    }

    /// <summary>
    /// Merge the partially computed aggregate with this aggregate.
    /// </summary>
    /// <param name="other"></param>
    public void Merge(JoinStr2 other)
    {
        intermediateResult.AddRange(other.intermediateResult);
    }

    /// <summary>
    /// Called at the end of aggregation, to return the results of the aggregation
    /// </summary>
    /// <returns></returns>
    [return: SqlFacet(MaxSize = -1)]
    public SqlString Terminate()
    {
        string output = string.Empty;
        //delete the trailing comma, if any
        if (intermediateResult != null && intermediateResult.Count > 0)
        {
            intermediateResult.Sort();


            output = string.Join(this.joinString, intermediateResult.ToArray());// intermediateResult.ToString(0, intermediateResult.cou - (this.joinString == null ? 1 : this.joinString.Length));
        }
        return new SqlString(output);
    }

    public void Read(BinaryReader r)
    {
        if (r == null) throw new ArgumentNullException("r");
        
        var value = r.ReadString();
        if (value == null) return;
        if (value.Length == 0) return;

        intermediateResult = new List<string>();
        intermediateResult.Add(value);
    }

    public void Write(BinaryWriter w)
    {
        if (w == null) throw new ArgumentNullException("w");
        intermediateResult.Sort();

        w.Write(string.Join(this.joinString, intermediateResult.ToArray()));
    }
}
