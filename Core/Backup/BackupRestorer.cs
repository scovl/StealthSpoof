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
        public static void RestoreHardwareInfo(Dictionary<string, Dictionary<string, object>> backupData)
        {
            if (backupData == null)
            {
                throw new ArgumentNullException(nameof(backupData));
            }
            
            RestoreCPUInfo(backupData.GetValueOrDefault("CPU"));
            RestoreDiskInfo(backupData.GetValueOrDefault("Disk"));
            RestoreMotherboardInfo(backupData.GetValueOrDefault("Motherboard"));
            RestoreGPUInfo(backupData.GetValueOrDefault("GPU"));
            RestoreMACInfo(backupData.GetValueOrDefault("MAC"));
        }
        
        private static void RestoreCPUInfo(Dictionary<string, object>? cpuInfo)
        {
            if (cpuInfo == null) return;
            
            Logger.Instance.Debug("Restoring CPU information");
            
            if (cpuInfo.TryGetValue(RegistryHelper.PROP_PROCESSOR_ID, out object? processorId) && processorId != null)
            {
                RegistryHelper.SetRegistryValue(
                    RegistryHelper.REG_PATH_CPU,
                    RegistryHelper.PROP_PROCESSOR_ID,
                    processorId.ToString() ?? string.Empty,
                    RegistryValueKind.String
                );
            }
        }
        
        private static void RestoreDiskInfo(Dictionary<string, object>? diskInfo)
        {
            if (diskInfo == null) return;
            
            Logger.Instance.Debug("Restoring Disk information");
            
            using (var reg = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_DISK_ENUM, true))
            {
                if (reg != null)
                {
                    foreach (var kvp in diskInfo)
                    {
                        reg.SetValue(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
                    }
                }
            }
        }
        
        private static void RestoreMotherboardInfo(Dictionary<string, object>? motherboardInfo)
        {
            if (motherboardInfo == null) return;
            
            Logger.Instance.Debug("Restoring Motherboard information");
            
            RestoreBaseBoardInfo(motherboardInfo);
            RestoreBIOSInfo(motherboardInfo);
            RestoreSystemInfo(motherboardInfo);
            RestoreHardwareConfigInfo(motherboardInfo);
        }
        
        private static void RestoreBaseBoardInfo(Dictionary<string, object> motherboardInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                var firstMotherboard = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstMotherboard != null)
                {
                    if (motherboardInfo.TryGetValue("SerialNumber", out object? serialNumber) && serialNumber != null)
                    {
                        RegistryHelper.SetRegistryValue(
                            RegistryHelper.REG_PATH_SYSTEM_INFO,
                            RegistryHelper.PROP_BASEBOARD_SERIAL,
                            serialNumber.ToString() ?? string.Empty,
                            RegistryValueKind.String
                        );
                    }
                    
                    if (motherboardInfo.TryGetValue("Manufacturer", out object? manufacturer) && manufacturer != null)
                    {
                        RegistryHelper.SetRegistryValue(
                            RegistryHelper.REG_PATH_SYSTEM_INFO,
                            RegistryHelper.PROP_SYSTEM_MANUFACTURER,
                            manufacturer.ToString() ?? string.Empty,
                            RegistryValueKind.String
                        );
                    }
                    
                    if (motherboardInfo.TryGetValue("Product", out object? product) && product != null)
                    {
                        RegistryHelper.SetRegistryValue(
                            RegistryHelper.REG_PATH_SYSTEM_INFO,
                            RegistryHelper.PROP_BASEBOARD_PRODUCT,
                            product.ToString() ?? string.Empty,
                            RegistryValueKind.String
                        );
                    }
                }
            }
        }
        
        private static void RestoreBIOSInfo(Dictionary<string, object> motherboardInfo)
        {
            if (motherboardInfo.TryGetValue("BIOSSerial", out object? biosSerial) && biosSerial != null)
            {
                RegistryHelper.SetRegistryValue(
                    RegistryHelper.REG_PATH_SYSTEM_INFO,
                    RegistryHelper.PROP_BIOS_SERIAL,
                    biosSerial.ToString() ?? string.Empty,
                    RegistryValueKind.String
                );
            }
        }
        
        private static void RestoreSystemInfo(Dictionary<string, object> motherboardInfo)
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_SYSTEM_INFO, true))
            {
                if (key != null)
                {
                    if (motherboardInfo.TryGetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, out object? manufacturer))
                    {
                        key.SetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, manufacturer?.ToString() ?? string.Empty);
                    }
                    
                    if (motherboardInfo.TryGetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, out object? productName))
                    {
                        key.SetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, productName?.ToString() ?? string.Empty);
                    }
                    
                    if (motherboardInfo.TryGetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, out object? baseBoard))
                    {
                        key.SetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, baseBoard?.ToString() ?? string.Empty);
                    }
                }
            }
        }
        
        private static void RestoreHardwareConfigInfo(Dictionary<string, object> motherboardInfo)
        {
            if (motherboardInfo.TryGetValue("UUID", out object? uuid))
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\HardwareConfig", true))
                {
                    if (key != null)
                    {
                        key.SetValue("LastConfig", uuid?.ToString() ?? string.Empty);
                    }
                }
            }
        }
        
        private static void RestoreGPUInfo(Dictionary<string, object>? gpuInfo)
        {
            if (gpuInfo == null) return;
            
            Logger.Instance.Debug("Restoring GPU information");
            
            RestoreVideoControllerInfo(gpuInfo);
            RestoreGPURegistryInfo(gpuInfo);
        }
        
        private static void RestoreVideoControllerInfo(Dictionary<string, object> gpuInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                var firstGpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstGpu != null)
                {
                    if (gpuInfo.TryGetValue("Name", out object? name) && name != null)
                    {
                        RegistryHelper.SetRegistryValue(
                            RegistryHelper.REG_PATH_GPU,
                            "DriverDesc",
                            name.ToString() ?? string.Empty,
                            RegistryValueKind.String
                        );
                    }
                    
                    if (gpuInfo.TryGetValue("AdapterRAM", out object? adapterRam) && adapterRam != null)
                    {
                        RegistryHelper.SetRegistryValue(
                            RegistryHelper.REG_PATH_GPU,
                            RegistryHelper.PROP_MEMORY_SIZE,
                            adapterRam.ToString() ?? string.Empty,
                            RegistryValueKind.String
                        );
                    }
                }
            }
        }
        
        private static void RestoreGPURegistryInfo(Dictionary<string, object> gpuInfo)
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_GPU, true))
            {
                if (key != null)
                {
                    if (gpuInfo.TryGetValue(RegistryHelper.PROP_HARDWARE_ID, out object? hardwareId))
                    {
                        key.SetValue(RegistryHelper.PROP_HARDWARE_ID, hardwareId?.ToString() ?? string.Empty);
                    }
                    
                    if (gpuInfo.TryGetValue("MemorySize", out object? memorySize))
                    {
                        key.SetValue(RegistryHelper.PROP_MEMORY_SIZE, memorySize?.ToString() ?? string.Empty);
                    }
                }
            }
        }
        
        private static void RestoreMACInfo(Dictionary<string, object>? macInfo)
        {
            if (macInfo == null) return;
            
            Logger.Instance.Debug("Restoring MAC information");
            
            foreach (var kvp in macInfo)
            {
                string adapterId = kvp.Key;
                string macAddress = kvp.Value?.ToString() ?? string.Empty;
                
                if (!string.IsNullOrEmpty(macAddress))
                {
                    RegistryHelper.SetRegistryValue(
                        $@"SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}\{adapterId}",
                        "NetworkAddress",
                        macAddress.Replace(":", ""),
                        RegistryValueKind.String
                    );
                }
            }
        }
    }
} 