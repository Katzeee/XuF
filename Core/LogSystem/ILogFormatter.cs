namespace Xuf.Core
{
    /// <summary>
    /// Formats a LogEvent into a string. Implement to customize log output.
    /// </summary>
    public interface ILogFormatter
    {
        string Format(LogEvent logEvent);
    }
}


