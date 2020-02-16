/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

namespace ZiMADE.EmoKill.Entity
{
    public enum CheckInfoSource
    {
        Registry = 1,
        Keywords = 2,
        Test = 9999,
    }

    public class CheckInfo : AbstractInfo
    {
        public override string EntityName => "EmoCheck";
        public override string SourceName => $"{Source}";
        public override string UID => MD5Hash;
        public uint VolumeSerialNumber { get; private set; }
        public string EmotetProcessName { get; private set; }
        public CheckInfoSource Source { get; private set; }
        public string Location { get; private set; }

        private string UniqueKey => $"{ComputerName}|{VolumeSerialNumber}|{EmotetProcessName}|{Source}|{Location}";
        private string MD5Hash { get; set; }

        public CheckInfo(uint serialno, string emotetName, CheckInfoSource source, string location)
        {
            VolumeSerialNumber = serialno;
            Source = source;
            EmotetProcessName = emotetName;
            Location = location;
            MD5Hash = Crypto.MD5Hash(UniqueKey);
        }
    }
}
