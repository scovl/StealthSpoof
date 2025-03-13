using System;

namespace StealthSpoof.Core
{
    /// <summary>
    /// Class helper for user interface functions
    /// </summary>
    public static class UIHelper
    {
        // Constantes
        public const string PROMPT_OPTION = "\nOption: ";
        
        /// <summary>
        /// Displays the application header
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
        /// Displays an invalid option message
        /// </summary>
        /// <param name="input">The invalid input</param>
        public static void DisplayInvalidOption(string? input)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nInvalid option: {input}");
            Console.WriteLine("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
        }
        
        /// <summary>
        /// Reads the user's option
        /// </summary>
        /// <returns>The option chosen by the user</returns>
        public static string? ReadOption()
        {
            Console.Write(PROMPT_OPTION);
            return Console.ReadLine();
        }
    }
} 