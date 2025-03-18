using System;
using System.Management;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Linq;
using StealthSpoof.Core;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Class responsible for restoring hardware information from backup
    /// </summary>
    public static class BackupRestorer
    {
        /// <summary>
        /// Restores hardware information from backup
        /// </summary>
        /// <param name="backupData">Dictionary containing backup data</param>
        /// <returns>Number of items successfully restored</returns>
        public static int RestoreHardwareInfo(Dictionary<string, Dictionary<string, object>> backupData)
        {
            if (backupData == null)
            {
                throw new ArgumentNullException(nameof(backupData));
            }
            
            int restoredCount = 0;
            
            Logger.Instance.Info("Starting hardware information restoration");
            
            try
            {
                restoredCount += RestoreCPUInfo(backupData.GetValueOrDefault("CPU"));
                restoredCount += RestoreDiskInfo(backupData.GetValueOrDefault("Disk"));
                restoredCount += RestoreMotherboardInfo(backupData.GetValueOrDefault("Motherboard"));
                restoredCount += RestoreGPUInfo(backupData.GetValueOrDefault("GPU"));
                restoredCount += RestoreMACInfo(backupData.GetValueOrDefault("MAC"));
                
                Logger.Instance.Info($"Restored {restoredCount} hardware information items");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error during hardware restoration");
            }
            
            return restoredCount;
        }
        
        private static int RestoreCPUInfo(Dictionary<string, object>? cpuInfo)
        {
            if (cpuInfo == null) return 0;
            
            Logger.Instance.Debug("Restoring CPU information");
            int restoredCount = 0;
            
            try
            {
                if (cpuInfo.TryGetValue(RegistryHelper.PROP_PROCESSOR_ID, out object? processorId) && processorId != null)
                {
                    bool success = RegistryHelper.SetRegistryValue(
                        RegistryHelper.REG_PATH_CPU,
                        RegistryHelper.PROP_PROCESSOR_ID,
                        processorId.ToString() ?? string.Empty,
                        RegistryValueKind.String
                    );
                    
                    if (success)
                    {
                        Logger.Instance.Debug($"Restored CPU processor ID: {processorId}");
                        restoredCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring CPU information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreDiskInfo(Dictionary<string, object>? diskInfo)
        {
            if (diskInfo == null) return 0;
            
            Logger.Instance.Debug("Restoring Disk information");
            int restoredCount = 0;
            
            try
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_DISK_ENUM, true))
                {
                    if (reg != null)
                    {
                        foreach (var kvp in diskInfo)
                        {
                            try
                            {
                                reg.SetValue(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
                                Logger.Instance.Debug($"Restored disk info: {kvp.Key} = {kvp.Value}");
                                restoredCount++;
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.LogException(ex, $"Failed to restore disk info key: {kvp.Key}");
                            }
                        }
                    }
                    else
                    {
                        Logger.Instance.Warning("Could not open disk registry key for writing");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring disk information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreMotherboardInfo(Dictionary<string, object>? motherboardInfo)
        {
            if (motherboardInfo == null) return 0;
            
            Logger.Instance.Debug("Restoring Motherboard information");
            int restoredCount = 0;
            
            try
            {
                restoredCount += RestoreBaseBoardInfo(motherboardInfo);
                restoredCount += RestoreBIOSInfo(motherboardInfo);
                restoredCount += RestoreSystemInfo(motherboardInfo);
                restoredCount += RestoreHardwareConfigInfo(motherboardInfo);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error in motherboard restoration process");
            }
            
            return restoredCount;
        }
        
        private static int RestoreBaseBoardInfo(Dictionary<string, object> motherboardInfo)
        {
            int restoredCount = 0;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                var firstMotherboard = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                
                if (firstMotherboard == null)
                {
                    Logger.Instance.Warning("No motherboard found to restore");
                    return 0;
                }
                
                restoredCount += RestoreBaseBoardSerialNumber(motherboardInfo);
                restoredCount += RestoreManufacturer(motherboardInfo);
                restoredCount += RestoreBaseBoardProduct(motherboardInfo);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring baseboard information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreBaseBoardSerialNumber(Dictionary<string, object> motherboardInfo)
        {
            if (!motherboardInfo.TryGetValue("SerialNumber", out object? serialNumber) || serialNumber == null)
                return 0;
                
            bool success = RegistryHelper.SetRegistryValue(
                RegistryHelper.REG_PATH_SYSTEM_INFO,
                RegistryHelper.PROP_BASEBOARD_SERIAL,
                serialNumber.ToString() ?? string.Empty,
                RegistryValueKind.String
            );
            
            if (success)
            {
                Logger.Instance.Debug($"Restored baseboard serial: {serialNumber}");
                return 1;
            }
            
            return 0;
        }
        
        private static int RestoreManufacturer(Dictionary<string, object> motherboardInfo)
        {
            if (!motherboardInfo.TryGetValue("Manufacturer", out object? manufacturer) || manufacturer == null)
                return 0;
                
            bool success = RegistryHelper.SetRegistryValue(
                RegistryHelper.REG_PATH_SYSTEM_INFO,
                RegistryHelper.PROP_SYSTEM_MANUFACTURER,
                manufacturer.ToString() ?? string.Empty,
                RegistryValueKind.String
            );
            
            if (success)
            {
                Logger.Instance.Debug($"Restored manufacturer: {manufacturer}");
                return 1;
            }
            
            return 0;
        }
        
        private static int RestoreBaseBoardProduct(Dictionary<string, object> motherboardInfo)
        {
            if (!motherboardInfo.TryGetValue("Product", out object? product) || product == null)
                return 0;
                
            bool success = RegistryHelper.SetRegistryValue(
                RegistryHelper.REG_PATH_SYSTEM_INFO,
                RegistryHelper.PROP_BASEBOARD_PRODUCT,
                product.ToString() ?? string.Empty,
                RegistryValueKind.String
            );
            
            if (success)
            {
                Logger.Instance.Debug($"Restored baseboard product: {product}");
                return 1;
            }
            
            return 0;
        }
        
        private static int RestoreBIOSInfo(Dictionary<string, object> motherboardInfo)
        {
            int restoredCount = 0;
            
            try
            {
                if (motherboardInfo.TryGetValue("BIOSSerial", out object? biosSerial) && biosSerial != null)
                {
                    bool success = RegistryHelper.SetRegistryValue(
                        RegistryHelper.REG_PATH_SYSTEM_INFO,
                        RegistryHelper.PROP_BIOS_SERIAL,
                        biosSerial.ToString() ?? string.Empty,
                        RegistryValueKind.String
                    );
                    
                    if (success)
                    {
                        Logger.Instance.Debug($"Restored BIOS serial: {biosSerial}");
                        restoredCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring BIOS information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreSystemInfo(Dictionary<string, object> motherboardInfo)
        {
            int restoredCount = 0;
            
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_SYSTEM_INFO, true);
                if (key == null)
                {
                    Logger.Instance.Warning("Could not open system info registry key for writing");
                    return 0;
                }
                
                restoredCount += RestoreSystemManufacturer(key, motherboardInfo);
                restoredCount += RestoreSystemProductName(key, motherboardInfo);
                restoredCount += RestoreSystemBaseBoardProduct(key, motherboardInfo);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring system information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreSystemManufacturer(RegistryKey key, Dictionary<string, object> motherboardInfo)
        {
            if (!motherboardInfo.TryGetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, out object? manufacturer))
                return 0;
                
            string oldValue = key.GetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER)?.ToString() ?? string.Empty;
            key.SetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, manufacturer?.ToString() ?? string.Empty);
            Logger.Instance.Debug($"Restored system manufacturer: {oldValue} -> {manufacturer}");
            return 1;
        }
        
        private static int RestoreSystemProductName(RegistryKey key, Dictionary<string, object> motherboardInfo)
        {
            if (!motherboardInfo.TryGetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, out object? productName))
                return 0;
                
            string oldValue = key.GetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME)?.ToString() ?? string.Empty;
            key.SetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, productName?.ToString() ?? string.Empty);
            Logger.Instance.Debug($"Restored system product name: {oldValue} -> {productName}");
            return 1;
        }
        
        private static int RestoreSystemBaseBoardProduct(RegistryKey key, Dictionary<string, object> motherboardInfo)
        {
            if (!motherboardInfo.TryGetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, out object? baseBoard))
                return 0;
                
            string oldValue = key.GetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT)?.ToString() ?? string.Empty;
            key.SetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, baseBoard?.ToString() ?? string.Empty);
            Logger.Instance.Debug($"Restored baseboard product: {oldValue} -> {baseBoard}");
            return 1;
        }
        
        private static int RestoreHardwareConfigInfo(Dictionary<string, object> motherboardInfo)
        {
            int restoredCount = 0;
            
            try
            {
                if (motherboardInfo.TryGetValue("UUID", out object? uuid))
                {
                    using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\HardwareConfig", true))
                    {
                        if (key != null)
                        {
                            string oldValue = key.GetValue("LastConfig")?.ToString() ?? string.Empty;
                            key.SetValue("LastConfig", uuid?.ToString() ?? string.Empty);
                            Logger.Instance.Debug($"Restored hardware config UUID: {oldValue} -> {uuid}");
                            restoredCount++;
                        }
                        else
                        {
                            Logger.Instance.Warning("Could not open hardware config registry key for writing");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring hardware config information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreGPUInfo(Dictionary<string, object>? gpuInfo)
        {
            if (gpuInfo == null) return 0;
            
            Logger.Instance.Debug("Restoring GPU information");
            int restoredCount = 0;
            
            try
            {
                restoredCount += RestoreVideoControllerInfo(gpuInfo);
                restoredCount += RestoreGPURegistryInfo(gpuInfo);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error in GPU restoration process");
            }
            
            return restoredCount;
        }
        
        private static int RestoreVideoControllerInfo(Dictionary<string, object> gpuInfo)
        {
            int restoredCount = 0;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                var firstGpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                
                if (firstGpu == null)
                {
                    Logger.Instance.Warning("No GPU found to restore");
                    return 0;
                }
                
                restoredCount += RestoreGpuDriverDescription(gpuInfo);
                restoredCount += RestoreGpuAdapterRam(gpuInfo);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring video controller information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreGpuDriverDescription(Dictionary<string, object> gpuInfo)
        {
            if (!gpuInfo.TryGetValue("Name", out object? name) || name == null)
                return 0;
                
            bool success = RegistryHelper.SetRegistryValue(
                RegistryHelper.REG_PATH_GPU,
                "DriverDesc",
                name.ToString() ?? string.Empty,
                RegistryValueKind.String
            );
            
            if (success)
            {
                Logger.Instance.Debug($"Restored GPU driver description: {name}");
                return 1;
            }
            
            return 0;
        }
        
        private static int RestoreGpuAdapterRam(Dictionary<string, object> gpuInfo)
        {
            if (!gpuInfo.TryGetValue("AdapterRAM", out object? adapterRam) || adapterRam == null)
                return 0;
                
            bool success = RegistryHelper.SetRegistryValue(
                RegistryHelper.REG_PATH_GPU,
                RegistryHelper.PROP_MEMORY_SIZE,
                adapterRam.ToString() ?? string.Empty,
                RegistryValueKind.String
            );
            
            if (success)
            {
                Logger.Instance.Debug($"Restored GPU adapter RAM: {adapterRam}");
                return 1;
            }
            
            return 0;
        }
        
        private static int RestoreGPURegistryInfo(Dictionary<string, object> gpuInfo)
        {
            int restoredCount = 0;
            
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_GPU, true);
                if (key == null)
                {
                    Logger.Instance.Warning("Could not open GPU registry key for writing");
                    return 0;
                }
                
                restoredCount += RestoreGpuHardwareId(key, gpuInfo);
                restoredCount += RestoreGpuMemorySize(key, gpuInfo);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring GPU registry information");
            }
            
            return restoredCount;
        }
        
        private static int RestoreGpuHardwareId(RegistryKey key, Dictionary<string, object> gpuInfo)
        {
            if (!gpuInfo.TryGetValue(RegistryHelper.PROP_HARDWARE_ID, out object? hardwareId))
                return 0;
                
            string oldValue = key.GetValue(RegistryHelper.PROP_HARDWARE_ID)?.ToString() ?? string.Empty;
            key.SetValue(RegistryHelper.PROP_HARDWARE_ID, hardwareId?.ToString() ?? string.Empty);
            Logger.Instance.Debug($"Restored GPU hardware ID: {oldValue} -> {hardwareId}");
            return 1;
        }
        
        private static int RestoreGpuMemorySize(RegistryKey key, Dictionary<string, object> gpuInfo)
        {
            if (!gpuInfo.TryGetValue("MemorySize", out object? memorySize))
                return 0;
                
            string oldValue = key.GetValue(RegistryHelper.PROP_MEMORY_SIZE)?.ToString() ?? string.Empty;
            key.SetValue(RegistryHelper.PROP_MEMORY_SIZE, memorySize?.ToString() ?? string.Empty);
            Logger.Instance.Debug($"Restored GPU memory size: {oldValue} -> {memorySize}");
            return 1;
        }
        
        private static int RestoreMACInfo(Dictionary<string, object>? macInfo)
        {
            if (macInfo == null) return 0;
            
            Logger.Instance.Debug("Restoring MAC information");
            int restoredCount = 0;
            
            try
            {
                foreach (var kvp in macInfo)
                {
                    string adapterId = kvp.Key;
                    string macAddress = kvp.Value?.ToString() ?? string.Empty;
                    
                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        bool success = RegistryHelper.SetRegistryValue(
                            $@"SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}\{adapterId}",
                            "NetworkAddress",
                            macAddress.Replace(":", ""),
                            RegistryValueKind.String
                        );
                        
                        if (success)
                        {
                            Logger.Instance.Debug($"Restored MAC address for adapter {adapterId}: {macAddress}");
                            restoredCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error restoring MAC information");
            }
            
            return restoredCount;
        }
    }
} 