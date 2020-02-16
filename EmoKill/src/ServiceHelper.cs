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
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    public static class ServiceHelper
    {
        public static ServiceControllerStatus? GetServiceState()
        {
            ServiceControllerStatus? retval = null;
            try
            {
                var sc = new ServiceController(Settings.ServiceName, Settings.ComputerName);
                retval = sc.Status;
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
            return retval;
        }

        public static string InstallService()
        {
            var result = string.Empty;
            var serviceState = GetServiceState();
            var serviceAlreadyInstalled = serviceState != null;
            if (serviceAlreadyInstalled && Settings.Is64BitOperatingSystem && Settings.Is64BitProcess && Directory.Exists(Settings.InstallX86Folder))
            {
                UninstallService();
                RemoveX86Files();
                serviceAlreadyInstalled = false;
            }
            else if (serviceAlreadyInstalled && serviceState != ServiceControllerStatus.Stopped && serviceState != ServiceControllerStatus.StopPending)
            {
                StopService();
            }
            CopyFilesToInstallFolder();
            if (serviceAlreadyInstalled)
            {
                StartService();
            }
            else
            {
                result = InstallService(string.Empty);
            }
            serviceState = GetServiceState();
            if (serviceState == null)
                result += $"Service State: UNKNOWN";
            else
                result += $"Service State: {serviceState}";
            return result;
        }

        public static string UninstallService()
        {
            return InstallService("/uninstall");
        }

        private static string InstallService(string cmd)
        {
            Settings.Log?.Debug($"{nameof(InstallService)}({cmd})");
            var retval = string.Empty;
            var instUtilx86 = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe";
            var instUtilx64 = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe";
            var installUtil = string.Empty;
            if (File.Exists(instUtilx64))
            {
                installUtil = instUtilx64;
            }
            else if (File.Exists(instUtilx86))
            {
                installUtil = instUtilx86;
            }
            if (!string.IsNullOrEmpty(installUtil))
            {
                var serviceFile = Path.Combine(Settings.InstallFolder, $"{Settings.ServiceName}.exe");
                if (File.Exists(serviceFile))
                {
                    retval = StartProgam(installUtil, $"\"{serviceFile}\" {cmd}");
                }
                else
                {
                    Settings.Log?.Error($"InstallService failed: cannot find {serviceFile}");
                }
            }
            else
            {
                Settings.Log?.Error("InstallService failed: cannot find installutil.exe");
            }
            return retval;
        }

        private static string StartProgam(string filename, string args)
        {
            var sb = new StringBuilder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                sb.AppendLine(process.StandardOutput.ReadLine());
            }
            return sb.ToString();
        }

        public static ServiceControllerStatus? StartService()
        {
            ServiceControllerStatus? retval = null;
            try
            {
                var sc = new ServiceController(Settings.ServiceName, Settings.ComputerName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                    retval = sc.Status;
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
            return retval;
        }

        public static ServiceControllerStatus? StopService()
        {
            ServiceControllerStatus? retval = null;
            try
            {
                var sc = new ServiceController(Settings.ServiceName, Settings.ComputerName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                    retval = sc.Status;
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
            return retval;
        }


        public static void RestartService()
        {
            Task.Run(() =>
            {
                try
                {
                    if (ThisIsTheServiceProcess())
                    {
                        Settings.Log?.Debug("DoRestart: spawning new process to restart service...");
                        string serviceName = Settings.ServiceName;
                        Process process = new Process();
                        process.StartInfo.FileName = "cmd";
                        process.StartInfo.Arguments = string.Format("/c net stop \"{0}\" && net start \"{0}\"", serviceName);
                        process.Start();
                    }
                    else
                    {
                        var sc = new ServiceController(Settings.ServiceName, Settings.ComputerName);
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                        }
                        sc.Start();
                    }
                }
                catch (Exception ex)
                {
                    Settings.Log?.Fatal(ex);
                }
            });
        }

        public static void ShowEmoKillLogfile()
        {
            try
            {
                if (!string.IsNullOrEmpty(Settings.Log?.LogFileName) && File.Exists(Settings.Log?.LogFileName))
                {
                    Settings.Log?.Info($"Show LogFile '{Settings.Log?.LogFileName}'");
                    Process process = new Process();
                    process.StartInfo.FileName = "notepad";
                    process.StartInfo.Arguments = Settings.Log?.LogFileName;
                    process.Start();
                }
                else
                {
                    Settings.Log?.Warn($"LogFile '{Settings.Log?.LogFileName}' does not exist.");
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
        }

        public static void DeleteEmoKillLogfile()
        {
            try
            {
                if (!string.IsNullOrEmpty(Settings.Log?.LogFileName) && File.Exists(Settings.Log?.LogFileName))
                {
                    Settings.Log?.Info($"Delete LogFile '{Settings.Log?.LogFileName}'");
                    File.Delete(Settings.Log?.LogFileName);
                }
                else
                {
                    Settings.Log?.Warn($"LogFile '{Settings.Log?.LogFileName}' does not exist.");
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
        }

        public static string GetSystemInfo()
        {
            var retval = string.Empty;
            try
            {
                var sysInfo = new Entity.SystemInfo();
                sysInfo.SaveToJsonFile();
                var lines = sysInfo.ToJson().Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\\\\", "\\").Replace("\r", "").Split('\n');
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        sb.AppendLine(line);
                    }
                }
                retval = sb.ToString();
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
            return retval.ToString();
        }

        private static bool ThisIsTheServiceProcess()
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            var processList = Process.GetProcessesByName(Settings.ServiceName);
            foreach (var process in processList)
            {
                if (process.Id == currentProcessId)
                {
                    return true;
                }
            }
            return false;
        }

        private static void CopyFilesToInstallFolder()
        {
            try
            {
                var currentPath = AppDomain.CurrentDomain.BaseDirectory;
                var filesToCopy = Directory.GetFiles(currentPath);
                foreach (var srcFile in filesToCopy)
                {
                    var dstFile = Path.Combine(Settings.InstallFolder, Path.GetFileName(srcFile));
                    Settings.Log?.Info($"Copy file '{srcFile}' to '{dstFile}'");
                    if (!Directory.Exists(Path.GetDirectoryName(dstFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dstFile));
                    }
                    File.Copy(srcFile, dstFile, true);
                }

            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
        }

        private static void RemoveX86Files()
        {
            try
            {
                if (Directory.Exists(Settings.InstallX86Folder))
                {
                    var filesToRemove = Directory.GetFiles(Settings.InstallX86Folder);
                    foreach (var srcFile in filesToRemove)
                    {
                        Settings.Log?.Info($"Delete file '{srcFile}'");
                        File.Delete(srcFile);
                    }
                    Directory.Delete(Settings.InstallX86Folder);
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
        }

    }
}
