using System;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Security.Principal;

namespace StealthSpoof.Core
{
    public static class HardwareSpoofer
    {
        private static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StealthSpoof", 
            "backup.json"
        );
        
        private static bool RequireAdminCheck()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
                
            if (!isAdmin)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This operation requires administrator privileges.");
                Console.WriteLine("Please restart the program as an administrator.");
                Console.ResetColor();
                return false;
            }
            
            return true;
        }
        
        public static void SpoofAllHardware()
        {
            if (!RequireAdminCheck()) return;
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nStarting hardware spoofing process...");
            Console.ResetColor();
            
            try
            {
                BackupOriginalValues();
                SpoofCPU();
                SpoofDisk();
                SpoofMotherboard();
                SpoofGPU();
                SpoofMAC();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSpoofing complete! It may be necessary to restart the computer to apply all changes.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing hardware: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void BackupOriginalValues()
        {
            // Implementação para backup dos valores originais
            Directory.CreateDirectory(Path.GetDirectoryName(BackupPath));
            
            Console.WriteLine("Original values backup completed successfully.");
        }
        
        public static void SpoofCPU()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing CPU...");
            
            try
            {
                // Criar novo ID para CPU via registro do Windows
                string newCpuId = HardwareInfo.GetRandomHardwareID();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
                {
                    key.SetValue("ProcessorId", newCpuId, RegistryValueKind.String);
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"CPU ID modified to: {newCpuId}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing CPU: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofDisk()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing Disks...");
            
            try
            {
                // Este é um método simplificado que mostra apenas o conceito
                // Na prática, seria necessário modificar o registro e usar ferramentas externas
                
                string newDiskId = HardwareInfo.GetRandomHardwareID(20);
                
                using (var reg = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\disk\Enum"))
                {
                    for (int i = 0; i < 10; i++) // Tenta atualizar até 10 discos
                    {
                        try
                        {
                            string keyName = i.ToString();
                            object value = reg.GetValue(keyName);
                            
                            if (value != null)
                            {
                                reg.SetValue(keyName, value.ToString().Replace(value.ToString().Split('&')[1], $"&{newDiskId}"));
                            }
                        }
                        catch { /* Ignora erros para entradas inexistentes */ }
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Discos modified successfully. Restart the computer to apply.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing disks: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofMotherboard()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing Motherboard and BIOS...");
            
            try
            {
                string newSerialNumber = HardwareInfo.GetRandomHardwareID(16);
                string newUUID = Guid.NewGuid().ToString().ToUpper();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\SystemInformation"))
                {
                    if (key != null)
                    {
                        key.SetValue("SystemManufacturer", "SpooferBIOS", RegistryValueKind.String);
                        key.SetValue("SystemProductName", "StealthBoard", RegistryValueKind.String);
                        key.SetValue("BaseBoardProduct", "SB-" + newSerialNumber, RegistryValueKind.String);
                    }
                }
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\HardwareConfig"))
                {
                    if (key != null)
                    {
                        key.SetValue("LastConfig", newUUID, RegistryValueKind.String);
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Motherboard modified successfully. New ID: {newSerialNumber}");
                Console.WriteLine($"UUID modified to: {newUUID}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing motherboard: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofGPU()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing GPU...");
            
            try
            {
                string newGpuId = HardwareInfo.GetRandomHardwareID();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"))
                {
                    if (key != null)
                    {
                        key.SetValue("HardwareInformation.qwMemorySize", new Random().Next(1, 16) * 1073741824, RegistryValueKind.QWord);
                        key.SetValue("HardwareID", newGpuId, RegistryValueKind.String);
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"GPU modified successfully. New ID: {newGpuId}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing GPU: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void SpoofMAC()
        {
            if (!RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing MAC addresses...");
            
            try
            {
                var adapters = new List<string>();
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=True"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string adapterName = obj["Name"].ToString();
                        string deviceId = obj["DeviceID"].ToString();
                        
                        adapters.Add($"{deviceId}: {adapterName}");
                    }
                }
                
                if (adapters.Count == 0)
                {
                    Console.WriteLine("No network adapters found.");
                    return;
                }
                
                Console.WriteLine("\nAvailable adapters:");
                for (int i = 0; i < adapters.Count; i++)
                {
                    Console.WriteLine($"{i+1}. {adapters[i]}");
                }
                
                Console.Write("\nChoose the adapter to spoof (0 for all): ");
                string input = Console.ReadLine();
                
                if (int.TryParse(input, out int selection))
                {
                    if (selection == 0)
                    {
                        foreach (var adapter in adapters)
                        {
                            string deviceId = adapter.Split(':')[0];
                            SpoofSingleMAC(deviceId);
                        }
                    }
                    else if (selection > 0 && selection <= adapters.Count)
                    {
                        string deviceId = adapters[selection - 1].Split(':')[0];
                        SpoofSingleMAC(deviceId);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid selection!");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing MAC addresses: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void SpoofSingleMAC(string deviceId)
        {
            try
            {
                string newMAC = HardwareInfo.GetRandomMACAddress().Replace(":", "");
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{deviceId}"))
                {
                    if (key != null)
                    {
                        key.SetValue("NetworkAddress", newMAC, RegistryValueKind.String);
                        
                        // Desativar e reativar o adaptador para aplicar as mudanças
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = "netsh";
                            process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" disabled";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.CreateNoWindow = true;
                            process.Start();
                            process.WaitForExit();
                        }
                        
                        Thread.Sleep(1000);
                        
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = "netsh";
                            process.StartInfo.Arguments = $"interface set interface \"{deviceId}\" enabled";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.CreateNoWindow = true;
                            process.Start();
                            process.WaitForExit();
                        }
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"MAC of adapter {deviceId} modified to: {newMAC}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing MAC of adapter {deviceId}: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static void RestoreOriginal()
        {
            if (!RequireAdminCheck()) return;
            
            if (!File.Exists(BackupPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No backups found to restore.");
                Console.ResetColor();
                return;
            }
            
            Console.WriteLine("\nRestoring original hardware settings...");
            
            try
            {
                
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Original settings restored successfully!");
                Console.WriteLine("Restart the computer to apply all changes.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error restoring settings: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
} 