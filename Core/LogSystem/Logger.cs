using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Xuf.Core
{
    /// <summary>
    /// Represents a named logger with its own level and appenders.
    /// By default, a logger uses system-level appenders. If any custom appender
    /// is added, the logger will switch to custom-only appenders.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        private readonly List<ILogAppender> _appenders = new List<ILogAppender>(2);

        public string Name { get; }
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
        // Each logger owns its own appenders; no sharing

        internal Logger(string name, LogLevel? level = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Default" : name;
            if (level.HasValue)
            {
                MinimumLevel = level.Value;
            }
        }

        public void AddAppender(ILogAppender appender)
        {
            if (appender == null) throw new ArgumentNullException(nameof(appender));
            if (!_appenders.Contains(appender))
            {
                _appenders.Add(appender);
            }
        }

        public void RemoveAppender(ILogAppender appender)
        {
            if (appender == null) return;
            _appenders.Remove(appender);
        }

        public void ClearAppenders()
        {
            for (int i = 0; i < _appenders.Count; i++)
            {
                try
                {
                    _appenders[i].Flush();
                    _appenders[i].Dispose();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[Logger] Error disposing appender: {e}");
                }
            }
            _appenders.Clear();
        }

        public void Log(
            LogLevel level,
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null)
        {
            if (level < MinimumLevel) return;

            var evt = new LogEvent(DateTime.Now, level, message ?? string.Empty, context, sourceFilePath, sourceLineNumber, memberName, Name);

            var appenders = ResolveAppenders();
            for (int i = 0; i < appenders.Count; i++)
            {
                try
                {
                    appenders[i].Append(evt);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[Logger] Appender error: {e}");
                }
            }
        }

        public void Trace(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null) => Log(LogLevel.Trace, message, context, sourceFilePath, sourceLineNumber, memberName);

        public void Debug(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null) => Log(LogLevel.Debug, message, context, sourceFilePath, sourceLineNumber, memberName);

        public void Info(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null) => Log(LogLevel.Info, message, context, sourceFilePath, sourceLineNumber, memberName);

        public void Warning(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null) => Log(LogLevel.Warning, message, context, sourceFilePath, sourceLineNumber, memberName);

        public void Error(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null) => Log(LogLevel.Error, message, context, sourceFilePath, sourceLineNumber, memberName);

        public void Fatal(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = null) => Log(LogLevel.Fatal, message, context, sourceFilePath, sourceLineNumber, memberName);

        public void Dispose()
        {
            for (int i = 0; i < _appenders.Count; i++)
            {
                try
                {
            _appenders[i].Flush();
            _appenders[i].Dispose();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[Logger] Error disposing appender: {e}");
                }
            }
            _appenders.Clear();
        }

        internal IReadOnlyList<ILogAppender> ResolveAppenders() => _appenders;

        internal T FindAppender<T>() where T : class, ILogAppender
        {
            for (int i = 0; i < _appenders.Count; i++)
            {
                if (_appenders[i] is T t) return t;
            }
            return null;
        }
    }
}


