using System;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace MyCmn
{
    /// <summary>
    /// 返回简单的Json消息对象.
    /// </summary>
    [Serializable]
    [DataContract]
    public class JsonMsg : ActionResult
    {
        public JsonMsg()
        {
            this.msg = string.Empty;
        }

        public JsonMsg(string Msg)
        {
            this.msg = Msg;
        }

        /// <summary>
        /// 记录错误消息 。 没有消息,就表示没有错误.(No New Is Good News)
        /// </summary>
        [DataMember]
        public string msg { get; set; }


        public bool HasMsg()
        {
            return this.msg.HasValue();
        }
        //private Func<GodError> _Func;
        //private IAsyncResult result;

        ///// <summary>
        ///// 记录错误的堆栈信息。
        ///// </summary>
        //private void RecordError()
        //{
        //    _Func = new Func<GodError>(() =>
        //    {
        //        try
        //        {
        //            throw new GodError(this.msg);
        //        }
        //        catch (GodError exp)
        //        {
        //            return exp;
        //        }
        //    });

        //    _Func.BeginInvoke(null, null);
        //}

        //public GodError GetError()
        //{
        //    if (result == null) return null;
        //    return _Func.EndInvoke(result);
        //}


        /// <summary>
        /// 外带数据。
        /// </summary>
        [DataMember]
        public object data { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/json";
            if (this.data.IsDBNull() == false && this.data.GetType().IsNumberType())
            {
                //修复Js精度。
                if (this.data.AsDecimal() > int.MaxValue)
                {
                    this.data = this.data.AsString();
                }
            }

            response.Write(this.ToString());

            //response.End();
        }
    }

    /// <summary>
    /// 返回简单的Json消息对象.
    /// </summary>
    [Serializable]
    [DataContract]
    public class JsonMsg<T> : JsonMsg
    {
        public JsonMsg()
            : base()
        {
        }

        public JsonMsg(string Msg)
            : base(Msg)
        {

        }

        /// <summary>
        /// 用于中间传值。
        /// </summary>
        [DataMember]
        public T value { get; set; }
    }
}
