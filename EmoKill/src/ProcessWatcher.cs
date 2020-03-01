/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    public class ProcessWatcher : IDisposable
    {
        private ManagementEventWatcher _ProcessStartEvent;
        private EmoCheck.JPCERT _EmoCheck_JPCERT;
        private EmoCheck.SOPHOS _EmoCheck_SOPHOS;
        private EmoCheck.ZiMADE _EmoCheck_ZiMADE;

        public bool IsRunning { get; private set; }

        public ProcessWatcher()
        {
            IsRunning = false;
            Settings.Log?.Debug($"Create new {nameof(ProcessWatcher)}");
            Activate();
        }

        private void Activate()
        {
            Settings.Log?.Debug(nameof(Activate));
            try
            {
                if (Settings.IsAdministrator)
                {
                    _ProcessStartEvent = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStartTrace");
                    _ProcessStartEvent.EventArrived += new EventArrivedEventHandler(ProcessStartEventArrived);
                    _ProcessStartEvent.Start();

                    _EmoCheck_JPCERT = new EmoCheck.JPCERT();
                    SaveToJsonFile(_EmoCheck_JPCERT.EmoProcessNameDictionary);
                    _EmoCheck_SOPHOS = new EmoCheck.SOPHOS();
                    SaveToJsonFile(_EmoCheck_SOPHOS.EmoProcessNameDictionary);
                    _EmoCheck_ZiMADE = new EmoCheck.ZiMADE();
                    SaveToJsonFile(_EmoCheck_ZiMADE.EmoProcessNameDictionary);

                    CheckAllRunningProcesses();
                    IsRunning = true;
                }
                else
                {
                    Settings.Log?.Fatal($"{Settings.ProductName} must be executed with admin priviledges (run as admin)!");
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex.Message, ex);
            }
        }

        public void Deactivate()
        {
            Settings.Log?.Debug(nameof(Deactivate));
            try
            {
                IsRunning = false;
                _ProcessStartEvent.EventArrived -= new EventArrivedEventHandler(ProcessStartEventArrived);
                _ProcessStartEvent.Dispose();
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex.Message, ex);
            }
        }

        private void ProcessStartEventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                int processID = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                KillIfEmotetProcess(processID);
            }
            catch (Exception ex)
            {
                // if it's not possible to get the processid, so it's not possible to kill such a process
                Settings.Log?.Debug($"Error getting ProcessId: {ex.Message}");
            }
        }

        private void CheckAllRunningProcesses()
        {
            Settings.Log?.Debug("Scanning running processes ...");
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                KillIfEmotetProcess(process);
            }
            Settings.Log?.Debug("... done!");
        }

        private void KillIfEmotetProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                KillIfEmotetProcess(process);
            }
            catch (Exception ex)
            {
                // if it's not possible to get the process by id, so it's not possible to kill such a process
                Settings.Log?.Debug($"Could not get informations about process with id {processId}: {ex.Message}");
            }
        }

        private void KillIfEmotetProcess(Process process)
        {
            Task.Run(() =>
            {
                try
                {
                    var pi = new Entity.ProcessInfo(process.Id, process.ProcessName, GetProcessUserName(process), process.MainModule?.FileName, process.StartTime);
                    try
                    {
                        if (_EmoCheck_JPCERT.EmoProcessNameMatches(pi.ProcessName)
                            || _EmoCheck_SOPHOS.EmoProcessNameMatches(pi.ProcessName)
                            || _EmoCheck_ZiMADE.EmoProcessNameMatches(pi.ProcessName))
                        {
                            try
                            {
                                if (pi.ProcessId != Settings.CurrentProcessId)
                                {
                                    process.Kill();
                                    pi.KilledTime = DateTime.Now;
                                    pi.EventId = 1000;
                                    pi.Message = $"KILLED: Emotet-Process with Id {pi.ProcessId} successful killed!";
                                }
                            }
                            catch 
                            {
                                pi.KilledTime = DateTime.Now;
                                pi.EventId = 1001;
                                pi.Message = $"FINISHED: Emotet-Process with Id {pi.ProcessId} was finished bevore killing!";
                            }
                            finally
                            {
                                pi.SaveToJsonFile(); 
                                var msg = new StringBuilder();
                                msg.AppendLine(pi.Message);
                                msg.AppendLine($"ProcessId:\t{pi.ProcessId}");
                                msg.AppendLine($"ProcessName:\t{pi.ProcessName}");
                                msg.AppendLine($"FileName:\t{pi.ProcessFileName}");
                                msg.AppendLine($"StartTime:\t{pi.StartTime.ToShortDateString()} {pi.StartTime.ToLongTimeString()}");
                                msg.AppendLine($"DetectedTime:\t{pi.DetectedTime.ToShortDateString()} {pi.DetectedTime.ToLongTimeString()} ({pi.DetectionTimeMS} ms after start of process)");
                                msg.AppendLine($"KilledTime:\t{pi.KilledTime.ToShortDateString()} {pi.KilledTime.ToLongTimeString()} ({pi.KillingTimeMS} ms after detection of process)");
                                Settings.Log?.Warn(msg.ToString(), pi.EventId);
                            }
                        }
                        else
                        {
                            Settings.Log?.Debug($"CLEAN:\t{pi.ProcessId }\t{pi.ProcessName}\t{pi.ProcessFileName}");
                        }
                        //SaveToJsonFile(_EmoCheck.EmoProcessNameDictionary);
                    }
                    catch (Exception ex)
                    {
                        // if it's not possible to get more informations of the process, so it's also not possible to check such a process
                        Settings.Log?.Debug($"Error while checking process with id {pi.ProcessId} ({pi.ProcessName}): {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // if it's not possible to get more informations about this process 
                    Settings.Log?.Debug(ex);
                }
            });
        }

        private string GetProcessUserName(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                NativeMethods.OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new WindowsIdentity(processHandle);
                return wi.Name;
            }
            catch (Exception ex)
            {
                Settings.Log.Debug($"Error getting process username: {ex.Message}", ex);
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(processHandle);
                }
            }
            return string.Empty;
        }

        private void SaveToJsonFile(Dictionary<string, Entity.CheckInfo> emoProcessNameDict)
        {
            try
            {
                foreach (var entry in emoProcessNameDict)
                {
                    entry.Value.SaveToJsonFile();
                }
            }
            catch (Exception ex)
            {
                Settings.Log?.Error(ex);
            }
        }


        ~ProcessWatcher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // so that Dispose(false) isn't called later
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose all owned managed objects
                if (_ProcessStartEvent != null)
                {
                    _ProcessStartEvent.Dispose();
                    _ProcessStartEvent = null;
                }
            }

            // Release unmanaged resources
        }
    }
}
