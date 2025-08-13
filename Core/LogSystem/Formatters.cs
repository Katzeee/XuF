using System;
using System.Globalization;

namespace Xuf.Core
{
    /// <summary>
    /// Simple formatter with template support.
    /// Tokens: {timestamp}, {level}, {logger}, {message}, {file}, {line}, {member}
    /// Timestamp format: yyyy-MM-dd HH:mm:ss.fff
    /// </summary>
    public sealed class TemplateLogFormatter : ILogFormatter
    {
        private readonly string _template;

        public TemplateLogFormatter(string template = "{timestamp} [{level}] [{logger}] {message} ({file}:{line})")
        {
            _template = template ?? "{timestamp} [{level}] [{logger}] {message} ({file}:{line})";
        }

        public string Format(LogEvent logEvent)
        {
            string timestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            string level = logEvent.Level.ToString().ToUpperInvariant();
            string message = logEvent.Message ?? string.Empty;
            string file = logEvent.SourceFilePath ?? string.Empty;
            string line = logEvent.SourceLineNumber > 0 ? logEvent.SourceLineNumber.ToString(CultureInfo.InvariantCulture) : string.Empty;
            string member = logEvent.MemberName ?? string.Empty;
            string logger = logEvent.LoggerName ?? string.Empty;

            string result = _template
                .Replace("{timestamp}", timestamp)
                .Replace("{level}", level)
                .Replace("{logger}", logger)
                .Replace("{message}", message)
                .Replace("{file}", file)
                .Replace("{line}", line)
                .Replace("{member}", member);

            return result;
        }
    }
}


