using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Collections.Generic;

/*
    This class is responsible for showing the current hardware information of the computer.
    It is used to show the user the current hardware information of the computer.
    It is also used to show the user the current hardware information of the computer to avoid detection by antivirus software.
*/

namespace StealthSpoof.Core
{
    public static class HardwareInfo
    {
        // Constants for WMI property names
        private const string PROP_NAME = "Name";
        private const string PROP_MANUFACTURER = "Manufacturer";
        private const string PROP_PROCESSOR_ID = "ProcessorId";
        private const string PROP_SERIAL_NUMBER = "SerialNumber";
        private const string PROP_PRODUCT = "Product";
        private const string PROP_VERSION = "Version";
        private const string PROP_MODEL = "Model";
        private const string PROP_ADAPTER_RAM = "AdapterRAM";
        private const string PROP_DRIVER_VERSION = "DriverVersion";
        private const string PROP_CAPACITY = "Capacity";
        private const string PROP_DEVICE_LOCATOR = "DeviceLocator";
        private const string PROP_SIZE = "Size";

        public static void ShowCurrentHardwareInfo()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n======= Current Hardware Info =======");
            Console.ResetColor();
            
            try
            {
                // CPU Info
                Console.WriteLine("\n[CPU]");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"CPU: {obj[PROP_NAME]}");
                        Console.WriteLine($"ID: {obj[PROP_PROCESSOR_ID]}");
                        Console.WriteLine($"Manufacturer: {obj[PROP_MANUFACTURER]}");
                    }
                }
                
                // Motherboard Info
                Console.WriteLine("\n[Motherboard]");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Manufacturer: {obj[PROP_MANUFACTURER]}");
                        Console.WriteLine($"Model: {obj[PROP_PRODUCT]}");
                        Console.WriteLine($"Serial: {obj[PROP_SERIAL_NUMBER]}");
                    }
                }
                
                // BIOS Info
                Console.WriteLine("\n[BIOS]");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Manufacturer: {obj[PROP_MANUFACTURER]}");
                        Console.WriteLine($"Version: {obj[PROP_VERSION]}");
                        Console.WriteLine($"Serial: {obj[PROP_SERIAL_NUMBER]}");
                    }
                }
                
                // Disk Info
                Console.WriteLine("\n[Disks]");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"Model: {obj[PROP_MODEL]}");
                        Console.WriteLine($"Serial: {obj[PROP_SERIAL_NUMBER]}");
                        Console.WriteLine($"Size: {Convert.ToInt64(obj[PROP_SIZE]) / 1073741824} GB");
                        Console.WriteLine("---");
                    }
                }
                
                // GPU Info
                Console.WriteLine("\n[GPU]");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Console.WriteLine($"GPU: {obj[PROP_NAME]}");
                        Console.WriteLine($"Adapter RAM: {Convert.ToInt64(obj[PROP_ADAPTER_RAM]) / 1048576} MB");
                        Console.WriteLine($"Driver Version: {obj[PROP_DRIVER_VERSION]}");
                        Console.WriteLine("---");
                    }
                }
                
                // Network Adapters
                Console.WriteLine("\n[Network Adapters]");
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    string mac = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");
                    if (!string.IsNullOrEmpty(mac) && mac != "00:00:00:00:00:00")
                    {
                        Console.WriteLine($"Adapter: {nic.Description}");
                        Console.WriteLine($"Name: {nic.Name}");
                        Console.WriteLine($"MAC: {mac}");
                        Console.WriteLine($"Status: {nic.OperationalStatus}");
                        Console.WriteLine("---");
                    }
                }
                
                // RAM Info
                Console.WriteLine("\n[RAM]");
                ulong totalMemory = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        ulong capacity = Convert.ToUInt64(obj[PROP_CAPACITY]);
                        totalMemory += capacity;
                        
                        Console.WriteLine($"Manufacturer: {obj[PROP_MANUFACTURER]}");
                        Console.WriteLine($"Capacity: {capacity / 1048576} MB");
                        Console.WriteLine($"Serial: {obj[PROP_SERIAL_NUMBER]}");
                        Console.WriteLine($"Slot: {obj[PROP_DEVICE_LOCATOR]}");
                        Console.WriteLine("---");
                    }
                }
                Console.WriteLine($"Total Memory: {totalMemory / 1073741824} GB");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error showing hardware information: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        public static string GetRandomHardwareID(int length = 16)
        {
            Random random = new Random();
            const string chars = "ABCDEF0123456789";
            var array = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                array[i] = chars[random.Next(chars.Length)];
            }
            
            return new string(array);
        }
        
        public static string GetRandomMACAddress()
        {
            Random random = new Random();
            byte[] mac = new byte[6];
            random.NextBytes(mac);
            
            // Ensure the address is not multicast (the least significant bit of the first byte must be 0)
            mac[0] = (byte)(mac[0] & 0xFE);
            
            return BitConverter.ToString(mac).Replace("-", ":");
        }
    }
} 