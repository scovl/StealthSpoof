using System;
using Microsoft.Win32;
using System.Security.Principal;

namespace StealthSpoof.Core.Utils
{
    /// <summary>
    /// Utility class for registry operations
    /// </summary>
    public static class RegistryHelper
    {
        // Registry path constants
        public const string REG_PATH_GPU = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
        public const string REG_PATH_NETWORK_ADAPTERS = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
        public const string REG_PATH_WINDOWS_NT = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        public const string REG_PATH_COMPUTER_NAME = @"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName";
        public const string REG_PATH_ACTIVE_COMPUTER_NAME = @"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName";
        public const string REG_PATH_TCPIP_PARAMETERS = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
        public const string REG_PATH_TCPIP_INTERFACES = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
        public const string REG_PATH_SCSI = @"HARDWARE\DEVICEMAP\Scsi";
        public const string REG_PATH_DISK_PERIPHERALS = @"HARDWARE\DEVICEMAP\Scsi\Scsi Port %d\Scsi Bus %d\Target Id %d\Logical Unit Id %d";
        public const string REG_PATH_HARDWARE_PROFILES = @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001";
        public const string REG_PATH_CRYPTOGRAPHY = @"SOFTWARE\Microsoft\Cryptography";
        public const string REG_PATH_SQM_CLIENT = @"SOFTWARE\Microsoft\SQMClient";
        public const string REG_PATH_SYSTEM_INFO = @"SYSTEM\CurrentControlSet\Control\SystemInformation";
        public const string REG_PATH_WINDOWS_UPDATE = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate";
        public const string REG_PATH_BIOS = @"HARDWARE\DESCRIPTION\System\BIOS";
        public const string REG_PATH_EFI_VARIABLES = @"SYSTEM\CurrentControlSet\Control\EFI\Variables";
        public const string REG_PATH_DISK_ENUM = @"SYSTEM\CurrentControlSet\Services\disk\Enum";
        public const string REG_PATH_CPU = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
        
        // Property name constants
        public const string PROP_PROCESSOR_ID = "ProcessorId";
        public const string PROP_SYSTEM_MANUFACTURER = "SystemManufacturer";
        public const string PROP_SYSTEM_PRODUCT_NAME = "SystemProductName";
        public const string PROP_BASEBOARD_PRODUCT = "BaseBoardProduct";
        public const string PROP_HARDWARE_ID = "HardwareID";
        public const string PROP_MEMORY_SIZE = "HardwareInformation.qwMemorySize";
        public const string PROP_BASEBOARD_SERIAL = "BaseBoardSerialNumber";
        public const string PROP_BIOS_SERIAL = "BIOSSerialNumber";
        
        /// <summary>
        /// Checks if the user has administrator privileges
        /// </summary>
        /// <returns>True if the user is an administrator, False otherwise</returns>
        public static bool RequireAdminCheck()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
                
            if (!isAdmin)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This application requires administrator privileges.");
                Console.WriteLine("Please restart the program as an administrator.");
                Console.ResetColor();
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if a registry key exists
        /// </summary>
        /// <param name="keyPath">Registry key path</param>
        /// <returns>True if the key exists, False otherwise</returns>
        public static bool CheckRegistryKey(string keyPath)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets a registry value
        /// </summary>
        /// <param name="keyPath">Registry key path</param>
        /// <param name="valueName">Value name</param>
        /// <returns>The registry value or null if it does not exist</returns>
        public static object? GetRegistryValue(string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        return key.GetValue(valueName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error getting registry value {keyPath}\\{valueName}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Sets a registry value
        /// </summary>
        /// <param name="keyPath">Registry key path</param>
        /// <param name="valueName">Value name</param>
        /// <param name="value">Value to be set</param>
        /// <param name="valueKind">Value type</param>
        /// <returns>True if the operation is successful, False otherwise</returns>
        public static bool SetRegistryValue(string keyPath, string valueName, object value, RegistryValueKind valueKind)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    key.SetValue(valueName, value, valueKind);
                    Logger.Instance.LogRegistryOperation(keyPath, valueName, "MODIFY");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error setting registry value {keyPath}\\{valueName}");
                return false;
            }
        }
        
        /// <summary>
        /// Displays the status of a registry key
        /// </summary>
        /// <param name="keyPath">Registry key path</param>
        /// <param name="keyName">Friendly name of the key</param>
        public static void DisplayRegistryKeyStatus(string keyPath, string keyName)
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
                Console.WriteLine($"✗ {keyName}: Erro - {ex.Message}");
                Console.ResetColor();
            }
        }
    }
} 