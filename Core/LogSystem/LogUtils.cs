using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Xuf.Core
{
    /// <summary>
    /// Static facade for convenient usage from anywhere.
    /// Automatically registers the log system if missing.
    /// Provides helper methods to get/create named loggers.
    /// </summary>
    public static class LogUtils
    {
        private static LogSystem EnsureSystem()
        {
            var mgr = Xuf.Core.CSystemManager.Instance;
            var sys = mgr.GetSystem<LogSystem>(throwException: false);
            if (sys == null)
            {
                sys = new LogSystem();
                mgr.RegisterGameSystem(sys);
            }
            return sys;
        }

        public static void SetMinimumLevel(LogLevel level)
        {
            EnsureSystem().MinimumLevel = level;
        }

        public static void Trace(string message, UnityEngine.Object context = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = null) => EnsureSystem().Trace(message, context, sourceFilePath, sourceLineNumber, memberName);
        public static void Debug(string message, UnityEngine.Object context = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = null) => EnsureSystem().Debug(message, context, sourceFilePath, sourceLineNumber, memberName);
        public static void Info(string message, UnityEngine.Object context = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = null) => EnsureSystem().Info(message, context, sourceFilePath, sourceLineNumber, memberName);
        public static void Warning(string message, UnityEngine.Object context = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = null) => EnsureSystem().Warning(message, context, sourceFilePath, sourceLineNumber, memberName);
        public static void Error(string message, UnityEngine.Object context = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = null) => EnsureSystem().Error(message, context, sourceFilePath, sourceLineNumber, memberName);
        public static void Fatal(string message, UnityEngine.Object context = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = null) => EnsureSystem().Fatal(message, context, sourceFilePath, sourceLineNumber, memberName);

        // Logger management
        public static Logger GetLogger(string name) => EnsureSystem().GetLogger(name) ?? EnsureSystem().CreateLogger(name);
        public static Logger CreateLogger(string name, LogLevel? level = null) => EnsureSystem().CreateLogger(name, level);
        public static bool RemoveLogger(string name) => EnsureSystem().RemoveLogger(name);
    }
}


