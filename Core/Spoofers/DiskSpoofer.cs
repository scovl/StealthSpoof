using System;
using Microsoft.Win32;
using System.Linq;
using StealthSpoof.Core.Utils;
using System.Security;

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
            if (!CheckAdminPrivileges())
                return;
            
            Logger.Instance.Info("Starting disk spoofing process");
            Console.WriteLine("\nSpoofing Disks...");
            
            try
            {
                string newDiskId = HardwareInfo.GetRandomHardwareID(20);
                Logger.Instance.Debug($"Generated new disk ID: {newDiskId}");
                
                var result = ModifyDiskRegistryEntries(newDiskId);
                DisplayResults(result.modifiedEntries, result.skippedEntries);
            }
            catch (SecurityException ex)
            {
                HandleSecurityException(ex);
            }
            catch (Exception ex)
            {
                HandleGenericException(ex);
            }
        }
        
        private static bool CheckAdminPrivileges()
        {
            if (!RegistryHelper.RequireAdminCheck())
            {
                Logger.Instance.Warning("Disk spoofing aborted - administrative privileges required");
                return false;
            }
            return true;
        }
        
        private static (int modifiedEntries, int skippedEntries) ModifyDiskRegistryEntries(string newDiskId)
        {
            int modifiedEntries = 0;
            int skippedEntries = 0;
            
            using var reg = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\disk\Enum");
            if (reg == null)
            {
                HandleRegistryAccessFailure();
                return (0, 0);
            }
            
            for (int i = 0; i < 10; i++)
            {
                ProcessRegistryEntry(reg, i.ToString(), newDiskId, ref modifiedEntries, ref skippedEntries);
            }
            
            Logger.Instance.Info($"Disk spoofing completed. Modified: {modifiedEntries}, Skipped: {skippedEntries}");
            return (modifiedEntries, skippedEntries);
        }
        
        private static void HandleRegistryAccessFailure()
        {
            Logger.Instance.Error("Failed to access disk enumeration registry key");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Could not access disk registry entries");
            Console.ResetColor();
        }
        
        private static void ProcessRegistryEntry(RegistryKey reg, string keyName, string newDiskId, ref int modifiedEntries, ref int skippedEntries)
        {
            try
            {
                object? value = reg.GetValue(keyName);
                
                if (value != null)
                {
                    string valueStr = value.ToString() ?? string.Empty;
                    string[] parts = valueStr.Split('&');
                    if (parts.Length > 1)
                    {
                        ModifyRegistryValue(reg, keyName, valueStr, parts, newDiskId);
                        modifiedEntries++;
                    }
                    else
                    {
                        Logger.Instance.Debug($"Skipped disk entry {keyName}: invalid format - {valueStr}");
                        skippedEntries++;
                    }
                }
                else
                {
                    // This is normal, not all indices will have values
                    Logger.Instance.Debug($"No value found for disk entry {keyName}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                HandleUnauthorizedAccessException(ex, keyName);
                skippedEntries++;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error processing disk entry {keyName}");
                skippedEntries++;
            }
        }
        
        private static void ModifyRegistryValue(RegistryKey reg, string keyName, string valueStr, string[] parts, string newDiskId)
        {
            string oldValue = valueStr;
            string newValue = valueStr.Replace(parts[1], $"&{newDiskId}");
            reg.SetValue(keyName, newValue);
            
            Logger.Instance.Debug($"Modified disk entry {keyName}: {oldValue} -> {newValue}");
        }
        
        private static void HandleUnauthorizedAccessException(UnauthorizedAccessException ex, string entry)
        {
            Logger.Instance.LogException(ex, $"Access denied while modifying disk entry {entry}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: Access denied to disk entry {entry}");
            Console.ResetColor();
        }
        
        private static void HandleSecurityException(SecurityException ex)
        {
            Logger.Instance.LogException(ex, "Security error during disk spoofing");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Access denied: {ex.Message}");
            Console.WriteLine("Try running the application as administrator.");
            Console.ResetColor();
        }
        
        private static void HandleGenericException(Exception ex)
        {
            Logger.Instance.LogException(ex, "Error spoofing disks");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error spoofing disks: {ex.Message}");
            Console.ResetColor();
        }
        
        private static void DisplayResults(int modifiedEntries, int skippedEntries)
        {
            if (modifiedEntries > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Disks modified successfully ({modifiedEntries} entries). Restart the computer to apply.");
                
                if (skippedEntries > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Note: {skippedEntries} entries were skipped due to format issues or access restrictions.");
                }
                
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No disk entries were modified. System may not have standard disk identifiers.");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Performs advanced disk information spoofing
        /// </summary>
        public static void SpoofAdvancedDiskInfo()
        {
            if (!CheckAdminPrivileges()) return;
            
            Logger.Instance.Info("Starting Advanced Disk Info spoofing");
            Console.WriteLine("\nSpoofing Advanced Disk Information...");
            
            try
            {
                SpoofScsiInformation();
                
                Logger.Instance.Info("Advanced Disk Info spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Advanced Disk Information modified successfully.");
                Console.WriteLine("Note: A system restart is required for these changes to take full effect.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                HandleAdvancedDiskInfoException(ex);
            }
        }
        
        private static void SpoofScsiInformation()
        {
            using RegistryKey? scsiPorts = Registry.LocalMachine.OpenSubKey(RegistryHelper.REG_PATH_SCSI, true);
            if (scsiPorts == null)
            {
                Logger.Instance.Warning("Could not access SCSI registry path");
                return;
            }
            
            // Iterate through SCSI ports and modify disk identifiers
            foreach (string portName in scsiPorts.GetSubKeyNames())
            {
                ProcessScsiPort(scsiPorts, portName);
            }
        }
        
        private static void ProcessScsiPort(RegistryKey parentKey, string portName)
        {
            using RegistryKey? port = parentKey.OpenSubKey(portName, true);
            if (port == null) return;
            
            var scsiKeys = port.GetSubKeyNames().Where(name => name.StartsWith("Scsi Bus"));
            foreach (string busName in scsiKeys)
            {
                ProcessScsiPortBus(port, busName);
            }
        }
        
        private static void ProcessScsiPortBus(RegistryKey parentKey, string busName)
        {
            using RegistryKey? bus = parentKey.OpenSubKey(busName, true);
            if (bus == null) return;
            
            foreach (string targetName in bus.GetSubKeyNames())
            {
                ProcessScsiPortBusTarget(bus, targetName);
            }
        }
        
        private static void ProcessScsiPortBusTarget(RegistryKey parentKey, string targetName)
        {
            using RegistryKey? target = parentKey.OpenSubKey(targetName, true);
            if (target == null) return;
            
            foreach (string lun in target.GetSubKeyNames())
            {
                UpdateDiskIdentifiers(target, lun);
            }
        }
        
        private static void UpdateDiskIdentifiers(RegistryKey parentKey, string lun)
        {
            using RegistryKey? lunKey = parentKey.OpenSubKey(lun, true);
            if (lunKey == null) return;
            
            // Update the disk identifiers with random values
            string newIdentifier = HardwareInfo.GetRandomHardwareID(20);
            string newSerialNumber = HardwareInfo.GetRandomHardwareID(16);
            
            lunKey.SetValue("Identifier", newIdentifier, RegistryValueKind.String);
            lunKey.SetValue("SerialNumber", newSerialNumber, RegistryValueKind.String);
            
            Logger.Instance.Debug($"Updated disk identifiers for LUN {lun}: ID={newIdentifier}, S/N={newSerialNumber}");
        }
        
        private static void HandleAdvancedDiskInfoException(Exception ex)
        {
            Logger.Instance.LogException(ex, "Error spoofing Advanced Disk Info");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error spoofing Advanced Disk Info: {ex.Message}");
            Console.ResetColor();
        }
    }
} 