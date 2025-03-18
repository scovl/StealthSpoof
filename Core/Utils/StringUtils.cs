using System;
using System.Text;
using System.Linq;

namespace StealthSpoof.Core.Utils
{
    /// <summary>
    /// Utility class for string operations and random text generation
    /// </summary>
    public static class StringUtils
    {
        private static readonly Random _random = new Random();
        
        // Common words that can be used for hostname generation
        private static readonly string[] _hostnameWords = new string[]
        {
            "desktop", "laptop", "work", "home", "office", "dev", "test", 
            "main", "user", "win", "pc", "system", "station", "box", "client",
            "local", "private", "secure", "lab", "net"
        };
        
        // Common workgroup names
        private static readonly string[] _workgroupPrefixes = new string[]
        {
            "WORKGROUP", "HOME", "OFFICE", "CORP", "COMPANY", "BUSINESS", 
            "ENTERPRISE", "LOCAL", "PRIVATE", "NETWORK", "DOMAIN"
        };
        
        /// <summary>
        /// Generates a random hostname with mixed alphanumeric characters
        /// </summary>
        /// <returns>A randomly generated hostname</returns>
        public static string GenerateRandomHostname()
        {
            // 50% chance to use a word-based hostname
            if (_random.Next(2) == 0)
            {
                string word = _hostnameWords[_random.Next(_hostnameWords.Length)];
                string numbers = _random.Next(10, 9999).ToString();
                return $"{word.ToUpper()}-{numbers}";
            }
            
            // Otherwise generate a completely random string
            StringBuilder sb = new StringBuilder();
            
            // Length between 5 and 12 characters
            int length = _random.Next(5, 13);
            
            // Start with a letter (requirement for hostnames)
            sb.Append((char)(_random.Next(26) + 'A'));
            
            // Add remaining characters (mix of uppercase and digits)
            for (int i = 1; i < length; i++)
            {
                // 70% chance for a letter, 30% chance for a digit
                if (_random.Next(10) < 7)
                {
                    sb.Append((char)(_random.Next(26) + 'A'));
                }
                else
                {
                    sb.Append(_random.Next(10).ToString());
                }
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Generates a random workgroup name
        /// </summary>
        /// <returns>A randomly generated workgroup name</returns>
        public static string GenerateRandomWorkgroup()
        {
            string prefix = _workgroupPrefixes[_random.Next(_workgroupPrefixes.Length)];
            
            // 50% chance to just return the prefix
            if (_random.Next(2) == 0)
            {
                return prefix;
            }
            
            // Otherwise add a number
            string suffix = _random.Next(1, 999).ToString();
            return $"{prefix}{suffix}";
        }
        
        /// <summary>
        /// Generates a random alphanumeric string of the specified length
        /// </summary>
        /// <param name="length">The desired length of the string</param>
        /// <param name="includeSpecialChars">Whether to include special characters</param>
        /// <returns>A randomly generated string</returns>
        public static string GenerateRandomString(int length, bool includeSpecialChars = false)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            
            string charset = includeSpecialChars ? chars + specialChars : chars;
            
            return new string(Enumerable.Repeat(charset, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
 