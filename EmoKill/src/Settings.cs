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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;

namespace ZiMADE.EmoKill
{
    public static class Settings
    {
        public static string ProductName => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false))?.Product;
        public static string ProductVersion => ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute), false))?.InformationalVersion;
        public static DateTime ProductDate => GetBuildDateTime();
        public static string ProductRepository => ((AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyConfigurationAttribute), false))?.Configuration;
        internal static string ServiceName => Assembly.GetExecutingAssembly().GetName().Name;
        internal static string ServiceDescription => ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute), false)).Description;
        public static bool IsAdministrator { get; private set; }

        internal static Logger Log { get; private set; }
        internal static Config Config { get; private set; }
        internal static string AssemblyVersion { get; private set; }
        internal static string EntryAssembly { get; private set; }
        internal static string ExecutingAssembly { get; private set; }
        internal static string ExecutingFolder { get; private set; }
        internal static string WindowsFolder { get; private set; }
        internal static string InstallFolder { get; private set; }
        internal static string InstallX86Folder { get; private set; }
        internal static string DataFolder { get; private set; }
        internal static string ComputerName { get; private set; }
        internal static OperatingSystem OSVersion { get; private set; }
        internal static bool Is64BitOperatingSystem { get; private set; }
        internal static bool Is64BitProcess { get; private set; }
        internal static int CurrentProcessId { get; private set; }
        internal static string IPAddress { get; private set; }

        public static void Initialize()
        {
            if (Log == null)
            {
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                EntryAssembly = Assembly.GetEntryAssembly().GetName().FullName;
                ExecutingAssembly = Assembly.GetExecutingAssembly().GetName().FullName;
                ExecutingFolder = AppDomain.CurrentDomain.BaseDirectory;
                WindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;
                InstallFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), ProductName);
                InstallX86Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), ProductName);
                DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ProductName);
                ComputerName = System.Environment.MachineName;
                OSVersion = System.Environment.OSVersion;
                Is64BitOperatingSystem = System.Environment.Is64BitOperatingSystem;
                Is64BitProcess = System.Environment.Is64BitProcess;
                CurrentProcessId = Process.GetCurrentProcess().Id;

                Log = new Logger();
                Config = new Config();
                IsAdministrator = IsAdmin();
                IPAddress = LocalIPAddress();

                new Entity.SystemInfo().SaveToJsonFile();

                if (Is64BitOperatingSystem && !Is64BitProcess)
                {
                    Log.Warn($"At this system the 64bit executable of {ProductName} should be used, otherwise its just possible to detect and kill only 32bit processes of Emotet !!!");
                }
            }
        }

        private static bool IsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// This method will give the local address that would be used to connect to the specified remote host.
        /// There is no real connection established, hence the specified remote ip can be unreachable.
        /// </summary>
        /// <returns>IP-Address or empty string, if ipaddress could not be detectet</returns>
        private static string LocalIPAddress()
        {
            try
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    return string.Empty;
                }
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("1.1.1.1", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        private static DateTime GetBuildDateTime()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var timestamp = new TimeSpan(
                TimeSpan.TicksPerDay * version.Build + // days since 1 January 2000
                TimeSpan.TicksPerSecond * 2 * version.Revision); // seconds since midnight, (multiply by 2 to get original)
            var buildDateTime = new DateTime(2000, 1, 1).Add(timestamp);
            return buildDateTime;
        }
    }
}
