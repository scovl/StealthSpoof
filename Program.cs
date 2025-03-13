using System;
using StealthSpoof.Core;

/*
    This is the main class of the program.
    It is responsible for displaying the menu and handling the user's input.
*/

namespace StealthSpoof
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Initialize logger at the start
            Logger.Instance.Info("StealthSpoof application started");
            
            // Check log file size
            Logger.Instance.CheckLogFileSize();
            
            // Verificar sistema operacional
            if (!OperatingSystem.IsWindows())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This application only works on Windows!");
                Console.ResetColor();
                return;
            }
            
            // Verificar versão do Windows
            Version windowsVersion = Environment.OSVersion.Version;
            if (windowsVersion.Major < 10)
            {
                Logger.Instance.Warning($"Unsupported Windows version: {windowsVersion}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: This application is designed for Windows 10 and later.");
                Console.WriteLine("Some features may not work correctly on your system.");
                Console.ResetColor();
            }
            
            Console.Title = "StealthSpoof - HWID Spoofer";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("        StealthSpoof HWID Spoofer v1.0           ");
            Console.WriteLine("=================================================");
            Console.ResetColor();
            
            while (true)
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1. Show current hardware information");
                Console.WriteLine("2. Spoof all hardware");
                Console.WriteLine("3. Spoof CPU");
                Console.WriteLine("4. Spoof Disks");
                Console.WriteLine("5. Spoof Motherboard");
                Console.WriteLine("6. Spoof GPU");
                Console.WriteLine("7. Spoof MAC Address");
                Console.WriteLine("8. Restore original settings");
                Console.WriteLine("0. Exit");
                
                Console.Write("\nOption: ");
                string? input = Console.ReadLine();
                
                Logger.Instance.Info($"User selected option: {input}");
                
                switch (input)
                {
                    case "0":
                        Logger.Instance.Info("Application exit requested");
                        return;
                    case "1":
                        HardwareInfo.ShowCurrentHardwareInfo();
                        break;
                    case "2":
                        HardwareSpoofer.SpoofAllHardware();
                        break;
                    case "3":
                        HardwareSpoofer.SpoofCPU();
                        break;
                    case "4":
                        HardwareSpoofer.SpoofDisk();
                        break;
                    case "5":
                        HardwareSpoofer.SpoofMotherboard();
                        break;
                    case "6":
                        HardwareSpoofer.SpoofGPU();
                        break;
                    case "7":
                        HardwareSpoofer.SpoofMAC();
                        break;
                    case "8":
                        HardwareSpoofer.RestoreOriginal();
                        break;
                    default:
                        Logger.Instance.Warning($"Invalid option entered: {input}");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option!");
                        Console.ResetColor();
                        break;
                }
            }
        }
    }
} 