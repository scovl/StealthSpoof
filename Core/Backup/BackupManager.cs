using System;
using System.IO;
using System.Management;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Linq;
using StealthSpoof.Core.Utils;
using System.Threading;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StealthSpoof.Core;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Main class responsible for coordinating backup operations
    /// </summary>
    public static class BackupManager
    {
        private static readonly BackupStorage _storage = new();
        private static readonly BackupMetadata _metadata = new();
        
        /// <summary>
        /// Creates a backup of the original hardware values
        /// </summary>
        public static void BackupOriginalValues()
        {
            try
            {
                Logger.Instance.Info("Starting backup of original hardware values");
                Console.WriteLine("Creating backup of original hardware values...");
                
                // Collect current hardware information
                var hardwareInfo = BackupCollector.CollectHardwareInfo();
                if (hardwareInfo == null || !hardwareInfo.Any())
                {
                    Logger.Instance.Error("Failed to collect hardware information for backup");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Failed to collect hardware information for backup");
                    Console.ResetColor();
                    return;
                }
                
                Logger.Instance.Debug($"Collected {hardwareInfo.Count} hardware information items");
                
                // Set backup metadata
                _metadata.SetIncremental(false);
                
                // Calculate checksum
                string serializedData = JsonSerializer.Serialize(hardwareInfo);
                string checksum = BackupValidator.CalculateChecksum(serializedData);
                _metadata.SetChecksum(checksum);
                
                Logger.Instance.Debug($"Backup checksum calculated: {checksum.Substring(0, 8)}...");
                
                // Save backup
                bool saveSuccess = _storage.SaveBackup(hardwareInfo);
                if (!saveSuccess)
                {
                    Logger.Instance.Error("Failed to save backup");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Could not save backup");
                    Console.ResetColor();
                    return;
                }
                
                Logger.Instance.Info("Original hardware values backup completed successfully");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Backup created successfully");
                Console.ResetColor();
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogException(ex, "Access denied while creating backup");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Access denied while creating backup: {ex.Message}");
                Console.WriteLine("Try running the application as administrator");
                Console.ResetColor();
            }
            catch (IOException ex)
            {
                Logger.Instance.LogException(ex, "I/O error while creating backup");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"I/O error while creating backup: {ex.Message}");
                Console.WriteLine("Check if you have write access to the backup directory");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Failed to backup original hardware values");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to backup original hardware values: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Restores the original hardware values
        /// </summary>
        public static void RestoreOriginal()
        {
            if (!RegistryHelper.RequireAdminCheck())
            {
                Logger.Instance.Warning("Cannot restore original values - administrative privileges required");
                return;
            }
            
            try
            {
                Logger.Instance.Info("Starting restore of original hardware values");
                Console.WriteLine("Restoring original hardware values...");
                
                // Get latest backup
                var latestBackup = _storage.GetLatestBackup();
                if (latestBackup == null)
                {
                    Logger.Instance.Error("No backup found to restore from");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: No backup found to restore from");
                    Console.ResetColor();
                    return;
                }
                
                Logger.Instance.Debug($"Found backup file: {latestBackup}");
                
                // Validate backup integrity
                if (!BackupValidator.ValidateBackupIntegrity(latestBackup, _storage))
                {
                    Logger.Instance.Error("Backup validation failed, backup may be corrupted");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Backup validation failed, backup may be corrupted");
                    Console.ResetColor();
                    return;
                }
                
                Logger.Instance.Debug("Backup validation passed");
                
                // Load backup data
                var backupData = BackupStorage.LoadBackup(latestBackup);
                if (backupData == null || !backupData.Any())
                {
                    Logger.Instance.Error("Failed to load backup data");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Failed to load backup data");
                    Console.ResetColor();
                    return;
                }
                
                Logger.Instance.Debug($"Loaded {backupData.Count} hardware information items from backup");
                
                // Restore hardware information
                int restoredCount = BackupRestorer.RestoreHardwareInfo(backupData);
                
                if (restoredCount > 0)
                {
                    Logger.Instance.Info($"Original hardware values restored successfully ({restoredCount} items)");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Original hardware values restored successfully ({restoredCount} items)");
                    Console.WriteLine("It may be necessary to restart the computer to apply all changes.");
                    Console.ResetColor();
                }
                else
                {
                    Logger.Instance.Warning("No hardware values were restored");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No hardware values were restored");
                    Console.ResetColor();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogException(ex, "Access denied while restoring original values");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Access denied while restoring original values: {ex.Message}");
                Console.WriteLine("Try running the application as administrator");
                Console.ResetColor();
            }
            catch (IOException ex)
            {
                Logger.Instance.LogException(ex, "I/O error while restoring original values");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"I/O error while restoring original values: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Failed to restore original hardware values");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to restore original hardware values: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Creates a new backup of the current hardware information
        /// </summary>
        /// <param name="isIncremental">Whether to create an incremental backup</param>
        /// <returns>True if backup was successful, false otherwise</returns>
        public static bool CreateBackup(bool isIncremental = false)
        {
            try
            {
                Logger.Instance.Info("Starting backup process");
                
                // Collect current hardware information
                var hardwareInfo = BackupCollector.CollectHardwareInfo();
                
                // Set backup metadata
                _metadata.SetIncremental(isIncremental);
                if (isIncremental)
                {
                    var latestBackup = _storage.GetLatestBackup();
                    if (latestBackup != null)
                    {
                        _metadata.SetBaseBackupPath(latestBackup);
                    }
                }
                
                // Calculate checksum
                string checksum = BackupValidator.CalculateChecksum(JsonSerializer.Serialize(hardwareInfo));
                _metadata.SetChecksum(checksum);
                
                // Save backup
                _storage.SaveBackup(hardwareInfo);
                
                Logger.Instance.Info("Backup completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to create backup: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Restores hardware information from the latest backup
        /// </summary>
        /// <returns>True if restore was successful, false otherwise</returns>
        public static bool RestoreFromBackup()
        {
            try
            {
                Logger.Instance.Info("Starting restore process");
                
                // Get latest backup
                var latestBackup = _storage.GetLatestBackup();
                if (latestBackup == null)
                {
                    Logger.Instance.Error("No backup found to restore from");
                    return false;
                }
                
                // Validate backup integrity
                if (!BackupValidator.ValidateBackupIntegrity(latestBackup, _storage))
                {
                    Logger.Instance.Error("Backup validation failed");
                    return false;
                }
                
                // Load backup data
                var backupData = BackupStorage.LoadBackup(latestBackup);
                if (backupData == null)
                {
                    Logger.Instance.Error("Failed to load backup data");
                    return false;
                }
                
                // Restore hardware information
                BackupRestorer.RestoreHardwareInfo(backupData);
                
                Logger.Instance.Info("Restore completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to restore from backup: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validates the integrity of the latest backup
        /// </summary>
        /// <returns>True if backup is valid, false otherwise</returns>
        public static bool ValidateLatestBackup()
        {
            try
            {
                var latestBackup = _storage.GetLatestBackup();
                if (latestBackup == null)
                {
                    Logger.Instance.Error("No backup found to validate");
                    return false;
                }
                
                return BackupValidator.ValidateBackupIntegrity(latestBackup, _storage);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to validate backup: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets the timestamp of the latest backup
        /// </summary>
        /// <returns>DateTime of the latest backup, or null if no backup exists</returns>
        public static DateTime? GetLatestBackupTime()
        {
            try
            {
                var latestBackup = _storage.GetLatestBackup();
                if (latestBackup == null)
                {
                    return null;
                }
                
                return File.GetLastWriteTime(latestBackup);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to get latest backup time: {ex.Message}");
                return null;
            }
        }
    }
} 