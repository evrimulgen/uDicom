

using System;
using log4net.Config;
using uDicom.Common;

// This will cause log4net to look for a configuration file
// called Logging.config in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.

[assembly: XmlConfigurator(ConfigFile = "Logging.config", Watch = true)]

namespace uDicom.Log.Log4net
{
    public static class Log4NetLogManager
    {
        public static Log4NetLogger GetLogger(string name)
        {
            return new Log4NetLogger(log4net.LogManager.GetLogger(name));
        }
    }
    
    public class Log4NetLogger : ILog
    {
        private readonly log4net.ILog _logger;

        public Log4NetLogger(log4net.ILog logger)
        {
            this._logger = logger;
        }

        public void Debug(string format, params object[] args)
        {
            _logger.DebugFormat(format, args);
        }

        public void Info(string format, params object[] args)
        {
            this._logger.InfoFormat(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            this._logger.WarnFormat(format, args);
        }

        public void Error(string format, params object[] args)
        {
            this._logger.ErrorFormat(format, args);
        }

        public void Error(Exception e, string format, params object[] args)
        {
            _logger.Error(string.Format(format, args), e);
        }

        public void Fatal(string format, params object[] args)
        {
            _logger.FatalFormat(format, args);
        }

        public void Fatal(Exception e, string format, params object[] args)
        {
            _logger.Fatal(string.Format(format, args), e);
        }

        
    }
}
