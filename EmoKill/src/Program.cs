/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.ServiceProcess;

namespace ZiMADE.EmoKill
{
    public static class Program
    {
        private static ProcessWatcher _ProcessWatcher;
        public static bool IsRunning => _ProcessWatcher?.IsRunning == true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WinService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        public static void Start()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledExceptionHandler;
                Settings.InitalizeLogger();
                _ProcessWatcher = new ProcessWatcher();
            }
            catch (Exception ex)
            {
                Settings.Log?.Fatal(ex);
            }
        }

        public static void Stop()
        {
            try
            {
                _ProcessWatcher?.Deactivate();
                AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledExceptionHandler;
            }
            catch (Exception ex)
            {
                Settings.Log?.Fatal(ex);
            }
        }

        private static void AppDomainUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (args == null)
            {
                Settings.Log?.Fatal("AppDomainUnhandledException: args = null");
            }
            else if (args.ExceptionObject is Exception)
            {
                Exception ex = (Exception)args.ExceptionObject;
                Settings.Log?.Fatal(ex);
                Settings.Log?.Fatal(string.Format("AppDomainUnhandledException: Runtime terminating = {0}, TargetSite = {1}", args.IsTerminating, ex.TargetSite));
            }
            else
            {
                Settings.Log?.Fatal(string.Format("AppDomainUnhandledException: Runtime terminating = {0}, ExceptionObject = {1}", args.IsTerminating, args.ExceptionObject.ToString()));
            }
            ServiceHelper.RestartService();
        }
    }
}
