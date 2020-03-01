/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System.ServiceProcess;

namespace ZiMADE.EmoKill
{
    public static class Program
    {
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

        /// <summary>
        /// Start EmoKill Service
        /// Plugins are loaded if the first plugin will be used
        /// </summary>
        public static void Start()
        {
            Runner.Start();
        }

        /// <summary>
        /// Stop EmoKill Service
        /// and unload all loaded plugins
        /// </summary>
        public static void Stop()
        {
            Runner.Stop();
        }

    }
}
