using System;
using StealthSpoof.Core;

namespace StealthSpoof
{
    class Program
    {
        static void Main(string[] args)
        {
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
                string input = Console.ReadLine();
                
                switch (input)
                {
                    case "0":
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option!");
                        Console.ResetColor();
                        break;
                }
            }
        }
    }
} 