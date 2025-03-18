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
    /// Class responsible for collecting hardware information for backup
    /// </summary>
    public static class BackupCollector
    {
        private const string PROP_SERIAL_NUMBER = "SerialNumber";
        
        public static Dictionary<string, Dictionary<string, object>> CollectHardwareInfo()
        {
            var backupData = new Dictionary<string, Dictionary<string, object>>();
            
            backupData["CPU"] = CollectCPUInfo();
            backupData["Disk"] = CollectDiskInfo();
            backupData["Motherboard"] = CollectMotherboardInfo();
            backupData["GPU"] = CollectGPUInfo();
            backupData["MAC"] = CollectMACInfo();
            
            return backupData;
        }
        
        private static Dictionary<string, object> CollectCPUInfo()
        {
            Logger.Instance.Debug("Collecting CPU information");
            var cpuInfo = new Dictionary<string, object>();
            
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                var firstCpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstCpu != null && firstCpu[RegistryHelper.PROP_PROCESSOR_ID] != null)
                {
                    cpuInfo[RegistryHelper.PROP_PROCESSOR_ID] = firstCpu[RegistryHelper.PROP_PROCESSOR_ID]?.ToString() ?? string.Empty;
                }
            }
            
            return cpuInfo;
        }
        
        private static Dictionary<string, object> CollectDiskInfo()
        {
            Logger.Instance.Debug("Collecting Disk information");
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
        
        private static Dictionary<string, object> CollectMotherboardInfo()
        {
            Logger.Instance.Debug("Collecting Motherboard information");
            var motherboardInfo = new Dictionary<string, object>();
            
            CollectBaseBoardInfo(motherboardInfo);
            CollectBIOSInfo(motherboardInfo);
            CollectSystemInfo(motherboardInfo);
            CollectHardwareConfigInfo(motherboardInfo);
            
            return motherboardInfo;
        }
        
        private static void CollectBaseBoardInfo(Dictionary<string, object> motherboardInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                var firstMotherboard = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstMotherboard != null)
                {
                    if (firstMotherboard[PROP_SERIAL_NUMBER] != null)
                        motherboardInfo[PROP_SERIAL_NUMBER] = firstMotherboard[PROP_SERIAL_NUMBER]?.ToString() ?? string.Empty;
                    if (firstMotherboard["Manufacturer"] != null)
                        motherboardInfo["Manufacturer"] = firstMotherboard["Manufacturer"]?.ToString() ?? string.Empty;
                    if (firstMotherboard["Product"] != null)
                        motherboardInfo["Product"] = firstMotherboard["Product"]?.ToString() ?? string.Empty;
                }
            }
        }
        
        private static void CollectBIOSInfo(Dictionary<string, object> motherboardInfo)
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
        
        private static void CollectSystemInfo(Dictionary<string, object> motherboardInfo)
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
        
        private static void CollectHardwareConfigInfo(Dictionary<string, object> motherboardInfo)
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
        
        private static Dictionary<string, object> CollectGPUInfo()
        {
            var gpuInfo = new Dictionary<string, object>();
            
            CollectVideoControllerInfo(gpuInfo);
            CollectGPURegistryInfo(gpuInfo);
            
            return gpuInfo;
        }
        
        private static void CollectVideoControllerInfo(Dictionary<string, object> gpuInfo)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                var firstGpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (firstGpu != null)
                {
                    if (firstGpu["Name"] != null)
                        gpuInfo["Name"] = firstGpu["Name"]?.ToString() ?? string.Empty;
                    if (firstGpu["AdapterRAM"] != null)
                        gpuInfo["AdapterRAM"] = firstGpu["AdapterRAM"]?.ToString() ?? string.Empty;
                }
            }
        }
        
        private static void CollectGPURegistryInfo(Dictionary<string, object> gpuInfo)
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
        
        private static Dictionary<string, object> CollectMACInfo()
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
                
                // Log when no adapter with matching description is found
                Logger.Instance.Debug($"No network adapter found with description: {adapterDescription}");
            }
            catch (Exception ex)
            {
                // Log the exception instead of silently ignoring it
                Logger.Instance.LogException(ex, $"Error retrieving network adapter ID for: {adapterDescription}");
            }
            
            return string.Empty;
        }
    }
} 