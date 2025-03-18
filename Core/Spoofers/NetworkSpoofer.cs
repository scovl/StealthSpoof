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
            if (!CheckAdminPrivileges()) return;
            
            Console.WriteLine("\nSpoofing MAC addresses...");
            Logger.Instance.Info("Starting MAC address spoofing");
            
            try
            {
                var adapters = GetPhysicalNetworkAdapters();
                
                if (adapters.Count == 0)
                {
                    HandleNoAdaptersFound();
                    return;
                }
                
                DisplayAvailableAdapters(adapters);
                ProcessAdapterSelection(adapters);
            }
            catch (Exception ex)
            {
                HandleMacSpoofingException(ex);
            }
        }
        
        /// <summary>
        /// Handles the case when no network adapters are found
        /// </summary>
        private static void HandleNoAdaptersFound()
        {
            Logger.Instance.Warning("No network adapters found for MAC spoofing");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No network adapters found.");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Handles exceptions during MAC spoofing process
        /// </summary>
        private static void HandleMacSpoofingException(Exception ex)
        {
            Logger.Instance.LogException(ex, "Error spoofing MAC addresses");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error spoofing MAC addresses: {ex.Message}");
            Console.ResetColor();
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
            if (string.IsNullOrEmpty(deviceId))
            {
                HandleInvalidDeviceId();
                return;
            }
            
            Logger.Instance.Info($"Spoofing MAC address for adapter {deviceId}");
            
            try
            {
                string newMAC = HardwareInfo.GetRandomMACAddress().Replace(":", "");
                Logger.Instance.Debug($"Generated new MAC: {newMAC} for device {deviceId}");
                
                if (!ModifyRegistryMAC(deviceId, newMAC))
                {
                    return;
                }
                
                bool success = ResetNetworkAdapter(deviceId);
                DisplayMacSpoofResult(deviceId, newMAC, success);
            }
            catch (UnauthorizedAccessException ex)
            {
                HandleUnauthorizedAccess(ex, deviceId);
            }
            catch (Exception ex)
            {
                HandleMacSpoofException(ex, deviceId);
            }
        }
        
        /// <summary>
        /// Modifies the MAC address in the registry
        /// </summary>
        private static bool ModifyRegistryMAC(string deviceId, string newMAC)
        {
            using RegistryKey? key = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{deviceId}");
            if (key == null)
            {
                Logger.Instance.Error($"Failed to access registry key for adapter {deviceId}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Could not access registry for adapter {deviceId}");
                Console.ResetColor();
                return false;
            }
            
            // Store original MAC for logging
            string? originalMAC = key.GetValue("NetworkAddress") as string;
            Logger.Instance.Debug($"Original MAC for device {deviceId}: {originalMAC ?? "Not set"}");
            
            key.SetValue("NetworkAddress", newMAC, RegistryValueKind.String);
            Logger.Instance.Info($"MAC address set in registry for device {deviceId}");
            
            return true;
        }
        
        /// <summary>
        /// Resets the network adapter to apply changes
        /// </summary>
        private static bool ResetNetworkAdapter(string deviceId)
        {
            bool disableSuccess = DisableNetworkAdapter(deviceId);
            if (!disableSuccess)
            {
                Logger.Instance.Warning($"Failed to disable adapter {deviceId}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Could not disable adapter {deviceId}");
                Console.ResetColor();
            }
            
            Thread.Sleep(1000);
            
            bool enableSuccess = EnableNetworkAdapter(deviceId);
            if (!enableSuccess)
            {
                Logger.Instance.Warning($"Failed to enable adapter {deviceId}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Could not enable adapter {deviceId}. You may need to enable it manually.");
                Console.ResetColor();
            }
            
            return disableSuccess && enableSuccess;
        }
        
        /// <summary>
        /// Displays the result of MAC spoofing
        /// </summary>
        private static void DisplayMacSpoofResult(string deviceId, string newMAC, bool success)
        {
            if (success)
            {
                Logger.Instance.Info($"Successfully spoofed MAC for device {deviceId} to {newMAC}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"MAC of adapter {deviceId} modified to: {newMAC}");
                Console.ResetColor();
            }
            else
            {
                Logger.Instance.Info($"MAC address set in registry, but adapter reset failed. Changes may not be active until system restart.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"MAC address changed in registry, but could not apply immediately.");
                Console.WriteLine("Changes will take effect after system restart.");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Handles the case when the device ID is invalid
        /// </summary>
        private static void HandleInvalidDeviceId()
        {
            Logger.Instance.Warning("Cannot spoof MAC: Device ID is null or empty");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid device ID provided");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Handles unauthorized access exceptions
        /// </summary>
        private static void HandleUnauthorizedAccess(UnauthorizedAccessException ex, string deviceId)
        {
            Logger.Instance.LogException(ex, $"Access denied while spoofing MAC of adapter {deviceId}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Access denied: {ex.Message}");
            Console.WriteLine("Try running the application as administrator.");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Handles general exceptions during MAC spoofing
        /// </summary>
        private static void HandleMacSpoofException(Exception ex, string deviceId)
        {
            Logger.Instance.LogException(ex, $"Error spoofing MAC of adapter {deviceId}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error spoofing MAC of adapter {deviceId}: {ex.Message}");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Disables a network adapter
        /// </summary>
        /// <param name="deviceId">Network device ID</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public static bool DisableNetworkAdapter(string deviceId)
        {
            try
            {
                Logger.Instance.Debug($"Disabling network adapter {deviceId}");
                
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = CMD_NETSH;
                    process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" disabled";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    
                    process.Start();
                    
                    process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    process.WaitForExit();
                    
                    int exitCode = process.ExitCode;
                    
                    if (exitCode != 0)
                    {
                        Logger.Instance.Warning($"Failed to disable adapter {deviceId}. Exit code: {exitCode}, Error: {error}");
                        return false;
                    }
                    
                    Logger.Instance.Debug($"Successfully disabled adapter {deviceId}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error disabling network adapter {deviceId}");
                return false;
            }
        }
        
        /// <summary>
        /// Activates a network adapter
        /// </summary>
        /// <param name="deviceId">Network device ID</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public static bool EnableNetworkAdapter(string deviceId)
        {
            try
            {
                Logger.Instance.Debug($"Enabling network adapter {deviceId}");
                
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = CMD_NETSH;
                    process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" enabled";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    
                    process.Start();
                    
                    process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    process.WaitForExit();
                    
                    int exitCode = process.ExitCode;
                    
                    if (exitCode != 0)
                    {
                        Logger.Instance.Warning($"Failed to enable adapter {deviceId}. Exit code: {exitCode}, Error: {error}");
                        return false;
                    }
                    
                    Logger.Instance.Debug($"Successfully enabled adapter {deviceId}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error enabling network adapter {deviceId}");
                return false;
            }
        }

        /// <summary>
        /// Spoofs network configuration settings like hostname and workgroup
        /// </summary>
        public static void SpoofNetworkConfig()
        {
            if (!CheckAdminPrivileges()) return;
            
            Console.WriteLine("\nSpoofing Network Configuration...");
            Logger.Instance.Info("Starting Network Configuration spoofing");
            
            try
            {
                bool hostnameModified = SpoofHostname();
                bool workgroupModified = SpoofWorkgroup();
                
                DisplayNetworkConfigResults(hostnameModified, workgroupModified);
            }
            catch (Exception ex)
            {
                HandleNetworkConfigException(ex);
            }
        }
        
        /// <summary>
        /// Checks if the application has administrative privileges
        /// </summary>
        private static bool CheckAdminPrivileges()
        {
            if (!RegistryHelper.RequireAdminCheck())
            {
                Logger.Instance.Warning("Network config spoofing requires admin privileges");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Spoofs the system hostname
        /// </summary>
        private static bool SpoofHostname()
        {
            string currentHostname = Environment.MachineName;
            string newHostname = StringUtils.GenerateRandomHostname();
            
            Logger.Instance.Info($"Attempting to change hostname from {currentHostname} to {newHostname}");
            
            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName", true);
                if (key == null)
                {
                    Logger.Instance.Warning("Failed to open hostname registry key");
                    return false;
                }
                
                key.SetValue("ComputerName", newHostname, RegistryValueKind.String);
                
                // Also update Active Computer Name
                using RegistryKey? activeKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", true);
                if (activeKey != null)
                {
                    activeKey.SetValue("ComputerName", newHostname, RegistryValueKind.String);
                }
                
                // Update Hostname key
                using RegistryKey? hostnameKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true);
                if (hostnameKey != null)
                {
                    hostnameKey.SetValue("Hostname", newHostname, RegistryValueKind.String);
                    hostnameKey.SetValue("NV Hostname", newHostname, RegistryValueKind.String);
                }
                
                Logger.Instance.Info($"Hostname changed to {newHostname} in registry");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Failed to change hostname");
                return false;
            }
        }
        
        /// <summary>
        /// Spoofs the system workgroup
        /// </summary>
        private static bool SpoofWorkgroup()
        {
            string newWorkgroup = StringUtils.GenerateRandomWorkgroup();
            
            Logger.Instance.Info($"Attempting to change workgroup to {newWorkgroup}");
            
            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true);
                if (key == null)
                {
                    Logger.Instance.Warning("Failed to open workgroup registry key");
                    return false;
                }
                
                key.SetValue("Domain", newWorkgroup, RegistryValueKind.String);
                key.SetValue("NV Domain", newWorkgroup, RegistryValueKind.String);
                
                // Also update Lanman parameters
                using RegistryKey? lanmanKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", true);
                if (lanmanKey != null)
                {
                    lanmanKey.SetValue("Domain", newWorkgroup, RegistryValueKind.String);
                }
                
                Logger.Instance.Info($"Workgroup changed to {newWorkgroup} in registry");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Failed to change workgroup");
                return false;
            }
        }
        
        /// <summary>
        /// Displays the results of the network configuration spoofing
        /// </summary>
        private static void DisplayNetworkConfigResults(bool hostnameModified, bool workgroupModified)
        {
            if (hostnameModified && workgroupModified)
            {
                Logger.Instance.Info("Network configuration spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Network configuration modified successfully.");
                Console.WriteLine("Note: A system restart is required for these changes to take effect.");
                Console.ResetColor();
            }
            else
            {
                Logger.Instance.Warning("Some network configuration changes failed");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Some network configuration changes could not be applied:");
                if (!hostnameModified) Console.WriteLine("- Hostname change failed");
                if (!workgroupModified) Console.WriteLine("- Workgroup change failed");
                Console.WriteLine("Check logs for details.");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Handles exceptions from network configuration spoofing
        /// </summary>
        private static void HandleNetworkConfigException(Exception ex)
        {
            Logger.Instance.LogException(ex, "Error spoofing network configuration");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error spoofing network configuration: {ex.Message}");
            Console.ResetColor();
        }
    }
} 