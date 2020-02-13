/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Reflection;

namespace EmoKillTest
{
    class Program
    {
        public static string ProductName => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute), false))?.Product;
        public static string ProductVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        public static string ProductDate = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute), false))?.InformationalVersion;
        public static string ProductRepository = ((AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyConfigurationAttribute), false))?.Configuration;

        static void Main(string[] args)
        {
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("This program just waits to be killed by:");
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
            System.Console.WriteLine($"Version:\t{ProductVersion}");
            System.Console.WriteLine($"Release Date:\t{ProductDate}");
            System.Console.WriteLine($"URL:\t\t{ProductRepository}");
            System.Console.WriteLine("_________________________________________");
            System.Console.WriteLine();
            System.Console.WriteLine("If you run EmoKill, this program should be");
            System.Console.WriteLine("killed immediatly.");
            System.Console.WriteLine("Or if you start EmoKillTest while EmoKill");
            System.Console.WriteLine("is running, EmoKillTest should not run for");
            System.Console.WriteLine("a long time, it should be killed as soon as");
            System.Console.WriteLine("possible.");
            System.Console.WriteLine("_________________________________________");
            while (true)
            {
                System.Console.ReadLine();
            }
        }
    }
}
