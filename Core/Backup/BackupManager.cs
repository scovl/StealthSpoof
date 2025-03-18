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
                
                // Collect current hardware information
                var hardwareInfo = BackupCollector.CollectHardwareInfo();
                
                // Set backup metadata
                _metadata.SetIncremental(false);
                
                // Calculate checksum
                string checksum = BackupValidator.CalculateChecksum(JsonSerializer.Serialize(hardwareInfo));
                _metadata.SetChecksum(checksum);
                
                // Save backup
                _storage.SaveBackup(hardwareInfo);
                
                Logger.Instance.Info("Original hardware values backup completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to backup original hardware values: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Restores the original hardware values
        /// </summary>
        public static void RestoreOriginal()
        {
            try
            {
                Logger.Instance.Info("Starting restore of original hardware values");
                
                // Get latest backup
                var latestBackup = _storage.GetLatestBackup();
                if (latestBackup == null)
                {
                    Logger.Instance.Error("No backup found to restore from");
                    return;
                }
                
                // Validate backup integrity
                if (!BackupValidator.ValidateBackupIntegrity(latestBackup, _storage))
                {
                    Logger.Instance.Error("Backup validation failed");
                    return;
                }
                
                // Load backup data
                var backupData = BackupStorage.LoadBackup(latestBackup);
                if (backupData == null)
                {
                    Logger.Instance.Error("Failed to load backup data");
                    return;
                }
                
                // Restore hardware information
                BackupRestorer.RestoreHardwareInfo(backupData);
                
                Logger.Instance.Info("Original hardware values restored successfully");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to restore original hardware values: {ex.Message}");
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