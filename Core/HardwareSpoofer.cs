using System;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.Linq;

/*
    This class is responsible for spoofing the hardware of the computer.
    It is used to hide the original hardware of the computer from the software.
    It is also used to spoof the hardware of the computer to avoid detection by antivirus software.
*/

namespace StealthSpoof.Core
{
    public static class HardwareSpoofer
    {
        // Registry path constants
        private const string REG_PATH_GPU = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
        private const string REG_PATH_NETWORK_ADAPTERS = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
        private const string REG_PATH_WINDOWS_NT = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private const string REG_PATH_COMPUTER_NAME = @"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName";
        private const string REG_PATH_ACTIVE_COMPUTER_NAME = @"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName";
        private const string REG_PATH_TCPIP_PARAMETERS = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
        private const string REG_PATH_TCPIP_INTERFACES = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
        private const string REG_PATH_SCSI = @"HARDWARE\DEVICEMAP\Scsi";
        private const string REG_PATH_DISK_PERIPHERALS = @"HARDWARE\DEVICEMAP\Scsi\Scsi Port %d\Scsi Bus %d\Target Id %d\Logical Unit Id %d";
        private const string REG_PATH_HARDWARE_PROFILES = @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001";
        private const string REG_PATH_CRYPTOGRAPHY = @"SOFTWARE\Microsoft\Cryptography";
        private const string REG_PATH_SQM_CLIENT = @"SOFTWARE\Microsoft\SQMClient";
        private const string REG_PATH_SYSTEM_INFO = @"SYSTEM\CurrentControlSet\Control\SystemInformation";
        private const string REG_PATH_WINDOWS_UPDATE = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate";
        private const string REG_PATH_BIOS = @"HARDWARE\DESCRIPTION\System\BIOS";
        private const string REG_PATH_EFI_VARIABLES = @"SYSTEM\CurrentControlSet\Control\EFI\Variables";
        
        // Property name constants
        private const string PROP_PROCESSOR_ID = "ProcessorId";
        private const string PROP_SYSTEM_MANUFACTURER = "SystemManufacturer";
        private const string PROP_SYSTEM_PRODUCT_NAME = "SystemProductName";
        private const string PROP_BASEBOARD_PRODUCT = "BaseBoardProduct";
        
        private static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StealthSpoof", 
            "backup.json"
        );
        
        private static readonly string CMD_NETSH = "netsh";
        
        private static bool RequireAdminCheck()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
                
            if (!isAdmin)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This operation requires administrator privileges.");
                Console.WriteLine("Please restart the program as an administrator.");
                Console.ResetColor();
                return false;
            }
            
            return true;
        }
        
        // Method to spoof all hardware and software identifiers
        public static void SpoofAll()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting complete system spoofing process");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nStarting complete system spoofing process...");
            Console.ResetColor();
            
            try
            {
                BackupOriginalValues();
                SpoofAllHardware();
                SpoofPCName();
                SpoofInstallationID();
                SpoofExtendedGUIDs();
                SpoofEFIVariableId();
                SpoofSMBIOSSerialNumber();
                
                Logger.Instance.Info("Complete system spoofing finished successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nComplete system spoofing finished! It may be necessary to restart the computer to apply all changes.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error during complete system spoofing");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error during complete system spoofing: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofAllHardware()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting hardware spoofing process for all hardware components");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nStarting hardware spoofing process...");
            Console.ResetColor();
            
            try
            {
                BackupOriginalValues();
                SpoofCPU();
                SpoofDisk();
                SpoofMotherboard();
                SpoofGPU();
                SpoofMAC();
                
                Logger.Instance.Info("All hardware components spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSpoofing complete! It may be necessary to restart the computer to apply all changes.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error during hardware spoofing");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing hardware: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void BackupOriginalValues()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Backing up original hardware values");
            Console.WriteLine("\nBacking up original hardware values...");
            
            try
            {
                // Create a dictionary to store hardware information
                var backupData = new Dictionary<string, Dictionary<string, object>>();
                
                // CPU Backup
                Logger.Instance.Debug("Backing up CPU information");
                var cpuInfo = new Dictionary<string, object>();
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj[PROP_PROCESSOR_ID] != null)
                            cpuInfo[PROP_PROCESSOR_ID] = obj[PROP_PROCESSOR_ID]?.ToString() ?? string.Empty;
                        
                        // Other CPU properties can be added here
                        break; // Only backup the first CPU
                    }
                }
                backupData["CPU"] = cpuInfo;
                
                // Disk Backup
                Logger.Instance.Debug("Backing up Disk information");
                var diskInfo = new Dictionary<string, object>();
                using (var reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\disk\Enum"))
                {
                    if (reg != null)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            string keyName = i.ToString();
                            object? value = reg.GetValue(keyName);
                            
                            if (value != null)
                            {
                                diskInfo[keyName] = value.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
                backupData["Disk"] = diskInfo;
                
                // Motherboard/BIOS Backup
                var motherboardInfo = new Dictionary<string, object>();
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["SerialNumber"] != null)
                            motherboardInfo["SerialNumber"] = obj["SerialNumber"]?.ToString() ?? string.Empty;
                        if (obj["Manufacturer"] != null)
                            motherboardInfo["Manufacturer"] = obj["Manufacturer"]?.ToString() ?? string.Empty;
                        if (obj["Product"] != null)
                            motherboardInfo["Product"] = obj["Product"]?.ToString() ?? string.Empty;
                        
                        break; // Only backup the first motherboard
                    }
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["SerialNumber"] != null)
                            motherboardInfo["BIOSSerial"] = obj["SerialNumber"]?.ToString() ?? string.Empty;
                        
                        break;
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(REG_PATH_SYSTEM_INFO))
                {
                    if (key != null)
                    {
                        object? manufacturer = key.GetValue(PROP_SYSTEM_MANUFACTURER);
                        object? productName = key.GetValue(PROP_SYSTEM_PRODUCT_NAME);
                        object? baseBoard = key.GetValue(PROP_BASEBOARD_PRODUCT);
                        
                        if (manufacturer != null)
                            motherboardInfo[PROP_SYSTEM_MANUFACTURER] = manufacturer.ToString() ?? string.Empty;
                        if (productName != null)
                            motherboardInfo[PROP_SYSTEM_PRODUCT_NAME] = productName.ToString() ?? string.Empty;
                        if (baseBoard != null)
                            motherboardInfo[PROP_BASEBOARD_PRODUCT] = baseBoard.ToString() ?? string.Empty;
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\HardwareConfig"))
                {
                    if (key != null)
                    {
                        object? lastConfig = key.GetValue("LastConfig");
                        if (lastConfig != null)
                            motherboardInfo["UUID"] = lastConfig?.ToString() ?? string.Empty;
                    }
                }
                backupData["Motherboard"] = motherboardInfo;
                
                // GPU Backup
                var gpuInfo = new Dictionary<string, object>();
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["Name"] != null)
                            gpuInfo["Name"] = obj["Name"]?.ToString() ?? string.Empty;
                        if (obj["AdapterRAM"] != null)
                            gpuInfo["AdapterRAM"] = obj["AdapterRAM"]?.ToString() ?? string.Empty;
                        
                        break; // Only backup the first GPU
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(REG_PATH_GPU))
                {
                    if (key != null)
                    {
                        object? hardwareId = key.GetValue("HardwareID");
                        object? memorySize = key.GetValue("HardwareInformation.qwMemorySize");
                        
                        if (hardwareId != null)
                            gpuInfo["HardwareID"] = hardwareId?.ToString() ?? string.Empty;
                        if (memorySize != null)
                            gpuInfo["MemorySize"] = memorySize?.ToString() ?? string.Empty;
                    }
                }
                backupData["GPU"] = gpuInfo;
                
                // MAC Address Backup
                var macInfo = new Dictionary<string, object>();
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                        nic.OperationalStatus == OperationalStatus.Up)
                    {
                        string mac = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");
                        if (!string.IsNullOrEmpty(mac) && mac != "00:00:00:00:00:00")
                        {
                            string registryId = GetNetworkAdapterId(nic.Description);
                            if (!string.IsNullOrEmpty(registryId))
                            {
                                macInfo[registryId] = mac;
                            }
                        }
                    }
                }
                backupData["MAC"] = macInfo;
                
                // Create directory if it doesn't exist
                string? backupDir = Path.GetDirectoryName(BackupPath);
                if (backupDir != null)
                {
                    Directory.CreateDirectory(backupDir);
                }
                
                // Serialize to JSON and save
                string json = System.Text.Json.JsonSerializer.Serialize(
                    backupData, 
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                );
                File.WriteAllText(BackupPath, json);
                
                Logger.Instance.Info($"Backup completed successfully: {BackupPath}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Original hardware values backed up successfully to: {BackupPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error backing up hardware values");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error backing up hardware values: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static string GetNetworkAdapterId(string adapterDescription)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_NetworkAdapter WHERE Description = '{adapterDescription.Replace("'", "\\'")}'"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["DeviceID"] != null)
                        {
                            return obj["DeviceID"].ToString() ?? string.Empty;
                        }
                    }
                }
            }
            catch { /* Ignore errors */ }
            
            return string.Empty;
        }
        
        public static void SpoofCPU()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting CPU spoofing");
            Console.WriteLine("\nSpoofing CPU...");
            
            try
            {
                string newCpuId = HardwareInfo.GetRandomHardwareID();
                Logger.Instance.Debug($"Generated new CPU ID: {newCpuId}");
                
                string registryKey = REG_PATH_GPU;
                Logger.Instance.LogRegistryOperation(registryKey, PROP_PROCESSOR_ID, "MODIFY");
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryKey))
                {
                    key.SetValue(PROP_PROCESSOR_ID, newCpuId, RegistryValueKind.String);
                }
                
                Logger.Instance.Info($"CPU ID spoofed successfully to: {newCpuId}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"CPU ID modified to: {newCpuId}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing CPU");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing CPU: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofDisk()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing Disks...");
            
            try
            {
                string newDiskId = HardwareInfo.GetRandomHardwareID(20);
                
                using (var reg = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\disk\Enum"))
                {
                    if (reg != null)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            try
                            {
                                string keyName = i.ToString();
                                object? value = reg.GetValue(keyName);
                                
                                if (value != null)
                                {
                                    string valueStr = value.ToString() ?? string.Empty;
                                    string[] parts = valueStr.Split('&');
                                    if (parts.Length > 1)
                                    {
                                        reg.SetValue(keyName, valueStr.Replace(parts[1], $"&{newDiskId}"));
                                    }
                                }
                            }
                            catch { /* Ignore errors for non-existent entries */ }
                        }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Disks modified successfully. Restart the computer to apply.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing disks: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofMotherboard()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing Motherboard and BIOS...");
            
            try
            {
                string newSerialNumber = HardwareInfo.GetRandomHardwareID(16);
                string newUUID = Guid.NewGuid().ToString().ToUpper();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH_SYSTEM_INFO))
                {
                    if (key != null)
                    {
                        key.SetValue(PROP_SYSTEM_MANUFACTURER, "SpooferBIOS", RegistryValueKind.String);
                        key.SetValue(PROP_SYSTEM_PRODUCT_NAME, "StealthBoard", RegistryValueKind.String);
                        key.SetValue(PROP_BASEBOARD_PRODUCT, "SB-" + newSerialNumber, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\HardwareConfig"))
                {
                    if (key != null)
                    {
                        key.SetValue("LastConfig", newUUID, RegistryValueKind.String);
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Motherboard modified successfully. New ID: {newSerialNumber}");
                Console.WriteLine($"UUID modified to: {newUUID}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing motherboard: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofGPU()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing GPU...");
            
            try
            {
                string newGpuId = HardwareInfo.GetRandomHardwareID();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH_GPU))
                {
                    if (key != null)
                    {
                        key.SetValue("HardwareInformation.qwMemorySize", new Random().Next(1, 16) * 1073741824, RegistryValueKind.QWord);
                        key.SetValue("HardwareID", newGpuId, RegistryValueKind.String);
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"GPU modified successfully. New ID: {newGpuId}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing GPU: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofMAC()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing MAC addresses...");
            
            try
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
                
                if (adapters.Count == 0)
                {
                    Console.WriteLine("No network adapters found.");
                    return;
                }
                
                Console.WriteLine("\nAvailable adapters:");
                for (int i = 0; i < adapters.Count; i++)
                {
                    Console.WriteLine($"{i+1}. {adapters[i]}");
                }
                
                Console.Write("\nChoose the adapter to spoof (0 for all): ");
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int selection))
                {
                    if (selection == 0)
                    {
                        foreach (var adapter in adapters)
                        {
                            string deviceId = adapter.Split(':')[0];
                            SpoofSingleMAC(deviceId);
                        }
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
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing MAC addresses: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void SpoofSingleMAC(string deviceId)
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
        
        private static void DisableNetworkAdapter(string deviceId)
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
        
        private static void EnableNetworkAdapter(string deviceId)
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
        
        public static void RestoreOriginal()
        {
            if (!RequireAdminCheck()) return;
            
            if (!File.Exists(BackupPath))
            {
                Logger.Instance.Warning("No backup file found for restoration");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No backups found to restore.");
                Console.ResetColor();
                return;
            }
            
            Logger.Instance.Info("Starting hardware restoration process");
            Console.WriteLine("\nRestoring original hardware settings...");
            
            try
            {
                var backupData = LoadBackupData();
                if (backupData == null) return;
                
                RestoreCPU(backupData);
                RestoreDisk(backupData);
                RestoreMotherboard(backupData);
                RestoreGPU(backupData);
                RestoreMAC(backupData);
                
                Logger.Instance.Info("Hardware restoration completed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nOriginal hardware settings restored successfully!");
                Console.WriteLine("You should restart your computer to apply all changes.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring original settings");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error restoring settings: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static Dictionary<string, Dictionary<string, object>>? LoadBackupData()
        {
            try
            {
                // Read and deserialize the backup file
                string json = File.ReadAllText(BackupPath);
                var backupData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                
                if (backupData == null)
                {
                    Logger.Instance.Error("Backup file is invalid or corrupted");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Backup file is invalid or corrupted.");
                    Console.ResetColor();
                }
                
                return backupData;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error loading backup data");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading backup data: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }
        
        private static void RestoreCPU(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring CPU information");
            if (backupData.TryGetValue("CPU", out var cpuInfo) && cpuInfo.TryGetValue(PROP_PROCESSOR_ID, out var processorId))
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH_GPU))
                {
                    if (key != null)
                    {
                        key.SetValue(PROP_PROCESSOR_ID, processorId?.ToString() ?? string.Empty, RegistryValueKind.String);
                        Console.WriteLine("CPU ID restored successfully.");
                    }
                }
            }
        }
        
        private static void RestoreDisk(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring Disk information");
            if (backupData.TryGetValue("Disk", out var diskInfo))
            {
                using (var reg = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\disk\Enum"))
                {
                    if (reg != null)
                    {
                        foreach (var entry in diskInfo)
                        {
                            string valueStr = entry.Value?.ToString() ?? string.Empty;
                            reg.SetValue(entry.Key, valueStr, RegistryValueKind.String);
                        }
                        Console.WriteLine("Disk IDs restored successfully.");
                    }
                }
            }
        }
        
        private static void RestoreMotherboard(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring Motherboard information");
            if (backupData.TryGetValue("Motherboard", out var motherboardInfo))
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH_SYSTEM_INFO))
                {
                    if (key != null)
                    {
                        if (motherboardInfo.TryGetValue(PROP_SYSTEM_MANUFACTURER, out var manufacturer))
                            key.SetValue(PROP_SYSTEM_MANUFACTURER, manufacturer?.ToString() ?? string.Empty, RegistryValueKind.String);
                        
                        if (motherboardInfo.TryGetValue(PROP_SYSTEM_PRODUCT_NAME, out var productName))
                            key.SetValue(PROP_SYSTEM_PRODUCT_NAME, productName?.ToString() ?? string.Empty, RegistryValueKind.String);
                        
                        if (motherboardInfo.TryGetValue(PROP_BASEBOARD_PRODUCT, out var baseBoard))
                            key.SetValue(PROP_BASEBOARD_PRODUCT, baseBoard?.ToString() ?? string.Empty, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\HardwareConfig"))
                {
                    if (key != null && motherboardInfo.TryGetValue("UUID", out var uuid))
                    {
                        string uuidStr = uuid?.ToString() ?? string.Empty;
                        key.SetValue("LastConfig", uuidStr, RegistryValueKind.String);
                    }
                }
                
                Console.WriteLine("Motherboard information restored successfully.");
            }
        }
        
        private static void RestoreGPU(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring GPU information");
            if (backupData.TryGetValue("GPU", out var gpuInfo))
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH_GPU))
                {
                    if (key != null)
                    {
                        if (gpuInfo.TryGetValue("HardwareID", out var hardwareId))
                        {
                            string hardwareIdStr = hardwareId?.ToString() ?? string.Empty;
                            key.SetValue("HardwareID", hardwareIdStr, RegistryValueKind.String);
                        }
                        
                        if (gpuInfo.TryGetValue("MemorySize", out var memorySize))
                        {
                            string memorySizeStr = memorySize?.ToString() ?? "0";
                            if (long.TryParse(memorySizeStr, out long size))
                                key.SetValue("HardwareInformation.qwMemorySize", size, RegistryValueKind.QWord);
                        }
                    }
                }
                
                Console.WriteLine("GPU information restored successfully.");
            }
        }
        
        private static void RestoreMAC(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring MAC addresses");
            if (backupData.TryGetValue("MAC", out var macInfo))
            {
                foreach (var entry in macInfo)
                {
                    string deviceId = entry.Key;
                    string mac = entry.Value?.ToString()?.Replace(":", "") ?? string.Empty;
                    
                    RestoreSingleMAC(deviceId, mac);
                }
                
                Console.WriteLine("MAC addresses restored successfully.");
            }
        }
        
        private static void RestoreSingleMAC(string deviceId, string mac)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                $@"SYSTEM\CurrentControlSet\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{deviceId}"))
            {
                if (key != null)
                {
                    key.SetValue("NetworkAddress", mac, RegistryValueKind.String);
                    
                    // Disable and re-enable the network adapter to apply changes
                    DisableNetworkAdapter(deviceId);
                    Thread.Sleep(1000);
                    EnableNetworkAdapter(deviceId);
                }
            }
        }
        
        public static void SpoofInstallationID()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting Installation ID spoofing");
            Console.WriteLine("\nSpoofing Installation ID...");
            
            try
            {
                string newInstallationID = Guid.NewGuid().ToString().ToUpper();
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_WINDOWS_NT))
                {
                    if (key != null)
                    {
                        key.SetValue("InstallationID", newInstallationID, RegistryValueKind.String);
                        key.SetValue("InstallDate", DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds, RegistryValueKind.DWord);
                    }
                }
                
                Logger.Instance.Info($"Installation ID spoofed successfully to: {newInstallationID}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Installation ID modified to: {newInstallationID}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing Installation ID");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing Installation ID: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofPCName()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting PC Name spoofing");
            Console.WriteLine("\nSpoofing PC Name...");
            
            try
            {
                string newPCName = "PC-" + HardwareInfo.GetRandomHardwareID(8);
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_COMPUTER_NAME))
                {
                    if (key != null)
                    {
                        key.SetValue("ComputerName", newPCName, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_ACTIVE_COMPUTER_NAME))
                {
                    if (key != null)
                    {
                        key.SetValue("ComputerName", newPCName, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_TCPIP_PARAMETERS))
                {
                    if (key != null)
                    {
                        key.SetValue("Hostname", newPCName, RegistryValueKind.String);
                        key.SetValue("NV Hostname", newPCName, RegistryValueKind.String);
                    }
                }
                
                Logger.Instance.Info($"PC Name spoofed successfully to: {newPCName}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PC Name modified to: {newPCName}");
                Console.WriteLine("Note: A system restart is required for this change to take full effect.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing PC Name");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing PC Name: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofAdvancedDiskInfo()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting Advanced Disk Info spoofing");
            Console.WriteLine("\nSpoofing Advanced Disk Information...");
            
            try
            {
                // Spoof SCSI information
                using (RegistryKey? scsiPorts = Registry.LocalMachine.OpenSubKey(REG_PATH_SCSI, true))
                {
                    if (scsiPorts != null)
                    {
                        foreach (string portName in scsiPorts.GetSubKeyNames())
                        {
                            using (RegistryKey? port = scsiPorts.OpenSubKey(portName, true))
                            {
                                if (port != null)
                                {
                                    foreach (string busName in port.GetSubKeyNames())
                                    {
                                        if (busName.StartsWith("Scsi Bus"))
                                        {
                                            using (RegistryKey? bus = port.OpenSubKey(busName, true))
                                            {
                                                if (bus != null)
                                                {
                                                    foreach (string targetName in bus.GetSubKeyNames())
                                                    {
                                                        using (RegistryKey? target = bus.OpenSubKey(targetName, true))
                                                        {
                                                            if (target != null)
                                                            {
                                                                foreach (string lun in target.GetSubKeyNames())
                                                                {
                                                                    using (RegistryKey? lunKey = target.OpenSubKey(lun, true))
                                                                    {
                                                                        if (lunKey != null)
                                                                        {
                                                                            lunKey.SetValue("Identifier", HardwareInfo.GetRandomHardwareID(20), RegistryValueKind.String);
                                                                            lunKey.SetValue("SerialNumber", HardwareInfo.GetRandomHardwareID(16), RegistryValueKind.String);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                Logger.Instance.Info("Advanced Disk Info spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Advanced Disk Information modified successfully.");
                Console.WriteLine("Note: A system restart is required for these changes to take full effect.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing Advanced Disk Info");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing Advanced Disk Info: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofExtendedGUIDs()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting Extended GUIDs spoofing");
            Console.WriteLine("\nSpoofing Extended GUIDs...");
            
            try
            {
                // Spoof Machine GUID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_CRYPTOGRAPHY))
                {
                    if (key != null)
                    {
                        key.SetValue("MachineGuid", Guid.NewGuid().ToString(), RegistryValueKind.String);
                    }
                }
                
                // Spoof Hardware Profile GUID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_HARDWARE_PROFILES))
                {
                    if (key != null)
                    {
                        key.SetValue("HwProfileGuid", "{" + Guid.NewGuid().ToString().ToUpper() + "}", RegistryValueKind.String);
                    }
                }
                
                // Spoof SQM Client ID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_SQM_CLIENT))
                {
                    if (key != null)
                    {
                        key.SetValue("MachineId", "{" + Guid.NewGuid().ToString().ToUpper() + "}", RegistryValueKind.String);
                    }
                }
                
                // Spoof Windows Update ID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_WINDOWS_UPDATE))
                {
                    if (key != null)
                    {
                        key.SetValue("SusClientId", Guid.NewGuid().ToString(), RegistryValueKind.String);
                    }
                }
                
                Logger.Instance.Info("Extended GUIDs spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Extended GUIDs modified successfully.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing Extended GUIDs");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing Extended GUIDs: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofEFIVariableId()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting EFI Variable ID spoofing");
            Console.WriteLine("\nSpoofing EFI Variable ID...");
            
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_BIOS))
                {
                    if (key != null)
                    {
                        key.SetValue(PROP_SYSTEM_MANUFACTURER, "StealthSpoof BIOS", RegistryValueKind.String);
                        key.SetValue(PROP_SYSTEM_PRODUCT_NAME, "StealthSpoof System", RegistryValueKind.String);
                        key.SetValue("SystemFamily", "StealthSpoof Family", RegistryValueKind.String);
                        key.SetValue("SystemVersion", "1.0", RegistryValueKind.String);
                        key.SetValue("SystemSKU", "SS-" + HardwareInfo.GetRandomHardwareID(8), RegistryValueKind.String);
                    }
                }
                
                Logger.Instance.Info("EFI Variable ID spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("EFI Variable ID modified successfully.");
                Console.WriteLine("Note: Some changes may require a system restart to take full effect.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing EFI Variable ID");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing EFI Variable ID: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofSMBIOSSerialNumber()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting SMBIOS Serial Number spoofing");
            Console.WriteLine("\nSpoofing SMBIOS Serial Number...");
            
            try
            {
                string newSerialNumber = HardwareInfo.GetRandomHardwareID(16);
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(REG_PATH_BIOS))
                {
                    if (key != null)
                    {
                        key.SetValue("SystemSerialNumber", newSerialNumber, RegistryValueKind.String);
                    }
                }
                
                Logger.Instance.Info($"SMBIOS Serial Number spoofed successfully to: {newSerialNumber}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"SMBIOS Serial Number modified to: {newSerialNumber}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing SMBIOS Serial Number");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing SMBIOS Serial Number: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void CheckRegistryKeys()
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info("Checking registry keys");
            Console.WriteLine("\nChecking Registry Keys...");
            
            try
            {
                Console.WriteLine("\n=== Registry Key Status ===");
                
                CheckRegistryKey(REG_PATH_GPU, "GPU Registry");
                CheckRegistryKey(REG_PATH_NETWORK_ADAPTERS, "Network Adapters Registry");
                CheckRegistryKey(REG_PATH_WINDOWS_NT, "Windows NT Registry");
                CheckRegistryKey(REG_PATH_COMPUTER_NAME, "Computer Name Registry");
                CheckRegistryKey(REG_PATH_ACTIVE_COMPUTER_NAME, "Active Computer Name Registry");
                CheckRegistryKey(REG_PATH_TCPIP_PARAMETERS, "TCP/IP Parameters Registry");
                CheckRegistryKey(REG_PATH_SCSI, "SCSI Registry");
                CheckRegistryKey(REG_PATH_HARDWARE_PROFILES, "Hardware Profiles Registry");
                CheckRegistryKey(REG_PATH_CRYPTOGRAPHY, "Cryptography Registry");
                CheckRegistryKey(REG_PATH_SQM_CLIENT, "SQM Client Registry");
                CheckRegistryKey(REG_PATH_SYSTEM_INFO, "System Information Registry");
                CheckRegistryKey(REG_PATH_WINDOWS_UPDATE, "Windows Update Registry");
                CheckRegistryKey(REG_PATH_BIOS, "BIOS Registry");
                
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
        
        private static void CheckRegistryKey(string keyPath, string keyName)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ {keyName}: Accessible");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"✗ {keyName}: Not accessible");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ {keyName}: Error - {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void DisplaySystemData()
        {
            Logger.Instance.Info("Displaying system data");
            Console.WriteLine("\n=== System Data ===");
            
            try
            {
                // Display OS information
                Console.WriteLine("\n--- Operating System ---");
                Console.WriteLine($"OS: {Environment.OSVersion}");
                Console.WriteLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
                Console.WriteLine($"Machine Name: {Environment.MachineName}");
                Console.WriteLine($"User Name: {Environment.UserName}");
                
                // Display CPU information
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
                
                // Display motherboard information
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
                
                // Display BIOS information
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
                
                // Display disk information
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
                
                // Display network adapter information
                Console.WriteLine("\n--- Network Adapters ---");
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        Console.WriteLine($"Name: {nic.Name}");
                        Console.WriteLine($"Description: {nic.Description}");
                        Console.WriteLine($"MAC Address: {BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":")}");
                        Console.WriteLine($"Status: {nic.OperationalStatus}");
                        Console.WriteLine("---");
                    }
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
        
        public static void ClearGameCache(string gameName)
        {
            if (!RequireAdminCheck()) return;
            
            Logger.Instance.Info($"Clearing {gameName} cache");
            Console.WriteLine($"\nClearing {gameName} cache...");
            
            try
            {
                switch (gameName)
                {
                    case "Ubisoft":
                        ClearUbisoftCache();
                        break;
                    case "Valorant":
                        ClearValorantCache();
                        break;
                    case "CallOfDuty":
                        ClearCallOfDutyCache();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unknown game: {gameName}");
                        Console.ResetColor();
                        return;
                }
                
                Logger.Instance.Info($"{gameName} cache cleared successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{gameName} cache cleared successfully!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error clearing {gameName} cache");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error clearing {gameName} cache: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void ClearUbisoftCache()
        {
            string ubisoftPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Ubisoft Game Launcher"
            );
            
            ClearDirectory(ubisoftPath, "cache");
            ClearDirectory(ubisoftPath, "logs");
            ClearDirectory(ubisoftPath, "temp");
        }
        
        private static void ClearValorantCache()
        {
            string valorantPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Riot Games", "Valorant"
            );
            
            ClearDirectory(valorantPath, "Saved");
            
            string vgcPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Riot Vanguard"
            );
            
            if (Directory.Exists(vgcPath))
            {
                Console.WriteLine("Note: Vanguard files found. You may need to reinstall Vanguard after spoofing.");
            }
        }
        
        private static void ClearCallOfDutyCache()
        {
            string codPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Activision"
            );
            
            ClearDirectory(codPath, "Cache");
            
            string blizzardPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Battle.net"
            );
            
            ClearDirectory(blizzardPath, "Cache");
        }
        
        private static void ClearDirectory(string basePath, string dirName)
        {
            string fullPath = Path.Combine(basePath, dirName);
            
            if (Directory.Exists(fullPath))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(fullPath);
                    
                    foreach (FileInfo file in di.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Warning($"Could not delete file {file.FullName}: {ex.Message}");
                        }
                    }
                    
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        try
                        {
                            dir.Delete(true);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Warning($"Could not delete directory {dir.FullName}: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"Cleared {fullPath}");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Error clearing directory {fullPath}: {ex.Message}");
                    Console.WriteLine($"Error clearing {fullPath}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Directory not found: {fullPath}");
            }
        }
    }
} 