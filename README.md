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

## Building and Running

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or higher
- Visual Studio 2022 (recommended) or any text editor
- Git (optional, for cloning the repository)

### Building from Source

#### Using Command Line
1. Clone or download the repository:
   ```
   git clone https://github.com/yourusername/StealthSpoof.git
   cd StealthSpoof
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

#### Using Visual Studio
1. Open the solution file (`StealthSpoof.sln`) in Visual Studio
2. Build the solution by pressing `Ctrl+Shift+B` or selecting `Build > Build Solution` from the menu
3. Run the application by pressing `F5` or selecting `Debug > Start Debugging`

### Running the Application

1. Make sure to run the application with administrator privileges:
   - If using the command line, open a command prompt as administrator before running `dotnet run`
   - If using Visual Studio, right-click on Visual Studio and select "Run as administrator" before opening the project

2. Once the application starts, you'll see the main menu with various options:
   - Option 1: View current hardware information
   - Option 2: Spoof all hardware identifiers
   - Option 3: Restore original settings
   - Option 4: Advanced options (for specific component spoofing)
   - Option 5: Diagnostics
   - Option 0: Exit

3. Follow the on-screen instructions to navigate through the menus and perform the desired operations

### Troubleshooting

- If you encounter "Access denied" errors, ensure you're running the application as administrator
- Check the logs located in `%APPDATA%\StealthSpoof\logs` for detailed error information
- Make sure all prerequisites are installed correctly, especially the .NET 6.0 SDK

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
