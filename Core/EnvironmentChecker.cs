using System;

namespace StealthSpoof.Core
{
    /// <summary>
    /// Class responsible for checking the application environment
    /// </summary>
    public static class EnvironmentChecker
    {
        /// <summary>
        /// Checks if the environment is compatible with the application
        /// </summary>
        /// <returns>True if the environment is compatible, False otherwise</returns>
        public static bool CheckEnvironment()
        {
            // Check if the operating system is Windows
            if (!OperatingSystem.IsWindows())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This application only works on Windows!");
                Console.ResetColor();
                return false;
            }
            
            // Check the Windows version
            Version windowsVersion = Environment.OSVersion.Version;
            if (windowsVersion.Major < 10)
            {
                Logger.Instance.Warning($"Unsupported Windows version: {windowsVersion}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: This application is designed for Windows 10 and later.");
                Console.WriteLine("Some features may not work correctly on your system.");
                Console.ResetColor();
            }
            
            return true;
        }
    }
} 