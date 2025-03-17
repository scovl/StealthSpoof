using System;
using Microsoft.Win32;
using System.Linq;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Spoofers
{
    /// <summary>
    /// Class responsible for disk spoofing
    /// </summary>
    public static class DiskSpoofer
    {
        /// <summary>
        /// Performs disk spoofing
        /// </summary>
        public static void SpoofDisk()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
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
        
        /// <summary>
        /// Performs advanced disk information spoofing
        /// </summary>
        public static void SpoofAdvancedDiskInfo()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting Advanced Disk Info spoofing");
            Console.WriteLine("\nSpoofing Advanced Disk Information...");
            
            try
            {
                // Spoof SCSI information
                using (RegistryKey? scsiPorts = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_SCSI, true))
                {
                    if (scsiPorts == null) return;
                    
                    // Iterate through SCSI ports and modify disk identifiers
                    foreach (string portName in scsiPorts.GetSubKeyNames())
                    {
                        ModifyScsiPortEntries(scsiPorts, portName);
                    }
                }
                
                // Helper method to process SCSI port entries
                static void ModifyScsiPortEntries(RegistryKey parentKey, string portName)
                {
                    using RegistryKey? port = parentKey.OpenSubKey(portName, true);
                    if (port == null) return;
                    
                    var scsiKeys = port.GetSubKeyNames().Where(name => name.StartsWith("Scsi Bus"));
                    foreach (string busName in scsiKeys)
                    {
                        ModifyBusEntries(port, busName);
                    }
                }
                
                // Helper method to process bus entries
                static void ModifyBusEntries(RegistryKey parentKey, string busName)
                {
                    using RegistryKey? bus = parentKey.OpenSubKey(busName, true);
                    if (bus == null) return;
                    
                    foreach (string targetName in bus.GetSubKeyNames())
                    {
                        ModifyTargetEntries(bus, targetName);
                    }
                }
                
                // Helper method to process target entries
                static void ModifyTargetEntries(RegistryKey parentKey, string targetName)
                {
                    using RegistryKey? target = parentKey.OpenSubKey(targetName, true);
                    if (target == null) return;
                    
                    foreach (string lun in target.GetSubKeyNames())
                    {
                        using RegistryKey? lunKey = target.OpenSubKey(lun, true);
                        if (lunKey == null) continue;
                        
                        // Update the disk identifiers with random values
                        lunKey.SetValue("Identifier", HardwareInfo.GetRandomHardwareID(20), RegistryValueKind.String);
                        lunKey.SetValue("SerialNumber", HardwareInfo.GetRandomHardwareID(16), RegistryValueKind.String);
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
    }
} 