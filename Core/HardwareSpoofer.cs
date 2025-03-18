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
using StealthSpoof.Core.Utils;
using StealthSpoof.Core.Backup;
using StealthSpoof.Core.Spoofers;

/*
    This class is responsible for spoofing the hardware of the computer.
    It is used to hide the original hardware of the computer from the software.
    It is also used to spoof the hardware of the computer to avoid detection by antivirus software.
*/

namespace StealthSpoof.Core
{
    /// <summary>
    /// Classe principal para spoofing de hardware
    /// </summary>
    public static class HardwareSpoofer
    {
        /// <summary>
        /// Realizes the spoofing of all hardware and software components
        /// </summary>
        public static void SpoofAll()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting complete system spoofing process");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nStarting complete system spoofing process...");
            Console.ResetColor();
            
            try
            {
                BackupManager.BackupOriginalValues();
                SpoofAllHardware();
                SystemInfoSpoofer.SpoofPCName();
                NetworkSpoofer.SpoofNetworkConfig();
                SystemInfoSpoofer.SpoofInstallationID();
                SystemInfoSpoofer.SpoofExtendedGUIDs();
                MotherboardSpoofer.SpoofEFIVariableId();
                MotherboardSpoofer.SpoofSMBIOSSerialNumber();
                
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
        
        /// <summary>
        /// Realizes the spoofing of all hardware components
        /// </summary>
        public static void SpoofAllHardware()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting hardware spoofing process for all hardware components");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nStarting hardware spoofing process...");
            Console.ResetColor();
            
            try
            {
                BackupManager.BackupOriginalValues();
                CpuSpoofer.SpoofCPU();
                DiskSpoofer.SpoofDisk();
                MotherboardSpoofer.SpoofMotherboard();
                GpuSpoofer.SpoofGPU();
                NetworkSpoofer.SpoofMAC();
                
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
        
        /// <summary>
        /// Restores the original hardware values
        /// </summary>
        public static void RestoreOriginal()
        {
            BackupManager.RestoreOriginal();
        }
        
        /// <summary>
        /// Checks the registry keys
        /// </summary>
        public static void CheckRegistryKeys()
        {
            DiagnosticsHelper.CheckRegistryKeys();
        }
        
        /// <summary>
        /// Displays system information
        /// </summary>
        public static void DisplaySystemData()
        {
            DiagnosticsHelper.DisplaySystemData();
        }
        
        /// <summary>
        /// Clears the game cache
        /// </summary>
        /// <param name="gameName">Game name</param>
        public static void ClearGameCache(string gameName)
        {
            SystemInfoSpoofer.ClearGameCache(gameName);
        }
        
        // Individual spoofing methods - delegate to specific classes
        
        /// <summary>
        /// Realizes the spoofing of the CPU
        /// </summary>
        public static void SpoofCPU()
        {
            CpuSpoofer.SpoofCPU();
        }
        
        /// <summary>
        /// Realizes the spoofing of the disk
        /// </summary>
        public static void SpoofDisk()
        {
            DiskSpoofer.SpoofDisk();
        }
        
        /// <summary>
        /// Realizes the spoofing of the motherboard
        /// </summary>
        public static void SpoofMotherboard()
        {
            MotherboardSpoofer.SpoofMotherboard();
        }
        
        /// <summary>
        /// Realizes the spoofing of the GPU
        /// </summary>
        public static void SpoofGPU()
        {
            GpuSpoofer.SpoofGPU();
        }
        
        /// <summary>
        /// Realizes the spoofing of the network adapters
        /// </summary>
        public static void SpoofMAC()
        {
            NetworkSpoofer.SpoofMAC();
        }
        
        /// <summary>
        /// Realizes the spoofing of the installation ID
        /// </summary>
        public static void SpoofInstallationID()
        {
            SystemInfoSpoofer.SpoofInstallationID();
        }
        
        /// <summary>
        /// Realizes the spoofing of the PC name
        /// </summary>
        public static void SpoofPCName()
        {
            SystemInfoSpoofer.SpoofPCName();
        }
        
        /// <summary>
        /// Realizes the spoofing of advanced disk information
        /// </summary>
        public static void SpoofAdvancedDiskInfo()
        {
            DiskSpoofer.SpoofAdvancedDiskInfo();
        }
        
        /// <summary>
        /// Realizes the spoofing of extended GUIDs
        /// </summary>
        public static void SpoofExtendedGUIDs()
        {
            SystemInfoSpoofer.SpoofExtendedGUIDs();
        }
        
        /// <summary>
        /// Realizes the spoofing of the EFI variable ID
        /// </summary>
        public static void SpoofEFIVariableId()
        {
            MotherboardSpoofer.SpoofEFIVariableId();
        }
        
        /// <summary>
        /// Realizes the spoofing of the SMBIOS serial number
        /// </summary>
        public static void SpoofSMBIOSSerialNumber()
        {
            MotherboardSpoofer.SpoofSMBIOSSerialNumber();
        }
        
        /// <summary>
        /// Realizes the spoofing of network configuration (hostname, workgroup)
        /// </summary>
        public static void SpoofNetworkConfig()
        {
            NetworkSpoofer.SpoofNetworkConfig();
        }
    }
} 