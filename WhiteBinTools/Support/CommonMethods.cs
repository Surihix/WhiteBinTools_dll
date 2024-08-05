using System;
using System.IO;

namespace WhiteBinTools.Support
{
    internal static class CommonMethods
    {
        public static void ErrorExit(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            Environment.Exit(1);
        }


        public static void IfFileExistsDel(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        public static void IfDirExistsDel(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }        
    }
}