/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    public class ProcessWatcher
    {
        private ManagementEventWatcher _ProcessStartEvent;
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
                if (Settings.IsAdministrator())
                {
                    _ProcessStartEvent = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStartTrace");
                    _ProcessStartEvent.EventArrived += new EventArrivedEventHandler(ProcessStartEventArrived);
                    _ProcessStartEvent.Start();

                    Settings.Log?.Debug("Scanning running processes ...");
                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        KillIfEmotetProcess(process);
                    }
                    Settings.Log?.Debug("... done!");
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
                    var pi = new ProcessInfo(process.Id, process.ProcessName, process.MainModule?.FileName, process.StartTime);
                    try
                    {
                        var emoCheck = new EmoCheck();
                        if (emoCheck.EmoProcessNames1.Match(pi.ProcessName)?.Success == true ||
                            emoCheck.EmoProcessNames2?.Match(pi.ProcessName)?.Success == true)
                        {
                            try
                            {
                                if (pi.ProcessId != Process.GetCurrentProcess().Id)
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
                                var msg = new StringBuilder();
                                msg.AppendLine(pi.Message);
                                msg.AppendLine($"ProcessId:\t{pi.ProcessId}");
                                msg.AppendLine($"ProcessName:\t{pi.ProcessName}");
                                msg.AppendLine($"FileName:\t{pi.FileName}");
                                msg.AppendLine($"StartTime:\t{pi.StartTime.ToShortDateString()} {pi.StartTime.ToLongTimeString()}");
                                msg.AppendLine($"DetectedTime:\t{pi.DetectedTime.ToShortDateString()} {pi.DetectedTime.ToLongTimeString()} ({pi.DetectionPeriod} ms after start of process)");
                                msg.AppendLine($"KilledTime:\t{pi.KilledTime.ToShortDateString()} {pi.KilledTime.ToLongTimeString()} ({pi.KillingPeriod} ms after detection of process)");
                                Settings.Log?.Warn(msg.ToString(), pi.EventId);
                            }
                        }
                        else
                        {
                            Settings.Log?.Debug($"CLEAN:\t{pi.ProcessId }\t{pi.ProcessName}\t{pi.FileName}");
                        }
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
    }
}
