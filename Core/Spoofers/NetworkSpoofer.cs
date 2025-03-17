using System;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Spoofers
{
    /// <summary>
    /// Class responsible for network adapter spoofing
    /// </summary>
    public static class NetworkSpoofer
    {
        private static readonly string CMD_NETSH = "netsh";
        
        /// <summary>
        /// Performs spoofing of MAC addresses of network adapters
        /// </summary>
        public static void SpoofMAC()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing MAC addresses...");
            
            try
            {
                var adapters = GetPhysicalNetworkAdapters();
                
                if (adapters.Count == 0)
                {
                    Console.WriteLine("No network adapters found.");
                    return;
                }
                
                DisplayAvailableAdapters(adapters);
                ProcessAdapterSelection(adapters);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing MAC addresses");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing MAC addresses: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Gets a list of physical network adapters
        /// </summary>
        private static List<string> GetPhysicalNetworkAdapters()
        {
            var adapters = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=True"))
            {
                foreach (var obj in searcher.Get())
                {
                    string? adapterName = obj["Name"]?.ToString();
                    string? deviceId = obj["DeviceID"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(adapterName) && !string.IsNullOrEmpty(deviceId))
                    {
                        adapters.Add($"{deviceId}: {adapterName}");
                    }
                }
            }
            return adapters;
        }
        
        /// <summary>
        /// Displays the list of available adapters
        /// </summary>
        private static void DisplayAvailableAdapters(List<string> adapters)
        {
            Console.WriteLine("\nAvailable adapters:");
            for (int i = 0; i < adapters.Count; i++)
            {
                Console.WriteLine($"{i+1}. {adapters[i]}");
            }
        }
        
        /// <summary>
        /// Processes the adapter selection from user input
        /// </summary>
        private static void ProcessAdapterSelection(List<string> adapters)
        {
            Console.Write("\nChoose the adapter to spoof (0 for all): ");
            string? input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int selection))
            {
                return;
            }
            
            if (selection == 0)
            {
                SpoofAllAdapters(adapters);
            }
            else if (selection > 0 && selection <= adapters.Count)
            {
                string deviceId = adapters[selection - 1].Split(':')[0];
                SpoofSingleMAC(deviceId);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid selection!");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Spoofs all network adapters in the list
        /// </summary>
        private static void SpoofAllAdapters(List<string> adapters)
        {
            foreach (var adapter in adapters)
            {
                string deviceId = adapter.Split(':')[0];
                SpoofSingleMAC(deviceId);
            }
        }
        
        /// <summary>
        /// Performs spoofing of a single network adapter
        /// </summary>
        /// <param name="deviceId">Network device ID</param>
        public static void SpoofSingleMAC(string deviceId)
        {
            try
            {
                string newMAC = HardwareInfo.GetRandomMACAddress().Replace(":", "");
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{deviceId}"))
                {
                    if (key != null)
                    {
                        key.SetValue("NetworkAddress", newMAC, RegistryValueKind.String);
                        
                        DisableNetworkAdapter(deviceId);
                        Thread.Sleep(1000);
                        EnableNetworkAdapter(deviceId);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"MAC of adapter {deviceId} modified to: {newMAC}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing MAC of adapter {deviceId}: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Disables a network adapter
        /// </summary>
        /// <param name="deviceId">Network device ID</param>
        public static void DisableNetworkAdapter(string deviceId)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = CMD_NETSH;
                process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" disabled";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
        }
        
        /// <summary>
        /// Activates a network adapter
        /// </summary>
        /// <param name="deviceId">Network device ID</param>
        public static void EnableNetworkAdapter(string deviceId)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = CMD_NETSH;
                process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" enabled";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
        }
    }
} 