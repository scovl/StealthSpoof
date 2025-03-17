using System;
using System.IO;
using System.Management;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Linq;
using StealthSpoof.Core.Utils;
using System.Threading;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Class responsible for backing up and restoring hardware information
    /// </summary>
    public static class BackupManager
    {
        private static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StealthSpoof", 
            "backup.json"
        );
        
        private static readonly string CMD_NETSH = "netsh";
        
        // Property name constants
        private const string PROP_SERIAL_NUMBER = "SerialNumber";
        private const string PROP_NAME = "Name";
        private const string PROP_MANUFACTURER = "Manufacturer";
        private const string PROP_PRODUCT = "Product";
        private const string PROP_ADAPTER_RAM = "AdapterRAM";
        private const string PROP_DEVICE_ID = "DeviceID";
        
        /// <summary>
        /// Realizes the backup of the original hardware values
        /// </summary>
        public static void BackupOriginalValues()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Backing up original hardware values");
            Console.WriteLine("\nBacking up original hardware values...");
            
            try
            {
                // Create a dictionary to store hardware information
                var backupData = new Dictionary<string, Dictionary<string, object>>();
                
                // Backup each component
                backupData["CPU"] = BackupCPUInfo();
                backupData["Disk"] = BackupDiskInfo();
                backupData["Motherboard"] = BackupMotherboardInfo();
                backupData["GPU"] = BackupGPUInfo();
                backupData["MAC"] = BackupMACInfo();
                
                // Save the backup data
                SaveBackupData(backupData);
                
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
        
        /// <summary>
        /// Backs up CPU information
        /// </summary>
        private static Dictionary<string, object> BackupCPUInfo()
        {
            Logger.Instance.Debug("Backing up CPU information");
            var cpuInfo = new Dictionary<string, object>();
            
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                var firstCpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstCpu != null && firstCpu[RegistryHelper.PROP_PROCESSOR_ID] != null)
                {
                    cpuInfo[RegistryHelper.PROP_PROCESSOR_ID] = firstCpu[RegistryHelper.PROP_PROCESSOR_ID]?.ToString() ?? string.Empty;
                    // Other CPU properties can be added here
                }
            }
            
            return cpuInfo;
        }
        
        /// <summary>
        /// Backs up disk information
        /// </summary>
        private static Dictionary<string, object> BackupDiskInfo()
        {
            Logger.Instance.Debug("Backing up Disk information");
            var diskInfo = new Dictionary<string, object>();
            
            using (var reg = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_DISK_ENUM))
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
            
            return diskInfo;
        }
        
        /// <summary>
        /// Backs up motherboard information
        /// </summary>
        private static Dictionary<string, object> BackupMotherboardInfo()
        {
            Logger.Instance.Debug("Backing up Motherboard information");
            var motherboardInfo = new Dictionary<string, object>();
            
            BackupBaseBoardInfo(motherboardInfo);
            BackupBIOSInfo(motherboardInfo);
            BackupSystemInfo(motherboardInfo);
            BackupHardwareConfigInfo(motherboardInfo);
            
            return motherboardInfo;
        }
        
        /// <summary>
        /// Backs up base board information
        /// </summary>
        private static void BackupBaseBoardInfo(Dictionary<string, object> motherboardInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                var firstMotherboard = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstMotherboard != null)
                {
                    if (firstMotherboard[PROP_SERIAL_NUMBER] != null)
                        motherboardInfo[PROP_SERIAL_NUMBER] = firstMotherboard[PROP_SERIAL_NUMBER]?.ToString() ?? string.Empty;
                    if (firstMotherboard[PROP_MANUFACTURER] != null)
                        motherboardInfo[PROP_MANUFACTURER] = firstMotherboard[PROP_MANUFACTURER]?.ToString() ?? string.Empty;
                    if (firstMotherboard[PROP_PRODUCT] != null)
                        motherboardInfo[PROP_PRODUCT] = firstMotherboard[PROP_PRODUCT]?.ToString() ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Backs up BIOS information
        /// </summary>
        private static void BackupBIOSInfo(Dictionary<string, object> motherboardInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                var firstBios = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstBios != null && firstBios[PROP_SERIAL_NUMBER] != null)
                {
                    motherboardInfo["BIOSSerial"] = firstBios[PROP_SERIAL_NUMBER]?.ToString() ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Backs up system information
        /// </summary>
        private static void BackupSystemInfo(Dictionary<string, object> motherboardInfo)
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_SYSTEM_INFO))
            {
                if (key != null)
                {
                    object? manufacturer = key.GetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER);
                    object? productName = key.GetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME);
                    object? baseBoard = key.GetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT);
                    
                    if (manufacturer != null)
                        motherboardInfo[RegistryHelper.PROP_SYSTEM_MANUFACTURER] = manufacturer.ToString() ?? string.Empty;
                    if (productName != null)
                        motherboardInfo[RegistryHelper.PROP_SYSTEM_PRODUCT_NAME] = productName.ToString() ?? string.Empty;
                    if (baseBoard != null)
                        motherboardInfo[RegistryHelper.PROP_BASEBOARD_PRODUCT] = baseBoard.ToString() ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Backs up hardware config information
        /// </summary>
        private static void BackupHardwareConfigInfo(Dictionary<string, object> motherboardInfo)
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\HardwareConfig"))
            {
                if (key != null)
                {
                    object? lastConfig = key.GetValue("LastConfig");
                    if (lastConfig != null)
                        motherboardInfo["UUID"] = lastConfig?.ToString() ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Backs up GPU information
        /// </summary>
        private static Dictionary<string, object> BackupGPUInfo()
        {
            var gpuInfo = new Dictionary<string, object>();
            
            BackupVideoControllerInfo(gpuInfo);
            BackupGPURegistryInfo(gpuInfo);
            
            return gpuInfo;
        }
        
        /// <summary>
        /// Backs up video controller information
        /// </summary>
        private static void BackupVideoControllerInfo(Dictionary<string, object> gpuInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                var firstGpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstGpu != null)
                {
                    if (firstGpu[PROP_NAME] != null)
                        gpuInfo[PROP_NAME] = firstGpu[PROP_NAME]?.ToString() ?? string.Empty;
                    if (firstGpu[PROP_ADAPTER_RAM] != null)
                        gpuInfo[PROP_ADAPTER_RAM] = firstGpu[PROP_ADAPTER_RAM]?.ToString() ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Backs up GPU registry information
        /// </summary>
        private static void BackupGPURegistryInfo(Dictionary<string, object> gpuInfo)
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_GPU))
            {
                if (key != null)
                {
                    object? hardwareId = key.GetValue(RegistryHelper.PROP_HARDWARE_ID);
                    object? memorySize = key.GetValue(RegistryHelper.PROP_MEMORY_SIZE);
                    
                    if (hardwareId != null)
                        gpuInfo[RegistryHelper.PROP_HARDWARE_ID] = hardwareId?.ToString() ?? string.Empty;
                    if (memorySize != null)
                        gpuInfo["MemorySize"] = memorySize?.ToString() ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Backs up MAC address information
        /// </summary>
        private static Dictionary<string, object> BackupMACInfo()
        {
            var macInfo = new Dictionary<string, object>();
            
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                            n.OperationalStatus == OperationalStatus.Up))
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
            
            return macInfo;
        }
        
        /// <summary>
        /// Saves the backup data to a file
        /// </summary>
        private static void SaveBackupData(Dictionary<string, Dictionary<string, object>> backupData)
        {
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
        }
        
        /// <summary>
        /// Gets the network adapter ID from the description
        /// </summary>
        private static string GetNetworkAdapterId(string adapterDescription)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_NetworkAdapter WHERE Description = '{adapterDescription.Replace("'", "\\'")}'"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj[PROP_DEVICE_ID] != null)
                        {
                            return obj[PROP_DEVICE_ID].ToString() ?? string.Empty;
                        }
                    }
                }
            }
            catch { /* Ignore errors */ }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Restores the original hardware values from the backup
        /// </summary>
        public static void RestoreOriginal()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
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
        
        /// <summary>
        /// Loads the backup data from the file
        /// </summary>
        public static Dictionary<string, Dictionary<string, object>>? LoadBackupData()
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
        
        /// <summary>
        /// Restores the CPU information
        /// </summary>
        private static void RestoreCPU(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring CPU information");
            if (backupData.TryGetValue("CPU", out var cpuInfo) && cpuInfo.TryGetValue(RegistryHelper.PROP_PROCESSOR_ID, out var processorId))
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_GPU))
                {
                    if (key != null)
                    {
                        key.SetValue(RegistryHelper.PROP_PROCESSOR_ID, processorId?.ToString() ?? string.Empty, RegistryValueKind.String);
                        Console.WriteLine("CPU ID restored successfully.");
                    }
                }
            }
        }
        
        /// <summary>
        /// Restores the disk information
        /// </summary>
        private static void RestoreDisk(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring Disk information");
            if (backupData.TryGetValue("Disk", out var diskInfo))
            {
                using (var reg = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_DISK_ENUM))
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
        
        /// <summary>
        /// Restores the motherboard information
        /// </summary>
        private static void RestoreMotherboard(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring Motherboard information");
            if (backupData.TryGetValue("Motherboard", out var motherboardInfo))
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_SYSTEM_INFO))
                {
                    if (key != null)
                    {
                        if (motherboardInfo.TryGetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, out var manufacturer))
                            key.SetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, manufacturer?.ToString() ?? string.Empty, RegistryValueKind.String);
                        
                        if (motherboardInfo.TryGetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, out var productName))
                            key.SetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, productName?.ToString() ?? string.Empty, RegistryValueKind.String);
                        
                        if (motherboardInfo.TryGetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, out var baseBoard))
                            key.SetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, baseBoard?.ToString() ?? string.Empty, RegistryValueKind.String);
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
        
        /// <summary>
        /// Restores the GPU information
        /// </summary>
        private static void RestoreGPU(Dictionary<string, Dictionary<string, object>> backupData)
        {
            Logger.Instance.Debug("Restoring GPU information");
            if (backupData.TryGetValue("GPU", out var gpuInfo))
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_GPU))
                {
                    if (key != null)
                    {
                        if (gpuInfo.TryGetValue(RegistryHelper.PROP_HARDWARE_ID, out var hardwareId))
                        {
                            string hardwareIdStr = hardwareId?.ToString() ?? string.Empty;
                            key.SetValue(RegistryHelper.PROP_HARDWARE_ID, hardwareIdStr, RegistryValueKind.String);
                        }
                        
                        if (gpuInfo.TryGetValue("MemorySize", out var memorySize))
                        {
                            string memorySizeStr = memorySize?.ToString() ?? "0";
                            if (long.TryParse(memorySizeStr, out long size))
                                key.SetValue(RegistryHelper.PROP_MEMORY_SIZE, size, RegistryValueKind.QWord);
                        }
                    }
                }
                
                Console.WriteLine("GPU information restored successfully.");
            }
        }
        
        /// <summary>
        /// Restores the MAC addresses
        /// </summary>
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
        
        /// <summary>
        /// Restores a single MAC address
        /// </summary>
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
        
        private static void DisableNetworkAdapter(string deviceId)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
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
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
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