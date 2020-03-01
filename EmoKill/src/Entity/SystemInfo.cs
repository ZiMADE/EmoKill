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
    public class SystemInfo : AbstractInfo
    {
        public override string EntityName => $"{EntityType.SysInfo}";
        [JsonIgnore]
        public override string SourceName => string.Empty;
        public override string UID => ID.ToString().ToLower();
        public string EntryAssembly { get; private set; }
        public string ExecutingAssembly { get; private set; }
        public string ExecutingFolder { get; private set; }
        public string InstallFolder { get; private set; }
        public string WindowsFolder { get; private set; }
        public string DataFolder { get; private set; }
        public string OSVersion { get; private set; }
        public string OSPlatform { get; private set; }
        public string OSServicePack { get; private set; }
        public int Is64BitOperatingSystem { get; private set; }
        public int Is64BitProcess { get; private set; }
        public string IPAddress { get; private set; }
        public string ServiceStatus { get; private set; }

        private Guid ID { get; set; }

        public SystemInfo()
        {
            SetProperties(null);
        }

        public SystemInfo(object parentEntity)
        {
            SetProperties(parentEntity);
        }

        private void SetProperties(object parentEntity)
        {
            ID = Guid.NewGuid();
            IPAddress = Settings.IPAddress;
            EntryAssembly = Settings.EntryAssembly;
            ExecutingAssembly = Settings.ExecutingAssembly;
            ExecutingFolder = Settings.ExecutingFolder;
            InstallFolder = Settings.InstallFolder;
            WindowsFolder = Settings.WindowsFolder;
            DataFolder = Settings.DataFolder;
            OSVersion = Settings.OSVersion.VersionString;
            OSPlatform = $"{(int)Settings.OSVersion.Platform} ({Settings.OSVersion.Platform})";
            OSServicePack = Settings.OSVersion.ServicePack;
            Is64BitOperatingSystem = Settings.Is64BitOperatingSystem ? 1 : 0;
            Is64BitProcess = Settings.Is64BitProcess ? 1 : 0;
            var svcStatus = ServiceHelper.GetServiceState();
            ServiceStatus = (svcStatus == null) ? "UNKNOWN" : $"{svcStatus}";
            ParentEntity = parentEntity;
        }
    }
}
