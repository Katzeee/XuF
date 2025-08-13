using System;

namespace Xuf.Core
{
    /// <summary>
    /// Appender contract. Implementations route log events to destinations.
    /// </summary>
    public interface ILogAppender : IDisposable
    {
        void Append(in LogEvent logEvent);
        void Flush();
    }
}


