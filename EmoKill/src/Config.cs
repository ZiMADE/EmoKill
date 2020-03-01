/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Configuration;
using System.Reflection;

namespace ZiMADE.EmoKill
{
    internal class Config
    {
        private Configuration _Config;

        internal bool ZiMADECheckTestProcess { get; private set; }
        internal bool JPCERTCheckKeywords { get; private set; }
        internal bool JPCERTCheckRegistry { get; private set; }
        internal bool SOPHOSCheckServices { get; private set; }
        internal bool SOPHOSStopServices { get; private set; }
        internal bool SOPHOSDisableServices { get; private set; }
        internal string SharedDataFolder { get; private set; }

        internal Config()
        {
            Initialize();
        }

        protected void Initialize()
        {
            _Config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            ZiMADECheckTestProcess = ConfigSetting("ZiMADE.Check.TestProcess", true);
            JPCERTCheckKeywords = ConfigSetting("JPCERT.Check.Keywords", true);
            JPCERTCheckRegistry = ConfigSetting("JPCERT.Check.Registry", true);
            SOPHOSCheckServices = ConfigSetting("SOPHOS.Check.Services", true);
            SOPHOSStopServices = ConfigSetting("SOPHOS.Stop.Services", true);
            SOPHOSDisableServices = ConfigSetting("SOPHOS.Disable.Services", true);
            SharedDataFolder = ConfigSetting("SharedDataFolder", string.Empty);
        }

        internal void Reload()
        {
            Initialize();
        }

        private T ConfigSetting<T>(string key, T defaultValue)
        {
            object retval;
            try
            {
                retval = _Config.AppSettings.Settings[key]?.Value;
                if (retval == null)
                {
                    retval = defaultValue;
                }
            }
            catch
            {
                retval = defaultValue;
            }
            return (T)Convert.ChangeType(retval, typeof(T));
        }
    }
}
