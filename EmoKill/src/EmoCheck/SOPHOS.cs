/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2015 JPCERT Coordination Center. All Rights Reserved.
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 *
 *  DESCRIPTION
 *      This class is based on the SOPHOS Knowledge Base article 
 *      "How to delete orphaned [Numeric] Emotet windows services".
 *      All credits goes to SOPHOS. Source is available at:
 *      https://community.sophos.com/kb/en-us/133423
 */

using System;
using System.Collections.Generic;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;

namespace ZiMADE.EmoKill.EmoCheck
{
    public class SOPHOS
    {
        internal Dictionary<string, Entity.CheckInfo> EmoProcessNameDictionary { get; set; }

        public SOPHOS()
        {
            EmoProcessNameDictionary = new Dictionary<string, Entity.CheckInfo>();
            if (Settings.Config.SOPHOSCheckServices)
            {
                GenerateEmotetProcessNames();
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

        private void GenerateEmotetProcessNames()
        {
            try
            {
                var regex = new Regex("^[0-9]", RegexOptions.IgnoreCase);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    string serviceName = obj["Name"] as string;
                    if (regex?.Match(serviceName)?.Success == true)
                    {
                        string pathName = obj["PathName"] as string;
                        string startMode = obj["StartMode"] as string;
                        var serviceController = new ServiceController(serviceName);
                        var serviceStatus = serviceController.Status;
                        var location = $"StartMode: {startMode}, ServiceStatus: {serviceStatus}, PathName: {pathName}, trying to stop and disable the service!";
                        AddEmoProcessNameIfNotExistInList(serviceName, CheckInfoSource.Services, location);
                        if (Settings.Config.SOPHOSDisableServices && !startMode.Equals("Disabled", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var m = new ManagementObject($"Win32_Service.Name=\"{serviceName}\""))
                            {
                                try
                                {
                                    m.InvokeMethod("ChangeStartMode", new object[] { "Disabled" });
                                }
                                catch (Exception ex)
                                {
                                    Settings.Log.Error($"Error while disabling service '{serviceName}': {ex.Message}");
                                }
                            }
                        }
                        if (Settings.Config.SOPHOSStopServices && serviceStatus != ServiceControllerStatus.Stopped)
                        {
                            try
                            {
                                serviceController.Stop();
                            }
                            catch (Exception ex)
                            {
                                Settings.Log.Error($"Error while stopping service '{serviceName}': {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Settings.Log.Error(ex);
            }
        }
    }
}
