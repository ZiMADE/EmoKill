/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace ZiMADE.EmoKill
{
    public static class Settings
    {
        public static string ProductName => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false))?.Product;
        public static string ProductVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string ProductDate => ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute), false))?.InformationalVersion;
        public static string ProductRepository => ((AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyConfigurationAttribute), false))?.Configuration;
        public static string ServiceName => Assembly.GetExecutingAssembly().GetName().Name;
        public static string ServiceDescription => ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute), false)).Description;
        public static string WindowsFolder => Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;
        public static string ComputerName => System.Environment.GetEnvironmentVariable("COMPUTERNAME");

        public static Logger Log { get; private set; }

        public static void InitalizeLogger()
        {
            Log = new Logger();
        }

        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
