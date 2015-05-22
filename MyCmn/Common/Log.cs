using System;
using System.Text;
using System.IO;
using System.Configuration;

namespace MyCmn
{
    public class LogEntity
    {
        public string Type { get; set; }
        public string Msg { get; set; }
        public string Detail { get; set; }
        public string Exception { get; set; }
        public double NumberValue { get; set; }
    }
    /// <summary>
    /// 日志类。
    /// </summary>
    /// <example>
    /// <code>
    /// Log&lt;InfoEnum&gt;.To(InfoEnum.Error,"Msg","User"); 
    /// </code>
    /// </example>
    public class Log
    {
        private static object Sync = new object();
        private static ILog _Log = null;
        private static bool IsFind = false;
        private static ILog GetLogEvent()
        {
            if (_Log == null && !IsFind)
            {
                lock (Sync)
                {
                    if (_Log == null)
                    {
                        var logEvent = ConfigurationManager.AppSettings["LogEvent"];
                        IsFind = true;
                        if (logEvent.HasValue())
                        {
                            var type = Type.GetType(logEvent);
                            if (type != null)
                            {
                                _Log = Activator.CreateInstance(type) as ILog;
                                return _Log;
                            }
                        }
                    }
                }
            }
            return _Log;
        }

        public static void To(string LogType, string Msg)
        {
            To(LogType, Msg, string.Empty, string.Empty, 0M);
        }

        public static void To<T>(T LogType, string Msg)
        {
            To(LogType.ToString(), Msg, string.Empty, string.Empty, 0M);
        }

        public static void To<T>(T LogType, string Msg, string Detail)
        {
            To(LogType.ToString(), Msg, Detail, string.Empty, 0M);
        }

        public static void To(string LogType, string Msg, string Detail, string Exception, decimal Value)
        {
            var log = GetLogEvent();
            if (log == null) throw new Exception("LogEvent配置项不正确，请检查！");
            log.To(LogType, Msg, Detail, Exception, Value);
        }
    }
}
