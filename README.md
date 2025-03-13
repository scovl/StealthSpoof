# StealthSpoof

A Windows hardware ID spoofer application designed to modify hardware identifiers for privacy and security purposes.

## Overview

StealthSpoof is a C# application that allows users to modify hardware identifiers on Windows systems. It can spoof various hardware components including CPU, GPU, motherboard, disk drives, and network adapters to help protect privacy and prevent hardware-based tracking.

## Features

- **Hardware Spoofing**: Modify identifiers for multiple hardware components:
  - CPU ID
  - GPU information
  - Motherboard/BIOS serials
  - Disk drive identifiers
  - MAC addresses

- **Backup & Restore**: Automatically backs up original hardware values before making changes, allowing for easy restoration.

- **User-Friendly Interface**: Simple console interface with clear options and status messages.

- **Detailed Logging**: Comprehensive logging system to track operations and troubleshoot issues.

## Requirements

- Windows 10 or later
- .NET 6.0 or higher
- Administrator privileges (required for registry modifications)

## Usage

1. Run the application as Administrator (required for hardware modifications)
2. Choose from the available options:
   - View current hardware information
   - Spoof all hardware identifiers
   - Restore original hardware settings

## Technical Details

StealthSpoof works by modifying Windows registry entries and system information that store hardware identifiers. It uses:

- Windows Management Instrumentation (WMI) for hardware detection
- Registry modifications for persistent changes
- Network interface manipulation for MAC address changes

## Disclaimer

This software is provided for educational and research purposes only. Use responsibly and only on systems you own or have permission to modify. Modifying hardware identifiers may violate terms of service for some applications and services.

## License

This project is available for personal, non-commercial use only. See the license file for details.
