using System;
using System.IO;
using WhiteBinTools.FilelistClasses;
using WhiteBinTools.SupportClasses;

namespace WhiteBinTools.UnpackClasses
{
    public class UnpackTypeA
    {
        public static void UnpackFull(CmnEnums.GameCodes gameCodeVar, string filelistFileVar, string whiteBinFileVar)
        {
            var filelistVariables = new FilelistProcesses();
            var unpackVariables = new UnpackProcess();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFileVar);
            UnpackProcess.PrepareBinVars(whiteBinFileVar, unpackVariables);

            filelistVariables.DefaultChunksExtDir = unpackVariables.ExtractDir + "\\_chunks";
            filelistVariables.ChunkFile = filelistVariables.DefaultChunksExtDir + "\\chunk_";

            if (Directory.Exists(unpackVariables.ExtractDir))
            {
                Console.WriteLine("Detected previous unpack. deleting....");
                unpackVariables.ExtractDir.IfDirExistsDel();
            }

            Directory.CreateDirectory(unpackVariables.ExtractDir);
            Directory.CreateDirectory(filelistVariables.DefaultChunksExtDir);


            FilelistProcesses.DecryptProcess(gameCodeVar, filelistVariables);

            using (var filelist = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelist))
                {
                    FilelistProcesses.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistProcesses.UnpackChunks(filelist, filelistVariables.ChunkFile, filelistVariables);
                }
            }

            if (filelistVariables.IsEncrypted.Equals(true))
            {
                filelistVariables.TmpDcryptFilelistFile.IfFileExistsDel();
                filelistVariables.MainFilelistFile = filelistFileVar;
            }


            // Extracting files section 
            filelistVariables.ChunkFNameCount = 0;
            unpackVariables.CountDuplicates = 0;
            for (int ch = 0; ch < filelistVariables.TotalChunks; ch++)
            {
                var filesInChunkCount = FilelistProcesses.GetFilesInChunkCount(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount);

                // Open a chunk file for reading
                using (var currentChunk = new FileStream(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount, FileMode.Open, FileAccess.Read))
                {
                    using (var chunkStringReader = new BinaryReader(currentChunk))
                    {
                        var chunkStringReaderPos = (uint)0;
                        for (int f = 0; f < filesInChunkCount; f++)
                        {
                            var convertedString = chunkStringReader.BinaryToString(chunkStringReaderPos);

                            if (convertedString.Equals("end") || convertedString.Equals(" ") || convertedString.Equals(null))
                            {
                                break;
                            }

                            UnpackProcess.PrepareExtraction(convertedString, filelistVariables, unpackVariables.ExtractDir);

                            // Extract all files
                            using (var whiteBin = new FileStream(whiteBinFileVar, FileMode.Open, FileAccess.Read))
                            {
                                if (!Directory.Exists(unpackVariables.ExtractDir + "\\" + filelistVariables.DirectoryPath))
                                {
                                    Directory.CreateDirectory(unpackVariables.ExtractDir + "\\" + filelistVariables.DirectoryPath);
                                }
                                if (File.Exists(filelistVariables.FullFilePath))
                                {
                                    File.Delete(filelistVariables.FullFilePath);
                                    unpackVariables.CountDuplicates++;
                                }

                                UnpackProcess.UnpackFile(filelistVariables, whiteBin, unpackVariables);
                            }

                            Console.WriteLine(unpackVariables.UnpackedState + " _" + unpackVariables.ExtractDirName + "\\" + filelistVariables.MainPath);

                            chunkStringReaderPos = (uint)chunkStringReader.BaseStream.Position;
                        }
                    }
                }

                filelistVariables.ChunkFNameCount++;
            }

            Directory.Delete(filelistVariables.DefaultChunksExtDir, true);

            Console.WriteLine("\nFinished extracting file " + unpackVariables.WhiteBinName);

            if (unpackVariables.CountDuplicates > 1)
            {
                Console.WriteLine(unpackVariables.CountDuplicates + " duplicate file(s)");
            }
        }
    }
}