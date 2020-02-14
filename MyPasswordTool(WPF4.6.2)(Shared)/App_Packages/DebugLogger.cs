using SilverEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Common
{
    public class DebugLogger : ILogger
    {
        public string Prev { get; private set; } 
        public string Content { get; private set; } 

        public void Info(string format, params object[] args)
        {
            Prev = string.Format("info::{0:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now);
            Content = (args == null || args.Length <= 0) ? format : string.Format(format, args);
            Debug.WriteLine(string.Format("{0}:{1}", Prev, Content));
        }

        public void Warn(string format, params object[] args)
        {
            Prev = string.Format("warn::{0:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now);
            Content = (args == null || args.Length <= 0) ? format : string.Format(format, args);
            Debug.WriteLine(string.Format("{0}:{1}", Prev, Content));
        }

        public void Error(string message, Exception exception)
        {
            Prev = string.Format("error::{0:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now);
            Content = string.Format("{0}--错误{1};内部错误{2}", message, exception.Message, exception.InnerException.Message);
            Debug.WriteLine(string.Format("{0}:{1}", Prev, Content));
        }

        public void Error(Exception exception)
        {
            Error("", exception);
        }
    }
}
