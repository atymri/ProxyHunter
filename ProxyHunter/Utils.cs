using System;

namespace ProxyHunter
{
    /// <summary>
    /// Utility helper functions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Clears the console screen.
        /// </summary>
        public static void ClearScreen()
        {
            Console.Clear();
        }
    }

    /// <summary>
    /// Stores the banner ASCII art for the app header.
    /// </summary>
    public static class Banner
    {
        public static string Text => @"
            ProxyHunter - Version 1.0
        ";
    }
}
