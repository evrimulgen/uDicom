using System;

namespace uDicom.Common
{
    public static class LogManager
    {
        static readonly ILog NullLogInstance = new NullLogger();

        public static Func<string, ILog> GetLog = type => NullLogInstance;

        private class NullLogger : ILog
        {
            public void Debug(string format, params object[] args)
            {
            }

            public void Info(string format, params object[] args)
            {
            }

            public void Warn(string format, params object[] args)
            {
            }

            public void Error(string format, params object[] args)
            {
            }

            public void Error(Exception e, string format, params object[] args)
            {
            }

            public void Fatal(string format, params object[] args)
            {
            }

            public void Fatal(Exception e, string format, params object[] args)
            {
            }

            
        }
    }
}
