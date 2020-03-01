/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2015 JPCERT Coordination Center. All Rights Reserved.
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 *
 *  DESCRIPTION
 *      This class is a port of the C++ source of EmoCheck 0.0.2 by JPCERTCC.
 *      All credits goes to JPCERTCC. Source of EmoCheck by JPCERTCC is available 
 *      at: https://github.com/JPCERTCC/EmoCheck/
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ZiMADE.EmoKill.EmoCheck
{
    public class ZiMADE
    {
        internal Dictionary<string, Entity.CheckInfo> EmoProcessNameDictionary { get; set; }

        private const string testProcessName = "EmoKillTest";

        public ZiMADE()
        {
            EmoProcessNameDictionary = new Dictionary<string, Entity.CheckInfo>();
            if (Settings.Config.ZiMADECheckTestProcess)
            {
                AddEmoProcessNameIfNotExistInList(testProcessName, CheckInfoSource.Test);
            }
        }

        public bool EmoProcessNameMatches(string processName)
        {
            var retval = false;
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var entry in EmoProcessNameDictionary)
                {
                    sb.Append(string.Concat("|", Regex.Escape(entry.Value.EmotetProcessName)));
                }
                if (sb.Length > 0)
                {
                    var regex = new Regex(string.Concat("(", sb.ToString().Substring(1), ")"), RegexOptions.IgnoreCase);
                    if (regex?.Match(processName)?.Success == true)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Settings.Log.Fatal($"{nameof(EmoProcessNameMatches)}(processName={processName})", ex);
            }
            return retval;
        }

        private void AddEmoProcessNameIfNotExistInList(string processName, CheckInfoSource source, string location = "")
        {
            if (!string.IsNullOrWhiteSpace(processName))
            {
                var checkInfo = new Entity.CheckInfo(processName, source, location);
                if (!EmoProcessNameDictionary.ContainsKey(checkInfo.UID))
                {
                    EmoProcessNameDictionary.Add(checkInfo.UID, checkInfo);
                }
            }
        }
    }
}
