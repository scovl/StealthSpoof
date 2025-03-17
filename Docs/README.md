# StealthSpoof Documentation

## Overview

StealthSpoof is a hardware ID spoofing tool designed to modify hardware identifiers in the Windows registry. It allows users to change various hardware identifiers to help prevent hardware-based tracking and identification. This tool is particularly useful for privacy-conscious users and for testing software that relies on hardware fingerprinting.

## Features

StealthSpoof offers the following key features:

- **Complete System Spoofing**: Change all hardware identifiers at once
- **Selective Spoofing**: Modify only specific hardware components
- **Backup and Restore**: Save original hardware identifiers and restore them when needed
- **Diagnostics**: View current system information and check registry key accessibility
- **Game Cache Clearing**: Clear cache files for specific games to prevent tracking

## Supported Hardware Components

StealthSpoof can modify identifiers for the following hardware components:

- CPU (Processor ID)
- GPU (Graphics Card)
- Disk Drives
- Motherboard (Serial Number, UUID)
- Network Adapters (MAC Addresses)
- BIOS/UEFI Information
- System Information (PC Name, Installation ID)

## Architecture

StealthSpoof is built with a modular architecture that separates concerns into different components:

### Core Components

- **HardwareSpoofer**: Main entry point for spoofing operations
- **HardwareInfo**: Provides information about current hardware and generates random IDs
- **Logger**: Handles logging of operations and errors
- **MenuManager**: Manages the user interface and menu system
- **UIHelper**: Provides UI-related utility functions
- **EnvironmentChecker**: Verifies system compatibility

### Specialized Components

- **Spoofers**: Contains specialized classes for each hardware component
  - CpuSpoofer: Handles CPU ID spoofing
  - GpuSpoofer: Handles GPU ID spoofing
  - DiskSpoofer: Handles disk ID spoofing
  - MotherboardSpoofer: Handles motherboard information spoofing
  - NetworkSpoofer: Handles MAC address spoofing
  - SystemInfoSpoofer: Handles system information spoofing

- **Utils**: Contains utility classes
  - RegistryHelper: Provides registry-related utility functions
  - DiagnosticsHelper: Provides diagnostic functions

- **Backup**: Contains backup-related functionality
  - BackupManager: Handles backup and restoration of original values

## Usage

### Prerequisites

- Windows 10 or later
- .NET 6.0 Runtime
- Administrator privileges

### Installation

1. Download the latest release from the releases page
2. Extract the ZIP file to a location of your choice
3. Run StealthSpoof.exe as administrator

### Basic Usage

1. Launch the application with administrator privileges
2. Select an option from the main menu:
   - Show current hardware information
   - Spoof EVERYTHING (Recommended)
   - Restore original settings
   - Advanced options
   - Diagnostics

### Advanced Options

The advanced options menu allows you to:

- Spoof individual hardware components
- Spoof PC name or Installation ID
- Clear game caches for specific games

### Backup and Restore

StealthSpoof automatically creates a backup of your original hardware identifiers before making any changes. You can restore these values at any time by selecting "Restore original settings" from the main menu.

## Technical Details

### Registry Modifications

StealthSpoof modifies the following registry keys:

- `SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000` (GPU)
- `SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}` (Network Adapters)
- `SOFTWARE\Microsoft\Windows NT\CurrentVersion` (Windows Installation)
- `SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName` (Computer Name)
- `SYSTEM\CurrentControlSet\Services\disk\Enum` (Disk Drives)
- `SYSTEM\CurrentControlSet\Control\SystemInformation` (System Information)
- `HARDWARE\DESCRIPTION\System\BIOS` (BIOS Information)
- And others...

### Backup Storage

Backups are stored in JSON format at:
`%APPDATA%\StealthSpoof\backup.json`

### Logs

Logs are stored at:
`%APPDATA%\StealthSpoof\Logs\StealthSpoof_yyyy-MM-dd.log`

## Security Considerations

- **Administrator Privileges**: StealthSpoof requires administrator privileges to modify registry keys
- **System Stability**: Modifying hardware identifiers can potentially cause system instability
- **Software Compatibility**: Some software may not function correctly after hardware identifiers are changed
- **Legal Considerations**: Use this tool responsibly and in accordance with applicable laws and terms of service

## Troubleshooting

### Common Issues

1. **Application won't start**: Ensure you're running as administrator
2. **Changes not applied**: Some changes require a system restart to take effect
3. **Software detection**: Some software may use additional methods to detect hardware changes

### Error Recovery

If you encounter issues after spoofing:

1. Use the "Restore original settings" option
2. Restart your computer
3. If restoration fails, manually delete the registry keys that were modified

## Development

### Building from Source

1. Clone the repository
2. Open the solution in Visual Studio 2022 or later
3. Restore NuGet packages
4. Build the solution

### Project Structure

- **Program.cs**: Entry point
- **Core/**: Contains core functionality
- **Core/Spoofers/**: Contains specialized spoofing classes
- **Core/Utils/**: Contains utility classes
- **Core/Backup/**: Contains backup functionality
