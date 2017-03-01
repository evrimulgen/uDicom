using System;

namespace uDicom.Common
{
    public interface ILog
    {
        void Debug(string format, params object[] args);

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Error(string format, params object[] args);

        void Error(Exception e, string format, params object[] args);

        void Fatal(string format, params object[] args);

        void Fatal(Exception e, string format, params object[] args);
    }
}
