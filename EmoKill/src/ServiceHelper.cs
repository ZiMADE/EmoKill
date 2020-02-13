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
        private static string _InstallationPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Settings.ProductName);

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
            return InstallService(string.Empty);
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
                if (string.IsNullOrEmpty(cmd))
                {
                    CopyFilesToProgramPath();
                }
                var serviceFile = Path.Combine(_InstallationPath, $"{Settings.ServiceName}.exe");
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

        static public void ShowEmoKillLogfile()
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

        static public void DeleteEmoKillLogfile()
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

        private static void CopyFilesToProgramPath()
        {
            try
            {
                var currentPath = AppDomain.CurrentDomain.BaseDirectory;
                var filesToCopy = Directory.GetFiles(currentPath, "*.exe");
                foreach (var srcFile in filesToCopy)
                {
                    var dstFile = Path.Combine(_InstallationPath, Path.GetFileName(srcFile));
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

    }
}
