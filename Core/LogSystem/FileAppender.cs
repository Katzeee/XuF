using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Xuf.Core
{
    /// <summary>
    /// File appender that writes to Application.persistentDataPath/Logs.
    /// Rolls file daily with name log_yyyyMMdd.txt.
    /// </summary>
    internal sealed class FileAppender : ILogAppender
    {
        private readonly object _lock = new object();
        private readonly string _filePath;
        private StreamWriter _writer;

        public string CurrentFilePath { get; private set; }
        private ILogFormatter _formatter;

        // Preferred: accept full file path from caller
        public FileAppender(string filePath, ILogFormatter formatter = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                var dir = Path.Combine(Application.persistentDataPath, "Logs");
                Directory.CreateDirectory(dir);
                var fileName = $"log_{DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.txt";
                _filePath = Path.Combine(dir, fileName);
            }
            else
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                _filePath = filePath;
            }
            CurrentFilePath = _filePath;
            _formatter = formatter ?? new TemplateLogFormatter();
        }

        public void Append(in LogEvent logEvent)
        {
            try
            {
                EnsureWriter();

                string formatted = _formatter.Format(logEvent);
                _writer.WriteLine(formatted);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LogSystem] Failed to write log file: {e}");
            }
        }

        public void Flush()
        {
            try
            {
                _writer?.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LogSystem] Failed to flush log file: {e}");
            }
        }

        public void Dispose()
        {
            try
            {
                _writer?.Flush();
                _writer?.Dispose();
                _writer = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LogSystem] Failed to close log file: {e}");
            }
        }

        private void EnsureWriter()
        {
            lock (_lock)
            {
                if (_writer != null)
                    return;

                _writer = new StreamWriter(_filePath, append: true, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                {
                    AutoFlush = true
                };
            }
        }
    }
}


