using System;
using System.IO;

namespace WhiteBinTools.SupportClasses
{
    public static class IOhelpers
    {
        public static void IfFileExistsDel(this string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        public static void IfDirExistsDel(this string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }


        public static void ExCopyTo(this Stream source, Stream destination, long offset, long count, int bufferSize = 81920)
        {
            var returnAddress = source.Position;
            source.Seek(offset, SeekOrigin.Begin);

            var bytesRemaining = count;
            while (bytesRemaining > 0)
            {
                var readSize = Math.Min(bufferSize, bytesRemaining);
                var buffer = new byte[readSize];
                _ = source.Read(buffer, 0, (int)readSize);

                destination.Write(buffer, 0, (int)readSize);
                bytesRemaining -= readSize;
            }

            source.Seek(returnAddress, SeekOrigin.Begin);
        }
    }
}