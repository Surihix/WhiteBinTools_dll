using System;
using System.IO;
using System.Text;
using WhiteBinTools.Filelist;
using WhiteBinTools.Support;
using static WhiteBinTools.Support.Enumerators;

namespace WhiteBinTools.Unpack
{
    public class UnpackTypeA
    {
        public static void UnpackFull(GameCode gameCode, string filelistFile, string whiteBinFile)
        {
            var filelistLoadData = FilelistLoader.LoadFilelist(gameCode, filelistFile);

            var filelistHeader = filelistLoadData.FilelistHeader;
            var filelistEntryV1Table = filelistLoadData.FilelistEntryV1Table;
            var filelistEntryV2Table = filelistLoadData.FilelistEntryV2Table;
            var filelistChunks = filelistLoadData.FilelistChunks;

            var whiteBinName = Path.GetFileName(whiteBinFile);
            var unpackDir = Path.Combine(Path.GetDirectoryName(whiteBinFile), $"_{whiteBinName}");

            if (Directory.Exists(unpackDir))
            {
                WhiteBinToolsLogger.WriteLog("Detected previous unpack. deleting....");
                WhiteBinToolsLogger.WriteLog("");

                SharedFunctions.IfDirExistsDel(unpackDir);
            }

            Directory.CreateDirectory(unpackDir);

            var duplicateCounter = 0;
            var dirgePathsLog = new string[filelistHeader.FileCount];

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
                    var unpackedState = UnpackHelper.UnpackFile(whiteFileInfoData, unpackDir, ref duplicateCounter, whiteBinStream);

                    if (gameCode == GameCode.dirge)
                    {
                        var sb = new StringBuilder();

                        _ = sb.Append($"FileCode:[{fileCode.ToString()}] ");
                        _ = sb.Append($"Generated:[{whiteFileInfoData.IsPathGenerated}] ");
                        _ = sb.Append($"Path: {whiteFileInfoData.FilePath}");

                        dirgePathsLog[i] = sb.ToString();
                    }

                    WhiteBinToolsLogger.WriteLog($"{unpackedState} _{Path.Combine(whiteBinName, whiteFileInfoData.FilePath)}");
                }

                if (gameCode == GameCode.dirge)
                {
                    WhiteBinToolsLogger.WriteLog("");
                    WhiteBinToolsLogger.WriteLog("Writing dirge unpacked paths log....");
                    WhiteBinToolsLogger.WriteLog("");

                    var dirgePathsTxtFile = Path.Combine(Path.GetDirectoryName(whiteBinFile), $"{whiteBinName}_unpacked_paths.txt");
                    SharedFunctions.IfFileExistsDel(dirgePathsTxtFile);

                    using (var dirgePathsLogWriter = new StreamWriter(dirgePathsTxtFile, true))
                    {
                        for (int i = 0; i < dirgePathsLog.Length; i++)
                        {
                            dirgePathsLogWriter.WriteLine(dirgePathsLog[i]);
                        }
                    }
                }

                WhiteBinToolsLogger.WriteLog("");
                WhiteBinToolsLogger.WriteLog($"Finished unpacking \"{whiteBinName}\"");

                if (duplicateCounter > 1)
                {
                    WhiteBinToolsLogger.WriteLog($"{duplicateCounter} duplicate file(s)");
                }
            }
        }
    }
}