/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Diagnostics;
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
        public static string EntryAssembly => Assembly.GetEntryAssembly().GetName().FullName;
        public static string ExecutingAssembly => Assembly.GetExecutingAssembly().GetName().FullName;
        public static string ExecutingFolder => AppDomain.CurrentDomain.BaseDirectory;
        public static string WindowsFolder => Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;
        public static string InstallFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), ProductName);
        public static string InstallX86Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), ProductName);
        public static string DataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ProductName);
        public static string ComputerName => System.Environment.MachineName;
        public static OperatingSystem OSVersion => System.Environment.OSVersion;
        public static bool Is64BitOperatingSystem => System.Environment.Is64BitOperatingSystem;
        public static bool Is64BitProcess => System.Environment.Is64BitProcess;


        public static Logger Log { get; private set; }

        public static void Initalize()
        {
            Log = new Logger();
            new Entity.SystemInfo().SaveToJsonFile();
            if (Is64BitOperatingSystem && !Is64BitProcess)
            {
                Log.Warn($"At this system the 64bit executable of {ProductName} should be used, otherwise its just possible to detect and kill only 32bit processes of Emotet !!!");
            }
        }

        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static bool IsWin64Emulator(this Process process)
        {
            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                return NativeMethods.IsWow64Process(process.Handle, out bool retVal) && retVal;
            }

            return false; // not on 64-bit Windows Emulator
        }
    }
}
