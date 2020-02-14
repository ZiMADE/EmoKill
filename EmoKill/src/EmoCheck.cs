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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ZiMADE.EmoKill
{
    public class EmoCheck
    {
        private uint _SerialNo { get; set; }
        private string _Keywords => string.Concat(
                "duck,mfidl,targets,ptr,khmer,purge,metrics,acc,inet,msra,symbol,driver,",
                "sidebar,restore,msg,volume,cards,shext,query,roam,etw,mexico,basic,url,",
                "createa,blb,pal,cors,send,devices,radio,bid,format,thrd,taskmgr,timeout,",
                "vmd,ctl,bta,shlp,avi,exce,dbt,pfx,rtp,edge,mult,clr,wmistr,ellipse,vol,",
                "cyan,ses,guid,wce,wmp,dvb,elem,channel,space,digital,pdeft,violet,thunk");

        public Regex EmoProcessNames1 { get; private set; }
        public Regex EmoProcessNames2 { get; private set; }

        public EmoCheck()
        {
            _SerialNo = GetVolumeSerialNumber();
            EmoProcessNames1 = GenerateEmotetProcessNames();
            EmoProcessNames2 = GetEmotetFileNameFromRegistry(_SerialNo);
        }

        private Regex GenerateEmotetProcessNames()
        {
            var emoProcessNames = string.Empty;
            if (_SerialNo == 0)
            {
                // it was not possible to get serialno of sysvolume
                // so all possible processnames of Emotet are will be returned
                var keywordArray = _Keywords.Split(',');
                foreach (var keyword1 in keywordArray)
                {
                    foreach (var keyword2 in keywordArray)
                    {
                        emoProcessNames = string.Concat(emoProcessNames, "|", keyword1, keyword2);
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

                emoProcessNames = string.Concat(emoProcessNames, "|", emoProcessName);
                Settings.Log.Debug($"Emotet ProcessName for VolumeSerialNo {_SerialNo} is: {emoProcessName}");
            }
            emoProcessNames = string.Concat(emoProcessNames, "|", "EmoKillTest");
            emoProcessNames = string.Concat("(", emoProcessNames.Substring(1), ")");
            var regex = new Regex(emoProcessNames, RegexOptions.IgnoreCase);
            return regex;
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
            uint serial_number = 0;
            uint max_component_length = 0;
            StringBuilder sb_volume_name = new StringBuilder(256);
            UInt32 file_system_flags = new UInt32();
            StringBuilder sb_file_system_name = new StringBuilder(256);

            if (NativeMethods.GetVolumeInformation(
                    drive_letter,
                    sb_volume_name,
                    (UInt32)sb_volume_name.Capacity,
                    ref serial_number,
                    ref max_component_length,
                    ref file_system_flags,
                    sb_file_system_name,
                    (UInt32)sb_file_system_name.Capacity) == 0)
            {
                return 0;
            }
            else
            {
                return serial_number;
            }
        }

        private Regex GetEmotetFileNameFromRegistry(uint serial)
        {
            List<string> filenames = new List<string>();
            string filename;
            string reg_key_path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer";
            string reg_key_path_admin = "Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Explorer";

            // if emotet runs with user auth.(x64,x32)
            filename = QueryRegistry(serial, Registry.CurrentUser, reg_key_path);
            if (filename.Length > 0 && !filenames.Contains(filename))
            {
                filenames.Add(filename);
            }

            // if emotet runs with admin auth. (x32)
            filename = QueryRegistry(serial, Registry.LocalMachine, reg_key_path);
            if (filename.Length > 0 && !filenames.Contains(filename))
            {
                filenames.Add(filename);
            }

            // if emotet runs with admin auth. (x64)
            filename = QueryRegistry(serial, Registry.LocalMachine, reg_key_path_admin);
            if (filename.Length > 0 && !filenames.Contains(filename))
            {
                filenames.Add(filename);
            }

            if (filenames.Count == 0)
            {
                return null;
            }

            var emoProcessNames = string.Empty;
            foreach (var entry in filenames)
            {
                emoProcessNames = string.Concat(emoProcessNames, "|", entry);
            }
            emoProcessNames = string.Concat("(", emoProcessNames.Substring(1), ")");
            var regex = new Regex(emoProcessNames, RegexOptions.IgnoreCase);
            return regex;
        }

        private string QueryRegistry(uint serial, RegistryKey root, string key_path)
        {
            string filename = string.Empty;

            // convert integer to wstring stream
            string wstring_serial = ((int)serial).ToString("x");

            using (var key = root.OpenSubKey(key_path, false))
            {
                if (key == null)
                {
                    // Key does not exist
                    return filename;
                }

                byte[] buffer = (byte[])key.GetValue(wstring_serial, null);
                if (buffer == null)
                {
                    // Value does not exist
                    return filename;
                }

                Settings.Log?.Debug($"Found suspicous subkey: {wstring_serial}");

                // XOR registry value with drive serial num;
                var xor_key = BitConverter.GetBytes(serial);

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
