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
        static void Main(string[] args)
        {
            EmoKill.Settings.InitalizeLogger();
            HandleUserInput();
        }

        static private void HandleUserInput()
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
                        UninstallEmoKillService();
                        break;
                    case 3:
                        StartEmoKillService();
                        break;
                    case 4:
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
                        ActivateEmoKillConsole();
                        break;
                }
            } while (userInput != 9);
        }

        static private int DisplayMenu()
        {
            ServiceControllerStatus? serviceState = EmoKill.ServiceHelper.GetServiceState();
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
            System.Console.WriteLine($"Release Date:\t{EmoKill.Settings.ProductDate}");
            System.Console.WriteLine($"URL:\t\t{EmoKill.Settings.ProductRepository}");
            if (serviceState == null)
                System.Console.WriteLine($"Service State:\tUNKNOWN");
            else
                System.Console.WriteLine($"Service State:\t{serviceState}");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("EmoKill-Console Menu");
            System.Console.WriteLine(" 1. Install and start EmoKill as Service");
            System.Console.WriteLine(" 2. Uninstall EmoKill Service");
            System.Console.WriteLine(" 3. Start EmoKill Service");
            System.Console.WriteLine(" 4. Stop EmoKill Service");
            System.Console.WriteLine(" 5. Get status of EmoKill Service");
            System.Console.WriteLine(" 6. Show EmoKill Logfile");
            System.Console.WriteLine(" 7. Delete EmoKill Logfile");
            System.Console.WriteLine(" 8. Activate EmoKill in this Console");
            System.Console.WriteLine(" 9. Exit");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.Write("Please choose what you wish to do: ");
            var result = System.Console.ReadLine();
            return Convert.ToInt32(result);
        }

        static private void ActivateEmoKillConsole()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Activate EmoKill in this Console ...");
            System.Console.WriteLine("_________________________________________");
            EmoKill.Program.Start();

            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            if (EmoKill.Program.IsRunning)
            {
                System.Console.WriteLine("EmoKill activated.");
                System.Console.WriteLine("Press <ENTER> to deactivate EmoKill");
                System.Console.WriteLine("_________________________________________");
                System.Console.ReadLine();
                EmoKill.Program.Stop();
                System.Console.WriteLine("_________________________________________");
                System.Console.WriteLine();
            }
            else
            {
                System.Console.WriteLine(">ERROR<: EmoKill could not be activated!");
                PressEnterToContinue();
            }
        }

        static private void DeactivateEmoKillConsole()
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

        static private void InstallEmoKillService()
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

        static private void UninstallEmoKillService()
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

        static private void StartEmoKillService()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Starting EmoKill Service ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            ServiceControllerStatus? result = EmoKill.ServiceHelper.StartService();
            if (result == null)
                System.Console.WriteLine($"Service State: UNKNOWN");
            else
                System.Console.WriteLine($"Service State: {result}");
            PressEnterToContinue();
        }

        static private void StopEmoKillService()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Stopping EmoKill Service ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            ServiceControllerStatus? result = EmoKill.ServiceHelper.StopService();
            if (result == null)
                System.Console.WriteLine($"Service State: UNKNOWN");
            else
                System.Console.WriteLine($"Service State: {result}");
            PressEnterToContinue();
        }

        static private void GetEmoKillServiceStatus()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Getting EmoKill Service Status ...");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            ServiceControllerStatus? result = EmoKill.ServiceHelper.GetServiceState();
            if (result == null)
                System.Console.WriteLine($"Service State: UNKNOWN");
            else
                System.Console.WriteLine($"Service State: {result}");
            PressEnterToContinue();
        }

        static private void ShowEmoKillLogfile()
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

        static private void DeleteEmoKillLogfile()
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

        static private void PressEnterToContinue()
        {
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("Press <ENTER> to continue");
            System.Console.WriteLine("_________________________________________");
            System.Console.ReadLine();
        }
    }
}
