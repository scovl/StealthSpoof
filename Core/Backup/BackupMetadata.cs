using System;
using System.Collections.Generic;

namespace StealthSpoof.Core.Backup
{
    /// <summary>
    /// Class responsible for managing backup metadata
    /// </summary>
    public class BackupMetadata
    {
        // Metadata keys
        public const string META_VERSION = "Version";
        public const string META_TIMESTAMP = "Timestamp";
        public const string META_CHECKSUM = "Checksum";
        public const string META_INCREMENTAL = "Incremental";
        public const string META_BASE_BACKUP = "BaseBackup";
        
        private readonly Dictionary<string, object> _metadata;
        private bool _isIncremental;
        private string? _baseBackupPath;
        private string? _checksum;
        
        public BackupMetadata()
        {
            _metadata = new Dictionary<string, object>
            {
                { META_VERSION, "1.0" },
                { META_TIMESTAMP, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { META_INCREMENTAL, false }
            };
        }
        
        /// <summary>
        /// Sets whether this is an incremental backup
        /// </summary>
        public void SetIncremental(bool isIncremental)
        {
            _isIncremental = isIncremental;
        }
        
        /// <summary>
        /// Sets the path to the base backup for incremental backups
        /// </summary>
        public void SetBaseBackupPath(string? baseBackupPath)
        {
            _baseBackupPath = baseBackupPath;
        }
        
        /// <summary>
        /// Sets the checksum of the backup data
        /// </summary>
        public void SetChecksum(string? checksum)
        {
            _checksum = checksum;
        }
        
        /// <summary>
        /// Gets whether this is an incremental backup
        /// </summary>
        public bool IsIncremental => _isIncremental;
        
        /// <summary>
        /// Gets the path to the base backup for incremental backups
        /// </summary>
        public string? BaseBackupPath => _baseBackupPath;
        
        /// <summary>
        /// Gets the checksum of the backup data
        /// </summary>
        public string? Checksum => _checksum;
        
        public Dictionary<string, object> GetMetadata()
        {
            return new Dictionary<string, object>(_metadata);
        }
        
        public string? GetBaseBackupName()
        {
            return _metadata.TryGetValue(META_BASE_BACKUP, out var value) ? 
                   value?.ToString() : null;
        }
        
        public string? GetChecksum()
        {
            return _metadata.TryGetValue(META_CHECKSUM, out var value) ? 
                   value?.ToString() : null;
        }
    }
} 