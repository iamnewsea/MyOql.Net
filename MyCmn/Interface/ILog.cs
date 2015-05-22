using System;
using MyCmn;


namespace MyCmn
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        bool To(string LogType, string Msg, string Detail, string Exception, decimal Value);
    }
}
