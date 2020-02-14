using System;

namespace MyPasswordTool.Service
{
    internal class log4netLogger : SilverEx.ILogger
    {
        public log4netLogger(Type type)
        {
            l4 = log4net.LogManager.GetLogger(type);
        }

        private readonly log4net.ILog l4;

        void SilverEx.ILogger.Info(string format, params object[] args)
        {
            if (args?.Length > 0) l4.InfoFormat(format, args);
            else l4.Info(format);
        }

        void SilverEx.ILogger.Warn(string format, params object[] args)
        {
            if (args?.Length > 0) l4.WarnFormat(format, args);
            else l4.Warn(format);
        }

        void SilverEx.ILogger.Error(string message, Exception exception)
        {
            l4.Error(message, exception);
        }

        void SilverEx.ILogger.Error(Exception exception)
        {
            l4.Error(exception);
        }
    }
}