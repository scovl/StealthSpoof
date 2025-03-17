using System;
using Microsoft.Win32;
using System.IO;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Spoofers
{
    /// <summary>
    /// Class responsible for system information spoofing
    /// </summary>
    public static class SystemInfoSpoofer
    {
        /// <summary>
        /// Performs Windows installation ID spoofing
        /// </summary>
        public static void SpoofInstallationID()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting Installation ID spoofing");
            Console.WriteLine("\nSpoofing Installation ID...");
            
            try
            {
                string newInstallationID = Guid.NewGuid().ToString().ToUpper();
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_WINDOWS_NT))
                {
                    if (key != null)
                    {
                        key.SetValue("InstallationID", newInstallationID, RegistryValueKind.String);
                        key.SetValue("InstallDate", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), RegistryValueKind.DWord);
                    }
                }
                
                Logger.Instance.Info($"Installation ID spoofed successfully to: {newInstallationID}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Installation ID modified to: {newInstallationID}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing Installation ID");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing Installation ID: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Performs PC name spoofing
        /// </summary>
        public static void SpoofPCName()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting PC Name spoofing");
            Console.WriteLine("\nSpoofing PC Name...");
            
            try
            {
                string newPCName = "PC-" + HardwareInfo.GetRandomHardwareID(8);
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_COMPUTER_NAME))
                {
                    if (key != null)
                    {
                        key.SetValue("ComputerName", newPCName, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_ACTIVE_COMPUTER_NAME))
                {
                    if (key != null)
                    {
                        key.SetValue("ComputerName", newPCName, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_TCPIP_PARAMETERS))
                {
                    if (key != null)
                    {
                        key.SetValue("Hostname", newPCName, RegistryValueKind.String);
                        key.SetValue("NV Hostname", newPCName, RegistryValueKind.String);
                    }
                }
                
                Logger.Instance.Info($"PC Name spoofed successfully to: {newPCName}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PC Name modified to: {newPCName}");
                Console.WriteLine("Note: A system restart is required for this change to take full effect.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing PC Name");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing PC Name: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Performs extended GUIDs spoofing
        /// </summary>
        public static void SpoofExtendedGUIDs()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting Extended GUIDs spoofing");
            Console.WriteLine("\nSpoofing Extended GUIDs...");
            
            try
            {
                // Spoof Machine GUID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_CRYPTOGRAPHY))
                {
                    if (key != null)
                    {
                        key.SetValue("MachineGuid", Guid.NewGuid().ToString(), RegistryValueKind.String);
                    }
                }
                
                // Spoof Hardware Profile GUID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_HARDWARE_PROFILES))
                {
                    if (key != null)
                    {
                        key.SetValue("HwProfileGuid", "{" + Guid.NewGuid().ToString().ToUpper() + "}", RegistryValueKind.String);
                    }
                }
                
                // Spoof SQM Client ID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_SQM_CLIENT))
                {
                    if (key != null)
                    {
                        key.SetValue("MachineId", "{" + Guid.NewGuid().ToString().ToUpper() + "}", RegistryValueKind.String);
                    }
                }
                
                // Spoof Windows Update ID
                using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_WINDOWS_UPDATE))
                {
                    if (key != null)
                    {
                        key.SetValue("SusClientId", Guid.NewGuid().ToString(), RegistryValueKind.String);
                    }
                }
                
                Logger.Instance.Info("Extended GUIDs spoofed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Extended GUIDs modified successfully.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing Extended GUIDs");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing Extended GUIDs: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Clears game cache
        /// </summary>
        /// <param name="gameName">Game name</param>
        public static void ClearGameCache(string gameName)
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info($"Clearing {gameName} cache");
            Console.WriteLine($"\nClearing {gameName} cache...");
            
            try
            {
                switch (gameName)
                {
                    case "Ubisoft":
                        ClearUbisoftCache();
                        break;
                    case "Valorant":
                        ClearValorantCache();
                        break;
                    case "CallOfDuty":
                        ClearCallOfDutyCache();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unknown game: {gameName}");
                        Console.ResetColor();
                        return;
                }
                
                Logger.Instance.Info($"{gameName} cache cleared successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{gameName} cache cleared successfully!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, $"Error clearing {gameName} cache");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error clearing {gameName} cache: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void ClearUbisoftCache()
        {
            string ubisoftPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Ubisoft Game Launcher"
            );
            
            ClearDirectory(ubisoftPath, "cache");
            ClearDirectory(ubisoftPath, "logs");
            ClearDirectory(ubisoftPath, "temp");
        }
        
        private static void ClearValorantCache()
        {
            string valorantPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Riot Games", "Valorant"
            );
            
            ClearDirectory(valorantPath, "Saved");
            
            string vgcPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Riot Vanguard"
            );
            
            if (Directory.Exists(vgcPath))
            {
                Console.WriteLine("Note: Vanguard files found. You may need to reinstall Vanguard after spoofing.");
            }
        }
        
        private static void ClearCallOfDutyCache()
        {
            string codPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Activision"
            );
            
            ClearDirectory(codPath, "Cache");
            
            string blizzardPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Battle.net"
            );
            
            ClearDirectory(blizzardPath, "Cache");
        }
        
        private static void ClearDirectory(string basePath, string dirName)
        {
            string fullPath = Path.Combine(basePath, dirName);
            
            if (Directory.Exists(fullPath))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(fullPath);
                    
                    foreach (FileInfo file in di.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Warning($"Could not delete file {file.FullName}: {ex.Message}");
                        }
                    }
                    
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        try
                        {
                            dir.Delete(true);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Warning($"Could not delete directory {dir.FullName}: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"Cleared {fullPath}");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Error clearing directory {fullPath}: {ex.Message}");
                    Console.WriteLine($"Error clearing {fullPath}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Directory not found: {fullPath}");
            }
        }
    }
} 