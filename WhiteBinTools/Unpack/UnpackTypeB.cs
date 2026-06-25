using System;
using System.IO;
using WhiteBinTools.Filelist;
using static WhiteBinTools.Support.Enumerators;

namespace WhiteBinTools.Unpack
{
    public class UnpackTypeB
    {
        public static void UnpackSingle(GameCode gameCode, string filelistFile, string whiteBinFile, string whiteFilePath)
        {
            var filelistLoadData = FilelistLoader.LoadFilelist(gameCode, filelistFile);

            var filelistHeader = filelistLoadData.FilelistHeader;
            var filelistEntryV1Table = filelistLoadData.FilelistEntryV1Table;
            var filelistEntryV2Table = filelistLoadData.FilelistEntryV2Table;
            var filelistChunks = filelistLoadData.FilelistChunks;

            var whiteBinName = Path.GetFileName(whiteBinFile);
            var unpackDir = Path.Combine(Path.GetDirectoryName(whiteBinFile), $"_{whiteBinName}");

            if (!Directory.Exists(unpackDir))
            {
                Directory.CreateDirectory(unpackDir);
            }

            var hasExtracted = false;

            var duplicateCounter = 0;

            using (var whiteBinStream = new FileStream(whiteBinFile, FileMode.Open, FileAccess.Read))
            {
                var noPathCounter = 0;

                for (int i = 0; i < filelistHeader.FileCount; i++)
                {
                    string whiteFileInfoString;
                    uint fileCode;

                    if (gameCode == GameCode.ff131 || gameCode == GameCode.dirge)
                    {
                        var filelistEntryV1 = filelistEntryV1Table[i];
                        whiteFileInfoString = FilelistLoader.GetWhiteFileInfoString(filelistEntryV1.FileInfoPos, filelistChunks, filelistEntryV1.ChunkID);
                        fileCode = filelistEntryV1.FileCode;
                    }
                    else
                    {
                        var filelistEntryV2 = filelistEntryV2Table[i];
                        whiteFileInfoString = FilelistLoader.GetWhiteFileInfoString(filelistEntryV2.FileInfoPos, filelistChunks, filelistEntryV2.ChunkID);
                        fileCode = filelistEntryV2.FileCode;
                    }

                    var whiteFileInfoData = FilelistLoader.GetWhiteFileInfoData(whiteFileInfoString, gameCode, fileCode, ref noPathCounter);

                    if (whiteFileInfoData.FilePath == whiteFilePath)
                    {
                        var unpackedState = UnpackHelper.UnpackFile(whiteFileInfoData, unpackDir, ref duplicateCounter, whiteBinStream);

                        Console.WriteLine($"{unpackedState} _{Path.Combine(whiteBinName, whiteFileInfoData.FilePath)}");
                        hasExtracted = true;
                    }
                }
            }

            if (hasExtracted)
            {
                Console.WriteLine($"\nFinished unpacking file from \"{whiteBinName}\"");

                if (duplicateCounter > 0)
                {
                    Console.WriteLine($"{duplicateCounter} duplicate file(s)");
                }
            }
            else
            {
                Console.WriteLine("Specified file does not exist. please specify a valid file path.");
            }
        }
    }
}