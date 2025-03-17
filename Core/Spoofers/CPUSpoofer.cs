using System;
using Microsoft.Win32;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Spoofers
{
    /// <summary>
    /// Class responsible for CPU spoofing
    /// </summary>
    public static class CpuSpoofer
    {
        /// <summary>
        /// Performs CPU spoofing
        /// </summary>
        public static void SpoofCPU()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Logger.Instance.Info("Starting CPU spoofing");
            Console.WriteLine("\nSpoofing CPU...");
            
            try
            {
                string newCpuId = HardwareInfo.GetRandomHardwareID();
                Logger.Instance.Debug($"Generated new CPU ID: {newCpuId}");
                
                string registryKey = RegistryHelper.REG_PATH_GPU;
                Logger.Instance.LogRegistryOperation(registryKey, RegistryHelper.PROP_PROCESSOR_ID, "MODIFY");
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryKey))
                {
                    key.SetValue(RegistryHelper.PROP_PROCESSOR_ID, newCpuId, RegistryValueKind.String);
                }
                
                Logger.Instance.Info($"CPU ID spoofed successfully to: {newCpuId}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"CPU ID modified to: {newCpuId}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex, "Error spoofing CPU");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error spoofing CPU: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
} 