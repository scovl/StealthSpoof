using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Linq;

namespace StealthSpoof.Core.Utils
{
    /// <summary>
    /// Utility class for diagnostics and displaying system information
    /// </summary>
    public static class DiagnosticsHelper
    {
        /// <summary>
        /// Checks the registry keys used by the program
        /// </summary>
        public static void CheckRegistryKeys()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Checking registry keys");
            Console.WriteLine("\n=== Registry Key Check ===");
            
            try
            {
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_GPU, "GPU Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_NETWORK_ADAPTERS, "Network Adapters Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_WINDOWS_NT, "Windows NT Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_COMPUTER_NAME, "Computer Name Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_ACTIVE_COMPUTER_NAME, "Active Computer Name Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_TCPIP_PARAMETERS, "TCP/IP Parameters Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_SCSI, "SCSI Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_HARDWARE_PROFILES, "Hardware Profiles Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_CRYPTOGRAPHY, "Cryptography Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_SQM_CLIENT, "SQM Client Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_SYSTEM_INFO, "System Information Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_WINDOWS_UPDATE, "Windows Update Registry");
                RegistryHelper.DisplayRegistryKeyStatus(RegistryHelper.REG_PATH_BIOS, "BIOS Registry");
                
                Logger.Instance.Info("Registry key check completed");
                Console.WriteLine("\nRegistry key check completed.");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error checking registry keys");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error checking registry keys: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Displays detailed system information
        /// </summary>
        public static void DisplaySystemData()
        {
            Logger.Instance.Info("Displaying system data");
            Console.WriteLine("\n=== Dados do Sistema ===");
            
            try
            {
                // Displays operating system information
                Console.WriteLine("\n--- Operating System ---");
                Console.WriteLine($"SO: {Environment.OSVersion}");
                Console.WriteLine($"SO 64-bit: {Environment.Is64BitOperatingSystem}");
                Console.WriteLine($"Machine Name: {Environment.MachineName}");
                Console.WriteLine($"User Name: {Environment.UserName}");
                
                // Displays CPU information
                Console.WriteLine("\n--- CPU Information ---");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Name: {obj["Name"]}");
                        Console.WriteLine($"Manufacturer: {obj["Manufacturer"]}");
                        Console.WriteLine($"Processor ID: {obj["ProcessorId"]}");
                        Console.WriteLine($"Cores: {obj["NumberOfCores"]}");
                        Console.WriteLine($"Logical Processors: {obj["NumberOfLogicalProcessors"]}");
                        Console.WriteLine($"Max Clock Speed: {obj["MaxClockSpeed"]} MHz");
                    }
                }
                
                // Displays motherboard information
                Console.WriteLine("\n--- Motherboard Information ---");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Manufacturer: {obj["Manufacturer"]}");
                        Console.WriteLine($"Product: {obj["Product"]}");
                        Console.WriteLine($"Serial Number: {obj["SerialNumber"]}");
                    }
                }
                
                // Displays BIOS information
                Console.WriteLine("\n--- BIOS Information ---");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Manufacturer: {obj["Manufacturer"]}");
                        Console.WriteLine($"Version: {obj["Version"]}");
                        Console.WriteLine($"Serial Number: {obj["SerialNumber"]}");
                        Console.WriteLine($"Release Date: {obj["ReleaseDate"]}");
                    }
                }
                
                // Displays disk information
                Console.WriteLine("\n--- Disk Information ---");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Model: {obj["Model"]}");
                        Console.WriteLine($"Serial Number: {obj["SerialNumber"]}");
                        Console.WriteLine($"Interface Type: {obj["InterfaceType"]}");
                        Console.WriteLine($"Size: {Convert.ToDouble(obj["Size"]) / (1024 * 1024 * 1024):F2} GB");
                        Console.WriteLine("---");
                    }
                }
                
                // Displays network adapter information
                Console.WriteLine("\n--- Network Adapters ---");
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                {
                    Console.WriteLine($"Name: {nic.Name}");
                    Console.WriteLine($"Description: {nic.Description}");
                    Console.WriteLine($"MAC Address: {BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":")}");
                    Console.WriteLine($"Status: {nic.OperationalStatus}");
                    Console.WriteLine("---");
                }
                
                Logger.Instance.Info("System data display completed");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error displaying system data");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error displaying system data: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
} 