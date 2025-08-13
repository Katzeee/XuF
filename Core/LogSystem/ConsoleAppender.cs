using UnityEngine;

namespace Xuf.Core
{
    /// <summary>
    /// Console appender that uses UnityEngine.Debug.
    /// </summary>
    internal sealed class ConsoleAppender : ILogAppender
    {
        private ILogFormatter _formatter;

        public ConsoleAppender(ILogFormatter formatter = null)
        {
            _formatter = formatter ?? new TemplateLogFormatter();
        }

        public void SetFormatter(ILogFormatter formatter)
        {
            _formatter = formatter ?? new TemplateLogFormatter();
        }

        public void Append(in LogEvent logEvent)
        {
            string formatted = _formatter.Format(logEvent);
            switch (logEvent.Level)
            {
                case LogLevel.Warning:
                    if (logEvent.Context != null) Debug.LogWarning(formatted, logEvent.Context); else Debug.LogWarning(formatted);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    if (logEvent.Context != null) Debug.LogError(formatted, logEvent.Context); else Debug.LogError(formatted);
                    break;
                default:
                    if (logEvent.Context != null) Debug.Log(formatted, logEvent.Context); else Debug.Log(formatted);
                    break;
            }
        }

        public void Flush() { /* No-op for console */ }
        public void Dispose() { }
    }
}




