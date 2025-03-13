using System;

namespace StealthSpoof.Core
{
    /// <summary>
    /// Classe auxiliar para funções de interface do usuário
    /// </summary>
    public static class UIHelper
    {
        // Constantes
        public const string PROMPT_OPTION = "\nOption: ";
        
        /// <summary>
        /// Exibe o cabeçalho do aplicativo
        /// </summary>
        public static void DisplayHeader()
        {
            Console.Title = "StealthSpoof - HWID Spoofer";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("        StealthSpoof HWID Spoofer v1.0           ");
            Console.WriteLine("=================================================");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Exibe uma mensagem de opção inválida
        /// </summary>
        /// <param name="input">A entrada inválida</param>
        public static void DisplayInvalidOption(string? input)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nInvalid option: {input}");
            Console.WriteLine("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
        }
        
        /// <summary>
        /// Lê uma opção do usuário
        /// </summary>
        /// <returns>A opção escolhida pelo usuário</returns>
        public static string? ReadOption()
        {
            Console.Write(PROMPT_OPTION);
            return Console.ReadLine();
        }
    }
} 