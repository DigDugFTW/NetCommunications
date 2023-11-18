using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Logging
{
    public delegate void OnDebugLog(string logMessage, bool newLineBefore, bool newLineAfter, string sender);
    public delegate void OnMessageLog(string message);
    public delegate void OnExceptionLog(Exception exception, string sender);
    public static class Logger
    {
        public static event OnMessageLog OnMessageLogCalled;
        public static event OnExceptionLog OnExceptionLogCalled;
        public static event OnDebugLog OnDebugLogCalled;

        public static void LogMessage(string message)
        {
            OnMessageLogCalled?.Invoke(message);
        }

        public static void LogException(Exception exception, [CallerMemberName] string sender = null)
        {
            OnExceptionLogCalled?.Invoke(exception, sender);
        }

        public static void LogDebug(string logMessage, bool newLineBefore = false, bool newLineAfter = false, [CallerMemberName] string sender = null)
        {
            OnDebugLogCalled?.Invoke(logMessage, newLineBefore, newLineAfter, $"[{DateTime.Now}]{sender}");
        }
    }
}
