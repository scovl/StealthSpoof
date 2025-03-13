using System;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Security.Principal;
using System.Net.NetworkInformation;

/*
    This class is responsible for spoofing the hardware of the computer.
    It is used to hide the original hardware of the computer from the software.
    It is also used to spoof the hardware of the computer to avoid detection by antivirus software.
*/

namespace StealthSpoof.Core
{
    public static class HardwareSpoofer
    {
        private static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StealthSpoof", 
            "backup.json"
        );
        
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
                        if (obj["ProcessorId"] != null)
                            cpuInfo["ProcessorId"] = obj["ProcessorId"]?.ToString() ?? string.Empty;
                        
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
                
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SystemInformation"))
                {
                    if (key != null)
                    {
                        object? manufacturer = key.GetValue("SystemManufacturer");
                        object? productName = key.GetValue("SystemProductName");
                        object? baseBoard = key.GetValue("BaseBoardProduct");
                        
                        if (manufacturer != null)
                            motherboardInfo["SystemManufacturer"] = manufacturer.ToString() ?? string.Empty;
                        if (productName != null)
                            motherboardInfo["SystemProductName"] = productName.ToString() ?? string.Empty;
                        if (baseBoard != null)
                            motherboardInfo["BaseBoardProduct"] = baseBoard.ToString() ?? string.Empty;
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
                
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
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
                
                string registryKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
                Logger.Instance.LogRegistryOperation(registryKey, "ProcessorId", "MODIFY");
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryKey))
                {
                    key.SetValue("ProcessorId", newCpuId, RegistryValueKind.String);
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
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\SystemInformation"))
                {
                    if (key != null)
                    {
                        key.SetValue("SystemManufacturer", "SpooferBIOS", RegistryValueKind.String);
                        key.SetValue("SystemProductName", "StealthBoard", RegistryValueKind.String);
                        key.SetValue("BaseBoardProduct", "SB-" + newSerialNumber, RegistryValueKind.String);
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
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
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
                        
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = "netsh";
                            process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" disabled";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.CreateNoWindow = true;
                            process.Start();
                            process.WaitForExit();
                        }
                        
                        Thread.Sleep(1000);
                        
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = "netsh";
                            process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" enabled";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.CreateNoWindow = true;
                            process.Start();
                            process.WaitForExit();
                        }
                        
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
                // Read and deserialize the backup file
                string json = File.ReadAllText(BackupPath);
                var backupData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                
                if (backupData == null)
                {
                    Logger.Instance.Error("Backup file is invalid or corrupted");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Backup file is invalid or corrupted.");
                    Console.ResetColor();
                    return;
                }
                
                // Restore CPU
                Logger.Instance.Debug("Restoring CPU information");
                if (backupData.TryGetValue("CPU", out var cpuInfo) && cpuInfo.TryGetValue("ProcessorId", out var processorId))
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                        @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
                    {
                        if (key != null)
                        {
                            key.SetValue("ProcessorId", processorId?.ToString() ?? string.Empty, RegistryValueKind.String);
                            Console.WriteLine("CPU ID restored successfully.");
                        }
                    }
                }
                
                // Restore Disk
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
                
                // Restore Motherboard/BIOS
                Logger.Instance.Debug("Restoring Motherboard information");
                if (backupData.TryGetValue("Motherboard", out var motherboardInfo))
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                        @"SYSTEM\CurrentControlSet\Control\SystemInformation"))
                    {
                        if (key != null)
                        {
                            if (motherboardInfo.TryGetValue("SystemManufacturer", out var manufacturer))
                                key.SetValue("SystemManufacturer", manufacturer?.ToString() ?? string.Empty, RegistryValueKind.String);
                            
                            if (motherboardInfo.TryGetValue("SystemProductName", out var productName))
                                key.SetValue("SystemProductName", productName?.ToString() ?? string.Empty, RegistryValueKind.String);
                            
                            if (motherboardInfo.TryGetValue("BaseBoardProduct", out var baseBoard))
                                key.SetValue("BaseBoardProduct", baseBoard?.ToString() ?? string.Empty, RegistryValueKind.String);
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
                
                // Restore GPU
                Logger.Instance.Debug("Restoring GPU information");
                if (backupData.TryGetValue("GPU", out var gpuInfo))
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                        @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
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
                
                // Restore MAC Addresses
                Logger.Instance.Debug("Restoring MAC addresses");
                if (backupData.TryGetValue("MAC", out var macInfo))
                {
                    foreach (var entry in macInfo)
                    {
                        string deviceId = entry.Key;
                        string mac = entry.Value?.ToString()?.Replace(":", "") ?? string.Empty;
                        
                        using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                            $@"SYSTEM\CurrentControlSet\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{deviceId}"))
                        {
                            if (key != null)
                            {
                                key.SetValue("NetworkAddress", mac, RegistryValueKind.String);
                                
                                // Disable and re-enable the network adapter to apply changes
                                using (Process process = new Process())
                                {
                                    process.StartInfo.FileName = "netsh";
                                    process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" disabled";
                                    process.StartInfo.UseShellExecute = false;
                                    process.StartInfo.CreateNoWindow = true;
                                    process.Start();
                                    process.WaitForExit();
                                }
                                
                                Thread.Sleep(1000);
                                
                                using (Process process = new Process())
                                {
                                    process.StartInfo.FileName = "netsh";
                                    process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" enabled";
                                    process.StartInfo.UseShellExecute = false;
                                    process.StartInfo.CreateNoWindow = true;
                                    process.Start();
                                    process.WaitForExit();
                                }
                            }
                        }
                    }
                    
                    Console.WriteLine("MAC addresses restored successfully.");
                }
                
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
    }
} 