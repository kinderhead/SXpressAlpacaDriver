using System;
using ASCOM.Tools;

namespace SXpressAlpacaDriver
{
    public class DualLogger(string logType) : ASCOM.Common.Interfaces.ILogger
    {
        private readonly TraceLogger fileLogger = new(logType, true);
        private readonly ConsoleLogger consoleLogger = new();

        public ASCOM.Common.Interfaces.LogLevel LoggingLevel => consoleLogger.LoggingLevel;

        public void Log(ASCOM.Common.Interfaces.LogLevel level, string message)
        {
            consoleLogger.Log(level, message);
            fileLogger.Log(level, message);
        }

        public void SetMinimumLoggingLevel(ASCOM.Common.Interfaces.LogLevel level)
        {
            consoleLogger.SetMinimumLoggingLevel(level);
            fileLogger.SetMinimumLoggingLevel(level);
        }
    }
}
