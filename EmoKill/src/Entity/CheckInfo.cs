/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

namespace ZiMADE.EmoKill.Entity
{
    public class CheckInfo : AbstractInfo
    {
        public override string EntityName => $"{EntityType.EmoCheck}";
        public CheckInfoSource Source { get; private set; }
        public override string SourceName => $"{Source}";
        public override string UID => MD5Hash;
        public SystemInfo SystemInfo { get; private set; }
        public uint VolumeSerialNumber { get; private set; }
        public string EmotetProcessName { get; private set; }
        public string Location { get; private set; }

        private string UniqueKey => $"{ComputerName}|{VolumeSerialNumber}|{EmotetProcessName}|{Source}|{Location}";
        private string MD5Hash { get; set; }

        public CheckInfo(string emotetName, CheckInfoSource source, string location)
        {
            SetProperties(0, emotetName, source, location);
        }

        public CheckInfo(uint serialno, string emotetName, CheckInfoSource source, string location)
        {
            SetProperties(serialno, emotetName, source, location);
        }

        private void SetProperties(uint serialno, string emotetName, CheckInfoSource source, string location)
        {
            VolumeSerialNumber = serialno;
            Source = source;
            EmotetProcessName = emotetName;
            Location = location;
            MD5Hash = Crypto.MD5Hash(UniqueKey);
            SystemInfo = new SystemInfo(this);
        }
    }
}
