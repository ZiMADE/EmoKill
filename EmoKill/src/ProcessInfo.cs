/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;

namespace ZiMADE.EmoKill
{
    public class ProcessInfo
    {
        public int ProcessId { get; private set; }
        public string ProcessName { get; private set; }
        public string FileName { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime DetectedTime { get; private set; }
        public DateTime KilledTime { get; set; }
        public int EventId { get; set; }
        public string Message { get; set; }

        public long DetectionPeriod => (long)(DetectedTime- StartTime).TotalMilliseconds;
        public long KillingPeriod => (long)(KilledTime - DetectedTime).TotalMilliseconds;

        public ProcessInfo(int id, string processName, string fileName, DateTime started)
        {
            ProcessId = id;
            ProcessName = processName;
            FileName = fileName;
            StartTime = started;
            DetectedTime = DateTime.Now;
        }
    }
}
