using System;

namespace Core
{
    public static class Logger
    {
        public static string LastLine;
        public static void Log(string message)
        {
            LastLine = message;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {message}");
        }

        public static void LogTimeDiff(DateTime time)
        {
            Log($"Took {Math.Round((DateTime.Now - time).TotalMilliseconds / 1000, 2)} seconds.");
        }
    }
}
