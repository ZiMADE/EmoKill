/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    internal enum LogLevel
    {
        NOLOG = 0,
        FATAL = 1,
        ERROR = 2,
        WARN = 3,
        INFO = 4,
        DEBUG = 5
    }

    internal class Logger
    {
        private string _AssemblyName => Assembly.GetCallingAssembly().GetName().Name;
        private List<string> _LogEntries = new List<string>();
        private bool _IsBusy = false;

        internal string LogFileName { get; set; }
        internal string EventLogName { get; set; }
        internal LogLevel Level { get; set; }

        internal Logger()
        {
            Level = LogLevel.INFO;
            //LogFileName = Path.Combine(Settings.WindowsFolder, "Temp", string.Concat(Settings.ProductName, ".log"));
            LogFileName = Path.Combine(Settings.DataFolder, string.Concat(Settings.ComputerName, "_Log.txt"));
            EventLogName = "Application";
            if (!string.IsNullOrWhiteSpace(LogFileName))
                Info($">>>>> Log to file '{LogFileName}' startet");
        }


        internal void Debug(string msg)
        {
            if (Level >= LogLevel.DEBUG)
                AddLogEntry(LogLevel.DEBUG, msg, 0, null);
        }

        internal void Debug(Exception ex)
        {
            if (Level >= LogLevel.DEBUG)
                AddLogEntry(LogLevel.DEBUG, ex.Message, 0, ex);
        }

        internal void Debug(string msg, Exception ex)
        {
            if (Level >= LogLevel.DEBUG)
                AddLogEntry(LogLevel.DEBUG, msg, 0, ex);
        }

        internal void Info(string msg, int eventId = 0)
        {
            if (Level >= LogLevel.INFO)
                AddLogEntry(LogLevel.INFO, msg, eventId, null);
        }

        internal void Warn(string msg, int eventId = 0)
        {
            if (Level >= LogLevel.WARN)
                AddLogEntry(LogLevel.WARN, msg, eventId, null);
        }

        internal void Warn(Exception ex)
        {
            if (Level >= LogLevel.WARN)
                AddLogEntry(LogLevel.WARN, ex.Message, 0, ex);
        }

        internal void Warn(string msg, Exception ex)
        {
            if (Level >= LogLevel.WARN)
                AddLogEntry(LogLevel.WARN, msg, 0, ex);
        }

        internal void Error(string msg, int eventId = 0)
        {
            if (Level >= LogLevel.ERROR)
                AddLogEntry(LogLevel.ERROR, msg, eventId, null);
        }

        internal void Error(Exception ex)
        {
            if (Level >= LogLevel.ERROR)
                AddLogEntry(LogLevel.ERROR, ex.Message, 0, ex);
        }

        internal void Error(string msg, Exception ex)
        {
            if (Level >= LogLevel.ERROR)
                AddLogEntry(LogLevel.ERROR, msg, 0, ex);
        }

        internal void Fatal(string msg)
        {
            if (Level >= LogLevel.FATAL)
                AddLogEntry(LogLevel.FATAL, msg, 0, null);
        }

        internal void Fatal(Exception ex)
        {
            if (Level >= LogLevel.FATAL)
                AddLogEntry(LogLevel.FATAL, ex.Message, 0, ex);
        }

        internal void Fatal(string msg, Exception ex)
        {
            if (Level >= LogLevel.FATAL)
                AddLogEntry(LogLevel.FATAL, msg, 0, ex);
        }

        private void AddLogEntry(LogLevel level, string msg, int eventId = 0, Exception ex = null)
        {
            if (string.IsNullOrWhiteSpace(EventLogName))
            {
                WriteToEventLog(level, msg, eventId, ex);
            }
            var logEntry = ($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}\t{level}\t{msg}");
            _LogEntries.Add(logEntry);
            if (!string.IsNullOrWhiteSpace(ex?.StackTrace))
            {
                _LogEntries.Add(ex.StackTrace);
            }
            Console.WriteLine(logEntry);
            if (!string.IsNullOrWhiteSpace(LogFileName))
            {
                SaveToFile();
            }
        }


        private void WriteToEventLog(LogLevel level, string msg, int eventId, Exception ex)
        {
            Task.Run(() =>
            {
                try
                {
                    if (Settings.IsAdministrator)
                    {
                        using (EventLog eventLog = new EventLog(EventLogName))
                        {
                            eventLog.Source = Settings.ProductName;
                            eventLog.WriteEntry($"{msg}\r\n{ex?.StackTrace}", EventLogLevel(level), eventId);
                        }
                    }
                }
                catch { }
            });
        }

        private EventLogEntryType EventLogLevel(LogLevel level)
        {
            var retval = EventLogEntryType.Information;
            switch (level)
            {
                case LogLevel.FATAL:
                    retval = EventLogEntryType.Error;
                    break;
                case LogLevel.ERROR:
                    retval = EventLogEntryType.Error;
                    break;
                case LogLevel.WARN:
                    retval = EventLogEntryType.Warning;
                    break;
                case LogLevel.INFO:
                    retval = EventLogEntryType.Information;
                    break;
                case LogLevel.DEBUG:
                    retval = EventLogEntryType.Information;
                    break;
            }
            return retval;
        }
        
        private void SaveToFile()
        {
            if (_IsBusy) return;
            _IsBusy = true;
            Task.Run(() =>
            {
                try
                {
                    var logLines = new StringBuilder();
                    while (_LogEntries.Count > 0)
                    {
                        var itemsToSave = new List<string>(_LogEntries);
                        foreach (var logEntry in itemsToSave)
                        {
                            logLines.AppendLine(logEntry);
                            _LogEntries.Remove(logEntry);
                        }
                    }

                    var logPath = Path.GetDirectoryName(LogFileName);
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    File.AppendAllText(LogFileName, logLines.ToString(), Encoding.Default);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    _IsBusy = false;
                }
            });
        }
    }
}
