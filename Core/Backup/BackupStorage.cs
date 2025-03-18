using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using StealthSpoof.Core;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Class responsible for backup storage operations
    /// </summary>
    public class BackupStorage
    {
        private readonly string _backupDir;
        private readonly string _keysDir;
        private const string BACKUP_EXTENSION = ".ssbak";
        private const string KEY_EXTENSION = ".key";
        private const string BACKUP_PREFIX = "backup_";
        private const int MAX_BACKUPS = 5;
        
        // Entropy for additional protection of keys
        private static readonly byte[] _entropyBytes = { 0x43, 0x87, 0x23, 0x72, 0x45, 0xA3, 0xB2, 0xE1 };
        
        public BackupStorage()
        {
            _backupDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StealthSpoof", 
                "Backups"
            );
            
            _keysDir = Path.Combine(_backupDir, "Keys");
            
            Directory.CreateDirectory(_backupDir);
            Directory.CreateDirectory(_keysDir);
            
            // Check for DPAPI availability
            if (!OperatingSystem.IsWindows())
            {
                Logger.Instance.Warning("DPAPI protection is only available on Windows. Using basic protection instead.");
            }
        }
        
        public string SaveBackup(Dictionary<string, Dictionary<string, object>> backupData)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(_backupDir, $"{BACKUP_PREFIX}{timestamp}{BACKUP_EXTENSION}");
            string keyPath = Path.Combine(_keysDir, $"{BACKUP_PREFIX}{timestamp}{KEY_EXTENSION}");
            
            // Serialize to JSON
            string json = System.Text.Json.JsonSerializer.Serialize(
                backupData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );
            
            // Compress and encrypt the data
            using (var fileStream = File.Create(backupPath))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            using (var aes = Aes.Create())
            {
                // Generate a random key and IV
                aes.GenerateKey();
                aes.GenerateIV();
                
                // Write the IV first (not encrypted)
                fileStream.Write(aes.IV, 0, aes.IV.Length);
                
                // Save the key securely to a separate file
                SaveEncryptionKey(keyPath, aes.Key);
                
                // Create encryptor and crypto stream
                using (var encryptor = aes.CreateEncryptor())
                using (var cryptoStream = new CryptoStream(gzipStream, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(json);
                }
            }
            
            Logger.Instance.Info($"Backup saved to {backupPath} with secure key storage");
            RotateBackups();
            return backupPath;
        }
        
        /// <summary>
        /// Saves the encryption key securely using DPAPI
        /// </summary>
        private static void SaveEncryptionKey(string keyPath, byte[] key)
        {
            try
            {
                // Encrypt the key using Windows Data Protection API
                byte[] protectedKey;
                
                if (OperatingSystem.IsWindows())
                {
                    // Use DPAPI for Windows
                    protectedKey = System.Security.Cryptography.ProtectedData.Protect(
                        key, 
                        _entropyBytes, 
                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                }
                else
                {
                    // Basic protection for non-Windows systems
                    protectedKey = ApplyBasicProtection(key);
                }
                
                // Save the protected key to file
                File.WriteAllBytes(keyPath, protectedKey);
                Logger.Instance.Debug($"Encryption key saved securely to {keyPath}");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error saving encryption key");
                throw;
            }
        }
        
        /// <summary>
        /// Loads the encryption key from secure storage
        /// </summary>
        private static byte[]? LoadEncryptionKey(string backupPath)
        {
            try
            {
                // Determine key path from backup path
                string backupName = Path.GetFileNameWithoutExtension(backupPath);
                string keyPath = Path.Combine(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "StealthSpoof", 
                        "Backups"
                    ),
                    "Keys", 
                    $"{backupName}{KEY_EXTENSION}");
                
                if (!File.Exists(keyPath))
                {
                    Logger.Instance.Error($"Encryption key file not found: {keyPath}");
                    return null;
                }
                
                // Read the protected key from file
                byte[] protectedKey = File.ReadAllBytes(keyPath);
                
                // Decrypt the key
                if (OperatingSystem.IsWindows())
                {
                    // Use DPAPI for Windows
                    return System.Security.Cryptography.ProtectedData.Unprotect(
                        protectedKey, 
                        _entropyBytes, 
                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                }
                else
                {
                    // Basic protection for non-Windows systems
                    return RemoveBasicProtection(protectedKey);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error loading encryption key");
                return null;
            }
        }
        
        /// <summary>
        /// Applies basic protection for non-Windows systems
        /// This is less secure than DPAPI but provides some level of protection
        /// </summary>
        private static byte[] ApplyBasicProtection(byte[] data)
        {
            // XOR with entropy bytes and apply basic scrambling
            byte[] protected_data = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                protected_data[i] = (byte)(data[i] ^ _entropyBytes[i % _entropyBytes.Length]);
            }
            return protected_data;
        }
        
        /// <summary>
        /// Removes basic protection for non-Windows systems
        /// </summary>
        private static byte[] RemoveBasicProtection(byte[] protectedData)
        {
            // Reverse the XOR operation
            byte[] original_data = new byte[protectedData.Length];
            for (int i = 0; i < protectedData.Length; i++)
            {
                original_data[i] = (byte)(protectedData[i] ^ _entropyBytes[i % _entropyBytes.Length]);
            }
            return original_data;
        }
        
        public static Dictionary<string, Dictionary<string, object>>? LoadBackup(string backupPath)
        {
            try
            {
                // Load the encryption key first
                byte[]? key = LoadEncryptionKey(backupPath);
                if (key == null)
                {
                    Logger.Instance.Error("Failed to load encryption key");
                    return null;
                }
                
                using (var fileStream = File.OpenRead(backupPath))
                using (var aes = Aes.Create())
                {
                    // Read the IV
                    byte[] iv = new byte[16];
                    int bytesRead = fileStream.Read(iv, 0, iv.Length);
                    if (bytesRead != iv.Length)
                    {
                        Logger.Instance.Error($"Failed to read complete IV. Expected {iv.Length} bytes, got {bytesRead}");
                        return null;
                    }
                    aes.IV = iv;
                    
                    // Use the loaded key
                    aes.Key = key;
                    
                    // Create decryptor and crypto stream
                    using (var decryptor = aes.CreateDecryptor())
                    using (var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                    using (var gzipStream = new GZipStream(cryptoStream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(gzipStream))
                    {
                        string json = reader.ReadToEnd();
                        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error loading backup data");
                return null;
            }
        }
        
        public string? GetLatestBackup()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDir, $"*{BACKUP_EXTENSION}")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .FirstOrDefault();
                    
                return backupFiles;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error getting latest backup");
                return null;
            }
        }
        
        private void RotateBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDir, $"*{BACKUP_EXTENSION}")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();
                
                var keyFiles = Directory.GetFiles(_keysDir, $"*{KEY_EXTENSION}")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();
                    
                while (backupFiles.Count > MAX_BACKUPS)
                {
                    // Delete backup file
                    string oldBackup = backupFiles[backupFiles.Count - 1];
                    File.Delete(oldBackup);
                    backupFiles.RemoveAt(backupFiles.Count - 1);
                    
                    // Delete corresponding key file if it exists
                    string oldBackupName = Path.GetFileNameWithoutExtension(oldBackup);
                    string oldKeyPath = Path.Combine(_keysDir, $"{oldBackupName}{KEY_EXTENSION}");
                    if (File.Exists(oldKeyPath))
                    {
                        File.Delete(oldKeyPath);
                    }
                }
                
                // Clean up any orphaned key files
                foreach (var keyFile in keyFiles)
                {
                    string keyBaseName = Path.GetFileNameWithoutExtension(keyFile);
                    string correspondingBackup = Path.Combine(_backupDir, $"{keyBaseName}{BACKUP_EXTENSION}");
                    if (!File.Exists(correspondingBackup))
                    {
                        File.Delete(keyFile);
                        Logger.Instance.Debug($"Deleted orphaned key file: {keyFile}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error rotating backups");
            }
        }
    }
} 