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
using System.Text;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    public enum LogLevel
    {
        NOLOG = 0,
        FATAL = 1,
        ERROR = 2,
        WARN = 3,
        INFO = 4,
        DEBUG = 5
    }

    public class Logger
    {
        private List<string> logEntries = new List<string>();
        private bool isBusy = false;

        public string LogFileName { get; set; }
        public string EventLogName { get; set; }
        public LogLevel Level { get; set; }

        public Logger()
        {
            Level = LogLevel.INFO;
            //LogFileName = Path.Combine(Settings.WindowsFolder, "Temp", string.Concat(Settings.ProductName, ".log"));
            LogFileName = Path.Combine(Settings.DataFolder, string.Concat(Settings.ComputerName, "_Log.txt"));
            EventLogName = "Application";
            if (!string.IsNullOrEmpty(LogFileName))
                Info($">>>>> Log to file '{LogFileName}' startet");
        }


        public void Debug(string msg)
        {
            if (Level >= LogLevel.DEBUG)
                AddLogEntry(LogLevel.DEBUG, msg, 0, null);
        }

        public void Debug(Exception ex)
        {
            if (Level >= LogLevel.DEBUG)
                AddLogEntry(LogLevel.DEBUG, ex.Message, 0, ex);
        }

        public void Debug(string msg, Exception ex)
        {
            if (Level >= LogLevel.DEBUG)
                AddLogEntry(LogLevel.DEBUG, msg, 0, ex);
        }

        public void Info(string msg, int eventId = 0)
        {
            if (Level >= LogLevel.INFO)
                AddLogEntry(LogLevel.INFO, msg, eventId, null);
        }

        public void Warn(string msg, int eventId = 0)
        {
            if (Level >= LogLevel.WARN)
                AddLogEntry(LogLevel.WARN, msg, eventId, null);
        }

        public void Warn(Exception ex)
        {
            if (Level >= LogLevel.WARN)
                AddLogEntry(LogLevel.WARN, ex.Message, 0, ex);
        }

        public void Warn(string msg, Exception ex)
        {
            if (Level >= LogLevel.WARN)
                AddLogEntry(LogLevel.WARN, msg, 0, ex);
        }

        public void Error(string msg, int eventId = 0)
        {
            if (Level >= LogLevel.ERROR)
                AddLogEntry(LogLevel.ERROR, msg, eventId, null);
        }

        public void Error(Exception ex)
        {
            if (Level >= LogLevel.ERROR)
                AddLogEntry(LogLevel.ERROR, ex.Message, 0, ex);
        }

        public void Error(string msg, Exception ex)
        {
            if (Level >= LogLevel.ERROR)
                AddLogEntry(LogLevel.ERROR, msg, 0, ex);
        }

        public void Fatal(string msg)
        {
            if (Level >= LogLevel.FATAL)
                AddLogEntry(LogLevel.FATAL, msg, 0, null);
        }

        public void Fatal(Exception ex)
        {
            if (Level >= LogLevel.FATAL)
                AddLogEntry(LogLevel.FATAL, ex.Message, 0, ex);
        }

        public void Fatal(string msg, Exception ex)
        {
            if (Level >= LogLevel.FATAL)
                AddLogEntry(LogLevel.FATAL, msg, 0, ex);
        }

        private void AddLogEntry(LogLevel level, string msg, int eventId = 0, Exception ex = null)
        {
            if (string.IsNullOrEmpty(EventLogName))
            {
                WriteToEventLog(level, msg, eventId, ex);
            }
            var logEntry = ($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}\t{level}\t{msg}");
            logEntries.Add(logEntry);
            if (!string.IsNullOrEmpty(ex?.StackTrace))
            {
                logEntries.Add(ex.StackTrace);
            }
            Console.WriteLine(logEntry);
            if (!string.IsNullOrEmpty(LogFileName))
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
                    if (Settings.IsAdministrator())
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
            if (isBusy) return;
            isBusy = true;
            Task.Run(() =>
            {
                try
                {
                    var logLines = new StringBuilder();
                    while (logEntries.Count > 0)
                    {
                        var itemsToSave = new List<string>(logEntries);
                        foreach (var logEntry in itemsToSave)
                        {
                            logLines.AppendLine(logEntry);
                            logEntries.Remove(logEntry);
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
                    isBusy = false;
                }
            });
        }
    }
}
