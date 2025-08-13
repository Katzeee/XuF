using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Core
{
    /// <summary>
    /// Central logging system with pluggable appenders.
    /// Default appenders: Console + File.
    /// Provides static facade via CLog for convenience.
    /// </summary>
    public sealed class LogSystem : IGameSystem
    {
        public string Name => "LogSystem";
        // Keep high priority so it is initialized early.
        public int Priority => 950;

        private readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>(StringComparer.Ordinal);
        private Logger _defaultLogger;

        private LogLevel _minimumLevel = LogLevel.Info;
        public LogLevel MinimumLevel
        {
            get => _minimumLevel;
            set
            {
                _minimumLevel = value;
                if (_defaultLogger != null)
                {
                    _defaultLogger.MinimumLevel = value;
                }
            }
        }

        // Do not expose system appenders publicly
        // No system-level appenders; each logger owns its appenders

        public Logger DefaultLogger => _defaultLogger ??= CreateLogger("Default", _minimumLevel);

        public void OnEnable()
        {
            // Ensure default appenders exist once enabled.
            // Ensure default logger exists
            if (_defaultLogger == null)
            {
                _defaultLogger = CreateLogger("Default", _minimumLevel);
                _defaultLogger.AddAppender(new ConsoleAppender());
            }

            // Ensure default logger exists
            if (_defaultLogger == null)
            {
                _defaultLogger = CreateLogger("Default", _minimumLevel);
            }
        }

        public void OnDisable()
        {
            // Dispose and clear user loggers
            foreach (var kv in _loggers)
            {
                try { kv.Value.Dispose(); } catch (Exception e) { UnityEngine.Debug.LogError($"[LogSystem] Error disposing logger '{kv.Key}': {e}"); }
            }
            _loggers.Clear();
            _defaultLogger = null;

            // No system-level appenders to dispose
        }

        public void Update(float deltaTime, float unscaledDeltaTime) { }
        public void FixedUpdate(float deltaTime, float unscaledDeltaTime) { }

        /// <summary>
        /// Create or return an existing named logger.
        /// </summary>
        public Logger CreateLogger(string name, LogLevel? level = null)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "Default";
            if (_loggers.TryGetValue(name, out var logger))
            {
                return logger;
            }
            var newLogger = new Logger(name, level ?? _minimumLevel);
            _loggers[name] = newLogger;
            return newLogger;
        }

        /// <summary>
        /// Get a logger by name; returns null if not found.
        /// </summary>
        public Logger GetLogger(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "Default";
            _loggers.TryGetValue(name, out var logger);
            return logger;
        }

        /// <summary>
        /// Remove and dispose a logger by name.
        /// </summary>
        public bool RemoveLogger(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (_loggers.TryGetValue(name, out var logger))
            {
                try { logger.Dispose(); } catch (Exception e) { UnityEngine.Debug.LogError($"[LogSystem] Error disposing logger '{name}': {e}"); }
                _loggers.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Main log entry point.
        /// </summary>
        public void Log(LogLevel level, string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null)
        {
            DefaultLogger.Log(level, message, context, sourceFilePath, sourceLineNumber, memberName);
        }

        public void Trace(string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null) => DefaultLogger.Trace(message, context, sourceFilePath, sourceLineNumber, memberName);
        public void Debug(string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null) => DefaultLogger.Debug(message, context, sourceFilePath, sourceLineNumber, memberName);
        public void Info(string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null) => DefaultLogger.Info(message, context, sourceFilePath, sourceLineNumber, memberName);
        public void Warning(string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null) => DefaultLogger.Warning(message, context, sourceFilePath, sourceLineNumber, memberName);
        public void Error(string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null) => DefaultLogger.Error(message, context, sourceFilePath, sourceLineNumber, memberName);
        public void Fatal(string message, UnityEngine.Object context = null, string sourceFilePath = null, int sourceLineNumber = 0, string memberName = null) => DefaultLogger.Fatal(message, context, sourceFilePath, sourceLineNumber, memberName);
    }
}


