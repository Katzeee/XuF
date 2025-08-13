using System;
using UnityEngine;

namespace Xuf.Core
{
    /// <summary>
    /// Log event data passed to appenders.
    /// </summary>
    public readonly struct LogEvent
    {
        public readonly DateTime Timestamp;
        public readonly LogLevel Level;
        public readonly string Message;
        public readonly UnityEngine.Object Context;
        public readonly string SourceFilePath;
        public readonly int SourceLineNumber;
        public readonly string MemberName;
        public readonly string LoggerName;

        public LogEvent(DateTime timestamp, LogLevel level, string message, UnityEngine.Object context, string sourceFilePath, int sourceLineNumber, string memberName, string loggerName)
        {
            Timestamp = timestamp;
            Level = level;
            Message = message;
            Context = context;
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;
            MemberName = memberName;
            LoggerName = loggerName;
        }
    }
}


