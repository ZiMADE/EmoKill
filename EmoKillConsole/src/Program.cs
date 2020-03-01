/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.ServiceProcess;

namespace ZiMADE.EmoKillConsole
{
    class Program
    {
        private static ServiceControllerStatus? _ServiceState;

        static void Main(string[] args)
        {
            EmoKill.Settings.Initialize();
            HandleUserInput();
        }

        private static void HandleUserInput()
        {
            int userInput = 0;
            do
            {
                userInput = DisplayMenu();
                switch (userInput)
                {
                    case 1:
                        InstallEmoKillService();
                        break;
                    case 2:
                        if (_ServiceState != null)
                            UninstallEmoKillService();
                        break;
                    case 3:
                        if (_ServiceState == ServiceControllerStatus.Stopped)
                            StartEmoKillService();
                        break;
                    case 4:
                        if (_ServiceState == ServiceControllerStatus.Running)
                            StopEmoKillService();
                        break;
                    case 5:
                        GetEmoKillServiceStatus();
                        break;
                    case 6:
                        ShowEmoKillLogfile();
                        break;
                    case 7:
                        DeleteEmoKillLogfile();
                        break;
                    case 8:
                        if (_ServiceState == null || _ServiceState == ServiceControllerStatus.Stopped)
                            ActivateEmoKillConsole();
                        break;
                }
            } while (userInput != 0);
        }

        private static int DisplayMenu()
        {
            _ServiceState = EmoKill.ServiceHelper.GetServiceState();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine(@" ______                 _   __ _   _   _ ");
            System.Console.WriteLine(@"|  ____|               | | / /|_| | | | |");
            System.Console.WriteLine(@"| |__   _ __ ___   ___ | |/ /  _  | | | |");
            System.Console.WriteLine(@"|  __| | '_ ` _ ` / _ `|   (  | | | | | |");
            System.Console.WriteLine(@"| |____| | | | | | (_) | |\ \ | | | | | |");
            System.Console.WriteLine(@"|______|_| |_| |_|`___/|_| \_\|_| |_| |_|");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Emotet process killing tool by ZiMADE.");
            System.Console.WriteLine();
            System.Console.WriteLine($"Version:\t{EmoKill.Settings.ProductVersion}");
            System.Console.WriteLine($"Release Date:\t{EmoKill.Settings.ProductDate.ToShortDateString()}");
            System.Console.WriteLine($"URL:\t\t{EmoKill.Settings.ProductRepository}");
            if (_ServiceState == null)
                System.Console.WriteLine($"Service State:\tUNKNOWN");
            else
                System.Console.WriteLine($"Service State:\t{_ServiceState}");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("EmoKill-Console Menu");
            System.Console.WriteLine(" 1. Install/Update and start EmoKill as Service");
            if (_ServiceState != null)
                System.Console.WriteLine(" 2. Uninstall EmoKill Service");
            if (_ServiceState == ServiceControllerStatus.Stopped)
                System.Console.WriteLine(" 3. Start EmoKill Service");
            if (_ServiceState == ServiceControllerStatus.Running)
                System.Console.WriteLine(" 4. Stop EmoKill Service");
            System.Console.WriteLine(" 5. Get status of EmoKill Service");
            System.Console.WriteLine(" 6. Show EmoKill Logfile");
            System.Console.WriteLine(" 7. Delete EmoKill Logfile");
            if (_ServiceState == null || _ServiceState == ServiceControllerStatus.Stopped)
                System.Console.WriteLine(" 8. Activate EmoKill in this Console (just for testing)");
            System.Console.WriteLine(" 0. Exit");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.Write("Please choose what you wish to do: ");
            var result = System.Console.ReadLine();
            try
            {
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static void ActivateEmoKillConsole()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Activate EmoKill in this Console ...");
            System.Console.WriteLine("_________________________________________");
            EmoKill.Runner.Start();

            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            if (EmoKill.Runner.IsRunning)
            {
                System.Console.WriteLine("EmoKill activated.");
                System.Console.WriteLine("Press <ENTER> to deactivate EmoKill");
                System.Console.WriteLine("_________________________________________");
                System.Console.ReadLine();
                EmoKill.Runner.Stop();
                System.Console.WriteLine("_________________________________________");
                System.Console.WriteLine();
            }
            else
            {
                System.Console.WriteLine(">ERROR<: EmoKill could not be activated!");
                PressEnterToContinue();
            }
        }

        private static void DeactivateEmoKillConsole()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Deactivate EmoKill ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            EmoKill.Program.Stop();
            PressEnterToContinue();
        }

        private static void InstallEmoKillService()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Installing EmoKill Service ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            var result = EmoKill.ServiceHelper.InstallService();
            System.Console.WriteLine("Result:");
            System.Console.WriteLine(result);
            PressEnterToContinue();
        }

        private static void UninstallEmoKillService()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Uninstalling EmoKill Service ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            var result = EmoKill.ServiceHelper.UninstallService();
            System.Console.WriteLine("Result:");
            System.Console.WriteLine(result);
            PressEnterToContinue();
        }

        private static void StartEmoKillService()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Starting EmoKill Service ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            ServiceControllerStatus? result = EmoKill.ServiceHelper.StartService();
            System.Console.WriteLine(string.Concat("Service State: ", (result == null) ? "UNKNOWN" : $"{result}"));
            PressEnterToContinue();
        }

        private static void StopEmoKillService()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Stopping EmoKill Service ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            ServiceControllerStatus? result = EmoKill.ServiceHelper.StopService();
            System.Console.WriteLine(string.Concat("Service State: ", (result == null) ? "UNKNOWN" : $"{result}"));
            PressEnterToContinue();
        }

        private static void GetEmoKillServiceStatus()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Getting EmoKill Service Status ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine(EmoKill.ServiceHelper.GetSystemInfo());
            PressEnterToContinue();
        }

        private static void ShowEmoKillLogfile()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Opening EmoKill LogFile ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            EmoKill.ServiceHelper.ShowEmoKillLogfile();
            PressEnterToContinue();
        }

        private static void DeleteEmoKillLogfile()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Deleting EmoKill LogFile ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            EmoKill.ServiceHelper.DeleteEmoKillLogfile();
            PressEnterToContinue();
        }

        private static void PressEnterToContinue()
        {
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Press <ENTER> to continue");
            System.Console.WriteLine("_________________________________________");
            System.Console.ReadLine();
        }
    }
}
