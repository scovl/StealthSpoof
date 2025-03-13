using System;
using System.IO;
using System.Text;
using System.Threading;

namespace StealthSpoof.Core
{
    /// <summary>
    /// Simple logging system for StealthSpoof
    /// </summary>
    public sealed class Logger
    {
        // Singleton instance
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        
        // Log file path
        private readonly string _logFilePath;
        
        // Log levels
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }
        
        // Minimum log level to record
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;
        
        // Enable or disable logging
        public bool LoggingEnabled { get; set; } = true;
        
        // Get singleton instance
        public static Logger Instance => _instance.Value;
        
        // Private constructor (singleton pattern)
        private Logger()
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StealthSpoof", 
                "Logs"
            );
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            _logFilePath = Path.Combine(
                logDirectory, 
                $"StealthSpoof_{DateTime.Now:yyyy-MM-dd}.log"
            );
            
            // Log startup
            Log(LogLevel.Info, "Logger initialized");
        }
        
        /// <summary>
        /// Log a message with the specified level
        /// </summary>
        public void Log(LogLevel level, string message)
        {
            if (!LoggingEnabled || level < MinimumLogLevel)
                return;
                
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                
                // Print to console if appropriate level
                if (level >= LogLevel.Warning)
                {
                    ConsoleColor originalColor = Console.ForegroundColor;
                    
                    switch (level)
                    {
                        case LogLevel.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogLevel.Error:
                        case LogLevel.Fatal:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                    }
                    
                    Console.WriteLine($"[LOG] {logEntry}");
                    Console.ForegroundColor = originalColor;
                }
                
                // Write to log file
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail if logging fails - we don't want to crash the app due to logging
            }
        }
        
        /// <summary>
        /// Log an exception
        /// </summary>
        public void LogException(Exception ex, string context = "")
        {
            string message = string.IsNullOrEmpty(context) 
                ? $"Exception: {ex.Message}" 
                : $"{context}: {ex.Message}";
                
            Log(LogLevel.Error, message);
            Log(LogLevel.Debug, $"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Log(LogLevel.Debug, $"Inner exception: {ex.InnerException.Message}");
            }
        }
        
        // Convenience methods for different log levels
        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message) => Log(LogLevel.Error, message);
        public void Fatal(string message) => Log(LogLevel.Fatal, message);
        
        // Add method to limit log file size
        public void CheckLogFileSize(long maxSizeBytes = 10485760) // 10MB default
        {
            try
            {
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Exists && fileInfo.Length > maxSizeBytes)
                {
                    // Criar arquivo de backup
                    string backupPath = Path.Combine(
                        Path.GetDirectoryName(_logFilePath) ?? string.Empty,
                        $"StealthSpoof_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log.bak"
                    );
                    
                    File.Move(_logFilePath, backupPath);
                    
                    // Criar novo arquivo de log
                    Log(LogLevel.Info, $"Log file rotated. Previous log saved to {backupPath}");
                }
            }
            catch
            {
                // Silently fail if log rotation fails
            }
        }
        
        // Method to log registry operations
        public void LogRegistryOperation(string key, string valueName, string operation)
        {
            Log(LogLevel.Warning, $"Registry {operation}: {key}\\{valueName}");
        }
    }
} 