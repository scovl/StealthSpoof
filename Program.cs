using System;
using StealthSpoof.Core;

/*
    This is the main class of the program.
    It is responsible for initializing the application and starting the main loop.
*/

namespace StealthSpoof
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Initialize logger at the start
            Logger.Instance.Info("StealthSpoof application started");
            
            // Check log file size
            Logger.Instance.CheckLogFileSize();
            
            // Check if the environment is compatible
            if (!EnvironmentChecker.CheckEnvironment())
                return;
            
            // Display the application header
            UIHelper.DisplayHeader();
            
            // Start the main menu loop
            MenuManager.RunMainLoop();
            
            // Application is exiting
            Logger.Instance.Info("StealthSpoof application exiting");
        }
    }
} 