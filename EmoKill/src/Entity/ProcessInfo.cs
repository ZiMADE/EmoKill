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
        public override string EntityName => (ProcessName?.Equals("EmoKillTest", StringComparison.InvariantCultureIgnoreCase) == true) ? $"{EntityType.TestKill}" : $"{EntityType.EmoKill}"; 
        [JsonIgnore]
        public override string SourceName => string.Empty;
        public override string UID => ID.ToString().ToLower();
        public SystemInfo SystemInfo { get; private set; }
        public int ProcessId { get; private set; }
        public string ProcessName { get; private set; }
        public string ProcessUserName { get; set; }
        public string ProcessFileName { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime DetectedTime { get; private set; }
        public DateTime KilledTime { get; set; }
        public long DetectionTimeMS => (long)(DetectedTime - StartTime).TotalMilliseconds;
        public long KillingTimeMS => (long)(KilledTime - DetectedTime).TotalMilliseconds;
        public int EventId { get; set; }
        public string Message { get; set; }
        private Guid ID { get; set; }

        public ProcessInfo(int processId, string processName, string userName, string fileName, DateTime started)
        {
            ID = Guid.NewGuid();
            ProcessId = processId;
            ProcessName = processName;
            ProcessUserName = userName;
            ProcessFileName = fileName;
            StartTime = started;
            DetectedTime = DateTime.Now;
            SystemInfo = new SystemInfo(this);
        }
    }
}
