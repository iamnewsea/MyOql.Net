using System;
using System.Web.Mvc;
using System.Configuration;
using System.Threading;


namespace MyCmn
{
    /// <summary>
    ///  不可能出现的错误，灾难性错误，根本性错误。也可能是系统移植过程中出现的不可预料的错误。
    /// </summary>
    /// <remarks>
    /// 需要两个配置项:
    /// 1. GodErrorLog=bool
    /// 2. GodErrorRes=bool
    /// </remarks>
    public class GodError : Exception
    {
        //public static bool GodErrorRes;
        //public static bool GodErrorLog;
        static GodError()
        {
            //GodErrorRes = ConfigurationManager.AppSettings["GodErrorRes"].AsBool();
            //GodErrorLog = ConfigurationManager.AppSettings["GodErrorLog"].AsBool();
        }

        public GodError()
            : this("God Error!")
        {
        }

        public GodError(Exception exp)
            : this(exp.Message, exp)
        {
            HelpLink = exp.HelpLink;
            Source = exp.Source;
        }

        public GodError(string Msg, Exception exp)
            : base(Msg, exp)
        {
            HelpLink = exp.HelpLink;
            Source = exp.Source;


            //if (GodErrorScopeWrapper.CurrentValue.Contains(GodErrorConfigEnum.SkipLog) == false)
            //{
            //    if (GodErrorLog)
            //    {
            //        Log.To("GodError", this.Message, this.Detail, this.StackTrace, 0M);
            //    }
            //}
        }

        //public static string GetRes(string msg)
        //{
        //    return GodErrorRes ? msg.GetRes().AsString() : msg;
        //}


        public GodError(string msg)
            : base(msg)
        {
            //if (GodErrorScopeWrapper.CurrentValue.Contains(GodErrorConfigEnum.SkipLog) == false)
            //{
            //    if (GodErrorLog)
            //    {
            //        Log.To("GodError", this.Message, this.Detail, this.StackTrace, 0M);
            //    }
            //}
        }

        public GodError(string msg, StringDict parameterFormat)
            : this(string.Format(msg, parameterFormat))
        {
        }

        public GodError(string msg, StringDict parameterFormat, string detail)
            : this(string.Format(msg, parameterFormat))
        {
            this.Detail = detail;
        }


        //----------------------------------------------------------------------------------------------//

        //public GodError(StringLinker msg)
        //    : base(msg)
        //{
        //}

        //public GodError(StringLinker msg, StringDict parameterFormat)
        //    : base(msg.Format(parameterFormat))
        //{
        //}

        //public GodError(StringLinker msg, StringDict parameterFormat, string culprit)
        //    : base(msg.Format(parameterFormat))
        //{
        //    this.Culprit = culprit;
        //}

        public string Type { get; set; }

        /// <summary>
        /// 犯人,事故发生的主体.
        /// </summary>
        //public string Culprit { get; set; }

        /// <summary>
        /// 消息详情
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCondition"></param>
        /// <param name="msgFunc"></param>
        /// <exception cref="GodError"></exception>
        public static void Check(bool errorCondition, string msg)
        {
            if (errorCondition && (msg != null))
            {
                throw new GodError(msg) { Type = "GodError" };
            }
        }

        public static void Check(bool errorCondition, string msg, string detail)
        {
            if (errorCondition && (msg != null))
            {
                throw new GodError(msg) { Type = "GodError", Detail = detail };
            }
        }

        public static void Check(bool errorCondition, Func<string> msgFunc)
        {
            if (errorCondition && (msgFunc != null))
            {
                throw new GodError(msgFunc()) { Type = "GodError" };
            }
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="errorCondition"></param>
        ///// <param name="detail"></param>
        ///// <param name="msgFunc"></param>
        ///// <exception cref="GodError"></exception>
        //public static void Check(bool errorCondition, Type detail, Func<string> msgFunc)
        //{
        //    if (errorCondition && (msgFunc != null))
        //    {
        //        throw new GodError(msgFunc()) { Detail = detail == null ? "" : detail.FullName, Type = "GodError" };
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCondition"></param>
        /// <param name="type"></param>
        /// <param name="detail"></param>
        /// <param name="msgFunc"></param>
        /// <exception cref="GodError"></exception>
        public static void Check(bool errorCondition, string type, Func<string> msgFunc, string detail)
        {
            if (errorCondition && (msgFunc != null))
            {
                throw new GodError(msgFunc())
                          {
                              Type = type,
                              Detail = detail
                          };
            }
        }


        public static void Check(bool errorCondition, string type, string msg, string detail)
        {
            if (errorCondition && (msg != null))
            {
                throw new GodError(msg)
                          {
                              Type = type,
                              Detail = detail
                          };
            }
        }
    }
}