/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ZiMADE.EmoKill.Entity
{
    public abstract class AbstractInfo
    {
        public abstract string EntityName { get; }
        public abstract string SourceName { get; }
        public abstract string UID { get; }
        public string ComputerName { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string ProductVersion { get; private set; }

        [JsonIgnore]
        internal object ParentEntity { get; set; }

        public string FileName
        {
            get
            {
                if (ParentEntity == null)
                {
                    if (string.IsNullOrWhiteSpace(SourceName))
                    {
                        return Path.Combine(Settings.DataFolder, $"{ComputerName}_{EntityName}_{Timestamp.ToString("yyyyMMdd-HHmmss")}_{UID}.json");
                    }
                    else
                    {
                        return Path.Combine(Settings.DataFolder, $"{ComputerName}_{EntityName}_{SourceName}_{Timestamp.ToString("yyyyMMdd-HHmmss")}_{UID}.json");
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public AbstractInfo()
        {
            ComputerName = Settings.ComputerName;
            Timestamp = DateTime.Now;
            ProductVersion = Settings.ProductVersion;
            ParentEntity = null;
        }

        public string ToJson()
        {
            return ToJson(Formatting.Indented);
        }

        public string ToJson(Formatting formatting)
        {
            return JsonConvert.SerializeObject(this, formatting);
        }

        /// <summary>
        /// Converts Class to a human readable info
        /// </summary>
        /// <returns>Ident formatted string without special characters</returns>
        public string ToJsonString()
        {
            var retval = string.Empty;
            var lines = ToJson().Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\\\\", "\\").Replace("\r", "").Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine(line);
                }
            }
            retval = sb.ToString();
            return retval;
        }

        public override string ToString()
        {
            return ToJson();
        }

        public void SaveToJsonFile()
        {
            try
            {
                if (!Directory.Exists(Settings.DataFolder))
                {
                    Directory.CreateDirectory(Settings.DataFolder);
                }
                if (!File.Exists(FileName))
                {
                    try
                    {
                        File.WriteAllText(FileName, ToJson(Formatting.Indented), Encoding.Default);
                        if (!string.IsNullOrWhiteSpace(Settings.Config.SharedDataFolder))
                        {
                            InfoDispatcher.SendInformationToShare();
                        }
                    }
                    catch (IOException)
                    {
                        //the file is unavailable because it is:
                        //still being written to
                        //or being processed by another thread
                    }
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
        }
    }
}
