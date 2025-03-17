using System;
using Microsoft.Win32;
using StealthSpoof.Core.Utils;

namespace StealthSpoof.Core.Spoofers
{
    /// <summary>
    /// Class responsible for GPU spoofing
    /// </summary>
    public static class GpuSpoofer
    {
        /// <summary>
        /// Performs GPU spoofing
        /// </summary>
        public static void SpoofGPU()
        {
            if (!RegistryHelper.RequireAdminCheck()) return;
            
            Console.WriteLine("\nSpoofing GPU...");
            
            try
            {
                string newGpuId = HardwareInfo.GetRandomHardwareID();
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryHelper.REG_PATH_GPU))
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
    }
} 