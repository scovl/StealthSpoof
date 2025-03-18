using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using StealthSpoof.Core;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Class responsible for backup storage operations
    /// </summary>
    public class BackupStorage
    {
        private readonly string _backupDir;
        private const string BACKUP_EXTENSION = ".ssbak";
        private const string BACKUP_PREFIX = "backup_";
        private const int MAX_BACKUPS = 5;
        
        public BackupStorage()
        {
            _backupDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StealthSpoof", 
                "Backups"
            );
            
            Directory.CreateDirectory(_backupDir);
        }
        
        public string SaveBackup(Dictionary<string, Dictionary<string, object>> backupData)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(_backupDir, $"{BACKUP_PREFIX}{timestamp}{BACKUP_EXTENSION}");
            
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
                
                // Write the IV first
                fileStream.Write(aes.IV, 0, aes.IV.Length);
                
                // Create encryptor and crypto stream
                using (var encryptor = aes.CreateEncryptor())
                using (var cryptoStream = new CryptoStream(gzipStream, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(json);
                }
            }
            
            RotateBackups();
            return backupPath;
        }
        
        public static Dictionary<string, Dictionary<string, object>>? LoadBackup(string backupPath)
        {
            try
            {
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
                    
                    // Generate a key (in a real application, you would store and retrieve this securely)
                    aes.GenerateKey();
                    
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
            catch
            {
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
                    
                while (backupFiles.Count > MAX_BACKUPS)
                {
                    File.Delete(backupFiles[backupFiles.Count - 1]);
                    backupFiles.RemoveAt(backupFiles.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error rotating backups");
            }
        }
    }
} 