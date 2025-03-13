using System;

namespace StealthSpoof.Core
{
    /// <summary>
    /// Classe responsável por gerenciar os menus da aplicação
    /// </summary>
    public static class MenuManager
    {
        /// <summary>
        /// Exibe o menu principal e processa a opção escolhida
        /// </summary>
        /// <returns>True para continuar, False para sair</returns>
        public static bool DisplayMainMenu()
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Show current hardware information");
            Console.WriteLine("2. Spoof EVERYTHING (Recommended)");
            Console.WriteLine("3. Restore original settings");
            Console.WriteLine("4. Advanced options");
            Console.WriteLine("5. Diagnostics");
            Console.WriteLine("0. Exit");
            
            string? input = UIHelper.ReadOption();
            
            Logger.Instance.Info($"User selected option: {input}");
            
            return ProcessMainMenuOption(input);
        }
        
        /// <summary>
        /// Processa a opção escolhida no menu principal
        /// </summary>
        /// <param name="input">A opção escolhida</param>
        /// <returns>True para continuar, False para sair</returns>
        private static bool ProcessMainMenuOption(string? input)
        {
            switch (input)
            {
                case "0":
                    Logger.Instance.Info("Application exit requested");
                    return false;
                case "1":
                    HardwareInfo.ShowCurrentHardwareInfo();
                    break;
                case "2":
                    HardwareSpoofer.SpoofAll();
                    break;
                case "3":
                    HardwareSpoofer.RestoreOriginal();
                    break;
                case "4":
                    DisplayAdvancedMenu();
                    break;
                case "5":
                    DisplayDiagnosticsMenu();
                    break;
                default:
                    UIHelper.DisplayInvalidOption(input);
                    break;
            }
            
            return true;
        }
        
        /// <summary>
        /// Exibe o menu de opções avançadas
        /// </summary>
        public static void DisplayAdvancedMenu()
        {
            Console.WriteLine("\n=== Advanced Options ===");
            Console.WriteLine("1. Spoof hardware components only");
            Console.WriteLine("2. Spoof CPU only");
            Console.WriteLine("3. Spoof Disk only");
            Console.WriteLine("4. Spoof Motherboard only");
            Console.WriteLine("5. Spoof GPU only");
            Console.WriteLine("6. Spoof MAC only");
            Console.WriteLine("7. Spoof PC Name only");
            Console.WriteLine("8. Spoof Installation ID only");
            Console.WriteLine("9. Spoof Extended GUIDs only");
            Console.WriteLine("10. Spoof EFI Variable ID only");
            Console.WriteLine("11. Spoof SMBIOS Serial Number only");
            Console.WriteLine("12. Clear Game Caches");
            Console.WriteLine("0. Back to main menu");
            
            string? input = UIHelper.ReadOption();
            
            Logger.Instance.Info($"User selected advanced option: {input}");
            
            ProcessAdvancedMenuOption(input);
        }
        
        /// <summary>
        /// Processa a opção escolhida no menu de opções avançadas
        /// </summary>
        /// <param name="input">A opção escolhida</param>
        private static void ProcessAdvancedMenuOption(string? input)
        {
            switch (input)
            {
                case "0":
                    return;
                case "1":
                    HardwareSpoofer.SpoofAllHardware();
                    break;
                case "2":
                    HardwareSpoofer.SpoofCPU();
                    break;
                case "3":
                    HardwareSpoofer.SpoofDisk();
                    break;
                case "4":
                    HardwareSpoofer.SpoofMotherboard();
                    break;
                case "5":
                    HardwareSpoofer.SpoofGPU();
                    break;
                case "6":
                    HardwareSpoofer.SpoofMAC();
                    break;
                case "7":
                    HardwareSpoofer.SpoofPCName();
                    break;
                case "8":
                    HardwareSpoofer.SpoofInstallationID();
                    break;
                case "9":
                    HardwareSpoofer.SpoofExtendedGUIDs();
                    break;
                case "10":
                    HardwareSpoofer.SpoofEFIVariableId();
                    break;
                case "11":
                    HardwareSpoofer.SpoofSMBIOSSerialNumber();
                    break;
                case "12":
                    DisplayGameCacheMenu();
                    break;
                default:
                    UIHelper.DisplayInvalidOption(input);
                    break;
            }
        }
        
        /// <summary>
        /// Exibe o menu de diagnósticos
        /// </summary>
        public static void DisplayDiagnosticsMenu()
        {
            Console.WriteLine("\n=== Diagnostics ===");
            Console.WriteLine("1. Check Registry Keys");
            Console.WriteLine("2. Display System Data");
            Console.WriteLine("0. Back to main menu");
            
            string? input = UIHelper.ReadOption();
            
            Logger.Instance.Info($"User selected diagnostics option: {input}");
            
            ProcessDiagnosticsMenuOption(input);
        }
        
        /// <summary>
        /// Process the selected option in the diagnostics menu
        /// </summary>
        /// <param name="input">The selected option</param>
        private static void ProcessDiagnosticsMenuOption(string? input)
        {
            switch (input)
            {
                case "0":
                    return;
                case "1":
                    HardwareSpoofer.CheckRegistryKeys();
                    break;
                case "2":
                    HardwareSpoofer.DisplaySystemData();
                    break;
                default:
                    UIHelper.DisplayInvalidOption(input);
                    break;
            }
        }
        
        /// <summary>
        /// Displays the game cache menu
        /// </summary>
        public static void DisplayGameCacheMenu()
        {
            Console.WriteLine("\nChoose a game cache to clear:");
            Console.WriteLine("1. Ubisoft");
            Console.WriteLine("2. Valorant");
            Console.WriteLine("3. Call of Duty");
            Console.WriteLine("0. Back to main menu");
            
            string? input = UIHelper.ReadOption();
            
            ProcessGameCacheMenuOption(input);
        }
        
        /// <summary>
        /// Process the selected option in the game cache menu
        /// </summary>
        /// <param name="input">The selected option</param>
        private static void ProcessGameCacheMenuOption(string? input)
        {
            switch (input)
            {
                case "0":
                    return;
                case "1":
                    HardwareSpoofer.ClearGameCache("Ubisoft");
                    break;
                case "2":
                    HardwareSpoofer.ClearGameCache("Valorant");
                    break;
                case "3":
                    HardwareSpoofer.ClearGameCache("CallOfDuty");
                    break;
                default:
                    UIHelper.DisplayInvalidOption(input);
                    break;
            }
        }
        
        /// <summary>
        /// Executes the main menu loop
        /// </summary>
        public static void RunMainLoop()
        {
            while (true)
            {
                if (!DisplayMainMenu())
                    return;
            }
        }
    }
} 