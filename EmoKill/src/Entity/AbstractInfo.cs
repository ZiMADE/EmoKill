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
        public string ApplicationVersion { get; private set; }

        public AbstractInfo()
        {
            ComputerName = Settings.ComputerName;
            Timestamp = DateTime.Now;
            ApplicationVersion = Settings.ProductVersion;
        }

        public string ToJson()
        {
            return ToJson(Formatting.Indented);
        }

        public string ToJson(Formatting formatting)
        {
            return JsonConvert.SerializeObject(this, formatting);
        }

        public string ToXML()
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(stringwriter, this);
                return stringwriter.ToString();
            }
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
                var filename = string.Empty;
                if (string.IsNullOrEmpty(SourceName))
                {
                    filename = Path.Combine(Settings.DataFolder, $"{ComputerName}_{EntityName}_{Timestamp.ToString("yyyyMMdd-HHmmss")}.json");
                }
                else
                {
                    filename = Path.Combine(Settings.DataFolder, $"{ComputerName}_{EntityName}_{SourceName}_{UID}.json");
                }
                if (!File.Exists(filename))
                {
                    try
                    {
                        File.WriteAllText(filename, ToJson(Formatting.Indented), Encoding.Default);
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
