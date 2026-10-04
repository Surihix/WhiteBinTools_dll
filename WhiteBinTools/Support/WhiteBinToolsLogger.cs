using System;

namespace WhiteBinTools.Support
{
    internal class WhiteBinToolsLogger
    {
        public static event Action<string> LogWritten;
        public static void WriteLog(string message)
        {
            Console.WriteLine(message);
            LogWritten.Invoke(message);
        }
    }
}