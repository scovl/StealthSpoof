using System;
using Microsoft.Win32;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Spoofers
{
    /// <summary>
    /// Class responsible for motherboard spoofing
    /// </summary>
    public static class MotherboardSpoofer
    {
        /// <summary>
        /// Performs motherboard spoofing
        /// </summary>
        public static void SpoofMotherboard()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing Motherboard and BIOS...");
            
            try
            {
                string newSerialNumber = HardwareInfo.GetRandomHardwareID(16);
                string newUUID = Guid.NewGuid().ToString().ToUpper();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_SYSTEM_INFO))
                {
                    if (key != null)
                    {
                        key.SetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, "SpooferBIOS", RegistryValueKind.String);
                        key.SetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, "StealthBoard", RegistryValueKind.String);
                        key.SetValue(RegistryHelper.PROP_BASEBOARD_PRODUCT, "SB-" + newSerialNumber, RegistryValueKind.String);
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
        
        /// <summary>
        /// Performs EFI Variable ID spoofing
        /// </summary>
        public static void SpoofEFIVariableId()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting EFI Variable ID spoofing");
            Console.WriteLine("\nSpoofing EFI Variable ID...");
            
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_BIOS))
                {
                    if (key != null)
                    {
                        key.SetValue(RegistryHelper.PROP_SYSTEM_MANUFACTURER, "StealthSpoof BIOS", RegistryValueKind.String);
                        key.SetValue(RegistryHelper.PROP_SYSTEM_PRODUCT_NAME, "StealthSpoof System", RegistryValueKind.String);
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
        
        /// <summary>
        /// Performs SMBIOS Serial Number spoofing
        /// </summary>
        public static void SpoofSMBIOSSerialNumber()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting SMBIOS Serial Number spoofing");
            Console.WriteLine("\nSpoofing SMBIOS Serial Number...");
            
            try
            {
                string newSerialNumber = HardwareInfo.GetRandomHardwareID(16);
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_BIOS))
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
    }
} 