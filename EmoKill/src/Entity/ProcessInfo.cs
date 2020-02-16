/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using Newtonsoft.Json;
using System;

namespace ZiMADE.EmoKill.Entity
{
    public class ProcessInfo : AbstractInfo
    {
        public override string EntityName => "EmoKill";
        [JsonIgnore]
        public override string SourceName => string.Empty;
        public override string UID => ID.ToString().ToLower();
        public int ProcessId { get; private set; }
        public string ProcessName { get; private set; }
        public string FileName { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime DetectedTime { get; private set; }
        public DateTime KilledTime { get; set; }
        public int EventId { get; set; }
        public string Message { get; set; }

        [JsonIgnore]
        public long DetectionPeriod => (long)(DetectedTime - StartTime).TotalMilliseconds;
        [JsonIgnore]
        public long KillingPeriod => (long)(KilledTime - DetectedTime).TotalMilliseconds;

        private Guid ID { get; set; }

        public ProcessInfo(int processId, string processName, string fileName, DateTime started)
        {
            ID = Guid.NewGuid();
            ProcessId = processId;
            ProcessName = processName;
            FileName = fileName;
            StartTime = started;
            DetectedTime = DateTime.Now;
        }
    }
}
