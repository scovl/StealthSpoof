using System;

namespace StealthSpoof.Core
{
    /// <summary>
    /// Classe responsável por verificar o ambiente de execução
    /// </summary>
    public static class EnvironmentChecker
    {
        /// <summary>
        /// Verifica se o ambiente é compatível com a aplicação
        /// </summary>
        /// <returns>True se o ambiente for compatível, False caso contrário</returns>
        public static bool CheckEnvironment()
        {
            // Verifica se o sistema operacional é Windows
            if (!OperatingSystem.IsWindows())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This application only works on Windows!");
                Console.ResetColor();
                return false;
            }
            
            // Verifica a versão do Windows
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