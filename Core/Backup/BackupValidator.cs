using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using StealthSpoof.Core;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Class responsible for backup validation operations
    /// </summary>
    public static class BackupValidator
    {
        public static bool ValidateBackupIntegrity(string backupPath, BackupStorage storage)
        {
            try
            {
                var backupData = BackupStorage.LoadBackup(backupPath);
                if (backupData == null) return false;
                
                if (!backupData.TryGetValue("Metadata", out var metadataObj) ||
                    !(metadataObj is Dictionary<string, object> metadata))
                {
                    return false;
                }
                
                if (!metadata.TryGetValue(BackupMetadata.META_CHECKSUM, out var checksumObj) ||
                    !(checksumObj is string storedChecksum))
                {
                    return false;
                }
                
                // Remove metadata before calculating checksum
                backupData.Remove("Metadata");
                string json = System.Text.Json.JsonSerializer.Serialize(backupData);
                string calculatedChecksum = CalculateChecksum(json);
                
                return storedChecksum.Equals(calculatedChecksum, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        
        public static string CalculateChecksum(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
} 