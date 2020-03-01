/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    internal static class InfoDispatcher
    {
        private static System.Threading.Timer _Timer;
        private static int _CheckIntervalMs { get; set; }

        private static bool _IsBusy = false;
        private static readonly int _MaxDispatchCount = 10;
        private static Dictionary<string, int> _DispatchCounter = new Dictionary<string, int>();

        internal static void Acitvate()
        {
            try
            {
                if (!DirectoryExists(Settings.Config.SharedDataFolder))
                {
                    Settings.Log.Warn($"Directory does not exist or is not accessible: {Settings.Config.SharedDataFolder}");
                }
                var checkIntervalSec = 60 * 5; // start dispatching every 5 minutes if there is something to dispatch
                _CheckIntervalMs = 1000 * checkIntervalSec;
                // Timer erstellen und das erste Mal ohne Verzögerung ausführen
                _Timer = new System.Threading.Timer(StartDispatching, null, 0, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Settings.Log.Error(ex);
            }
        }
        internal static void Deacitvate()
        {
            try
            {
                if (_Timer != null)
                {
                    _Timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _Timer.Dispose();
                }
            }
            catch (Exception ex)
            {
                Settings.Log.Error(ex);
            }
            finally
            {
                _Timer = null;
            }
        }

        private static void StartDispatching(Object state)
        {
            Settings.Log.Debug($"{nameof(StartDispatching)}");
            try
            {
                _Timer.Change(Timeout.Infinite, Timeout.Infinite);
                SendInformationToShare();
            }
            catch (Exception ex)
            {
                Settings.Log.Error(ex);
            }
            finally
            {
                _Timer?.Change(_CheckIntervalMs, Timeout.Infinite);
            }
        }

        internal static void SendInformationToShare()
        {
            if (_IsBusy) return;
            _IsBusy = true;
            try
            {
                var jsonFiles = Directory.GetFiles(Settings.DataFolder, "*.json");
                if (jsonFiles.Count() > 0 && DirectoryExists(Settings.Config.SharedDataFolder))
                {
                    var maxLoops = 10;
                    var loopCount = 0;
                    do
                    {
                        if (jsonFiles.Count() > 0)
                        {
                            foreach (var jsonFile in jsonFiles)
                            {
                                ProcessJsonFiles();
                            }
                        }
                        // have a look if there are new files to dispatch
                        jsonFiles = Directory.GetFiles(Settings.DataFolder, "*.json");
                    }
                    while (jsonFiles.Count() > 0 && ++loopCount < maxLoops);
                    if (loopCount >= maxLoops)
                    {
                        Settings.Log.Debug($"Long running dispatch loop!!! Current json-filecount: {jsonFiles.Count()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _IsBusy = false;
            }
        }

        private static void ProcessJsonFiles()
        {
            var dataFolder = Settings.DataFolder;
            var sharedFolder = Settings.Config.SharedDataFolder;
            var errorFolder = Path.Combine(Settings.DataFolder, "Error");
            var jsonFiles = Directory.GetFiles(dataFolder, "*.json");
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    if (!_DispatchCounter.ContainsKey(jsonFile))
                    {
                        _DispatchCounter.Add(jsonFile, 0);
                    }
                    ++_DispatchCounter[jsonFile];
                    if (_DispatchCounter[jsonFile] > _MaxDispatchCount)
                    {
                        try
                        {
                            var errorFile = jsonFile.Replace(dataFolder, errorFolder);
                            if (FileExists(errorFile))
                            {
                                File.Delete(errorFile);
                            }
                            MoveFile(jsonFile, errorFile);
                            _DispatchCounter.Remove(jsonFile);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        var sharedFile = jsonFile.Replace(dataFolder, sharedFolder);
                        MoveFile(jsonFile, sharedFile);
                    }
                }
                catch (Exception ex)
                {
                    Settings.Log.Debug($"File {jsonFile} may be locked, try it later again", ex);
                }
            }
        }

        private static void CopyFile(string sourceFile, string targetFolder)
        {
            var targetFile = Path.Combine(targetFolder, Path.GetFileName(sourceFile));
            if (!DirectoryExists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            // if there is an older version of the file with the same filename, it will be delete first
            if (FileExists(targetFile))
            {
                File.Delete(targetFile);
            }
            File.Copy(sourceFile, targetFile);
        }

        private static void MoveFile(string sourceFile, string targetFile)
        {
            var targetFolder = Path.GetDirectoryName(targetFile);
            if (!DirectoryExists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            // if there is an older version of the file with the same filename, it will be delete first
            if (FileExists(targetFile))
            {
                File.Delete(targetFile);
            }
            File.Move(sourceFile, targetFile);
        }

        private static bool DirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var task = new Task<bool>(() => { var fi = new DirectoryInfo(path); return fi.Exists; });
            task.Start();
            return task.Wait(100) && task.Result;
        }

        private static bool FileExists(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;
            var task = new Task<bool>(() => { var fi = new FileInfo(filename); return fi.Exists; });
            task.Start();
            return task.Wait(100) && task.Result;
        }
    }
}
