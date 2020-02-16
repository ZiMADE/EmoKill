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

namespace ZiMADE.EmoKill
{
    public class EmoCheck
    {
        private uint _SerialNo { get; set; }
        private string _SerialNoX => _SerialNo.ToString("x");

        private string _Keywords => string.Concat(
                "duck,mfidl,targets,ptr,khmer,purge,metrics,acc,inet,msra,symbol,driver,",
                "sidebar,restore,msg,volume,cards,shext,query,roam,etw,mexico,basic,url,",
                "createa,blb,pal,cors,send,devices,radio,bid,format,thrd,taskmgr,timeout,",
                "vmd,ctl,bta,shlp,avi,exce,dbt,pfx,rtp,edge,mult,clr,wmistr,ellipse,vol,",
                "cyan,ses,guid,wce,wmp,dvb,elem,channel,space,digital,pdeft,violet,thunk");

        public Dictionary<string, Entity.CheckInfo> EmoProcessNameDictionary { get; set; }

        private const string testProcessName = "EmoKillTest";

        public EmoCheck()
        {
            _SerialNo = GetVolumeSerialNumber();
            EmoProcessNameDictionary = new Dictionary<string, Entity.CheckInfo>();
            GenerateEmotetProcessNames();
            GetEmotetFileNameFromRegistry();
            AddEmoProcessNameIfNotExistInList(testProcessName, Entity.CheckInfoSource.Test);
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

        private void AddEmoProcessNameIfNotExistInList(string processName, Entity.CheckInfoSource source, string location = "")
        {
            if (!string.IsNullOrEmpty(processName))
            {
                var checkInfo = new Entity.CheckInfo(_SerialNo, processName, source, location);
                if (!EmoProcessNameDictionary.ContainsKey(checkInfo.UID))
                {
                    EmoProcessNameDictionary.Add(checkInfo.UID, checkInfo);
                }
            }
        }

        private void GenerateEmotetProcessNames()
        {
            if (_SerialNo == 0)
            {
                // it was not possible to get serialno of sysvolume
                // so all possible processnames of Emotet are will be returned
                var keywordArray = _Keywords.Split(',');
                foreach (var keyword1 in keywordArray)
                {
                    foreach (var keyword2 in keywordArray)
                    {
                        AddEmoProcessNameIfNotExistInList(string.Concat(keyword1, keyword2), Entity.CheckInfoSource.Keywords);
                    }
                }
                Settings.Log.Warn("Error getting serialno of volume, so all possible processnames of Emotet will be returned");
            }
            else
            {
                // generate emotet ProcessName depending on serialno of sysvolume
                uint q;
                uint seed;
                int mod;
                int keylen = _Keywords.Length;
                string emoProcessName = string.Empty;

                // first round
                seed = _SerialNo;
                q = seed / Convert.ToUInt32(keylen);
                mod = Convert.ToInt32(seed % Convert.ToUInt32(keylen));
                emoProcessName += GetWord(_Keywords, mod, keylen);

                // second round
                seed = 0xFFFFFFFF - q;
                mod = Convert.ToInt32(seed % Convert.ToUInt32(keylen));
                emoProcessName += GetWord(_Keywords, mod, keylen);

                AddEmoProcessNameIfNotExistInList(emoProcessName, Entity.CheckInfoSource.Keywords);
                Settings.Log.Debug($"Emotet ProcessName for VolumeSerialNo {_SerialNo} is: {emoProcessName}");
            }
        }

        private string GetWord(string keywords, int ptr, int keylen)
        {
            string keyword = string.Empty;

            for (int i = ptr; i > 0; i--)
            {
                if (keywords[i] != ',')
                {
                    continue;
                }
                else
                {
                    ptr = i;
                    break;
                }
            }
            if (keywords[ptr] == ',')
            {
                ptr++;
            }
            for (int i = ptr; i < keylen; i++)
            {
                if (keywords[i] != ',')
                {
                    keyword += keywords[i];
                    ptr++;
                }
                else
                {
                    break;
                }
            }
            return keyword;
        }

        private uint GetVolumeSerialNumber()
        {
            string drive_letter = "C:\\";
            StringBuilder sb_volume_name = new StringBuilder(256);
            StringBuilder sb_file_system_name = new StringBuilder(256);

            if (!NativeMethods.GetVolumeInformation(
                    drive_letter,
                    sb_volume_name,
                    sb_volume_name.Capacity,
                    out uint serial_number,
                    out uint max_component_length,
                    out uint file_system_flags,
                    sb_file_system_name,
                    sb_file_system_name.Capacity) == true)
            {
                return 0;
            }
            else
            {
                return serial_number;
            }
        }

        private void GetEmotetFileNameFromRegistry()
        {
            string filename;
            string reg_key_path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer";
            string reg_key_path_admin = "Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Explorer";

            // if emotet runs with user auth.(x64,x32)
            CheckUsersRegistry(reg_key_path);

            // if emotet runs with admin auth. (x32)
            filename = QueryRegistry(Registry.LocalMachine, reg_key_path);
            AddEmoProcessNameIfNotExistInList(filename, Entity.CheckInfoSource.Registry, $"HKEY_LOCAL_MACHINE\\{reg_key_path}:{_SerialNoX}");

            // if emotet runs with admin auth. (x64)
            filename = QueryRegistry(Registry.LocalMachine, reg_key_path_admin);
            AddEmoProcessNameIfNotExistInList(filename, Entity.CheckInfoSource.Registry, $"HKEY_LOCAL_MACHINE\\{reg_key_path_admin}:{_SerialNoX}");
        }

        private void CheckUsersRegistry(string key_path)
        {
            String[] subkeys = Registry.Users.GetSubKeyNames();
            foreach (var subkey in subkeys)
            {
                var filename = QueryRegistry(Registry.Users, $"{subkey}\\{key_path}");
                AddEmoProcessNameIfNotExistInList(filename, Entity.CheckInfoSource.Registry, $"HKEY_USERS\\{subkey}\\{key_path}:{_SerialNoX}");
            }
        }

        private string QueryRegistry(RegistryKey root, string key_path)
        {
            string filename = string.Empty;

            using (var key = root.OpenSubKey(key_path, false))
            {
                if (key == null)
                {
                    // Key does not exist
                    return filename;
                }

                byte[] buffer = (byte[])key.GetValue(_SerialNoX, null);
                if (buffer == null)
                {
                    // Value does not exist
                    return filename;
                }

                Settings.Log?.Debug($"Found suspicous subkey: {_SerialNoX}");

                // XOR registry value with drive serial num;
                var xor_key = BitConverter.GetBytes(_SerialNo);

                var decoded_chars = new byte[buffer.Length];
                for (uint i = 0; i < buffer.Length; i++)
                {
                    decoded_chars[i] = (byte)(xor_key[i % 4] ^ buffer[i]);
                }

                for (uint i = 0; i < buffer.Length; i++)
                {
                    if (0x20 < (int)decoded_chars[i] && (int)decoded_chars[i] < 0x7e)
                    {
                        filename += (char)decoded_chars[i];
                    }
                }
            }
            return filename;
        }
    }
}
