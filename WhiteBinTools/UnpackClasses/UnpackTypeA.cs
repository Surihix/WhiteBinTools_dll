using System;
using System.IO;
using WhiteBinTools.FilelistClasses;
using WhiteBinTools.SupportClasses;
using static WhiteBinTools.SupportClasses.ProgramEnums;

namespace WhiteBinTools.UnpackClasses
{
    public class UnpackTypeA
    {
        public static void UnpackFull(GameCodes gameCodeVar, string filelistFile, string whiteBinFile)
        {
            var filelistVariables = new FilelistVariables();
            var unpackVariables = new UnpackVariables();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFile);
            UnpackProcess.PrepareBinVars(whiteBinFile, unpackVariables);

            filelistVariables.DefaultChunksExtDir = Path.Combine(unpackVariables.ExtractDir, "_chunks");
            filelistVariables.ChunkFile = Path.Combine(filelistVariables.DefaultChunksExtDir, "chunk_");

            if (Directory.Exists(unpackVariables.ExtractDir))
            {
                Console.WriteLine("Detected previous unpack. deleting....");
                unpackVariables.ExtractDir.IfDirExistsDel();
            }

            Directory.CreateDirectory(unpackVariables.ExtractDir);
            Directory.CreateDirectory(filelistVariables.DefaultChunksExtDir);


            FilelistProcesses.DecryptProcess(gameCodeVar, filelistVariables);

            using (var filelistStream = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelistStream))
                {
                    FilelistChunksPrep.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistChunksPrep.UnpackChunks(filelistStream, filelistVariables.ChunkFile, filelistVariables);
                }
            }

            if (filelistVariables.IsEncrypted)
            {
                filelistVariables.TmpDcryptFilelistFile.IfFileExistsDel();
                filelistVariables.MainFilelistFile = filelistFile;
            }


            using (var whiteBinStream = new FileStream(whiteBinFile, FileMode.Open, FileAccess.Read))
            {
                // Extracting files section 
                filelistVariables.ChunkFNameCount = 0;
                unpackVariables.CountDuplicates = 0;

                for (int ch = 0; ch < filelistVariables.TotalChunks; ch++)
                {
                    var filesInChunkCount = FilelistProcesses.GetFilesInChunkCount(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount);

                    // Open a chunk file for reading
                    using (var currentChunkStream = new FileStream(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount, FileMode.Open, FileAccess.Read))
                    {
                        using (var chunkStringReader = new BinaryReader(currentChunkStream))
                        {

                            var chunkStringReaderPos = (uint)0;
                            for (int f = 0; f < filesInChunkCount; f++)
                            {
                                var convertedString = chunkStringReader.BinaryToString(chunkStringReaderPos);

                                if (convertedString == "end" || convertedString == " " || convertedString == null)
                                {
                                    break;
                                }

                                UnpackProcess.PrepareExtraction(convertedString, filelistVariables, unpackVariables.ExtractDir);

                                // Extract all files
                                if (!Directory.Exists(Path.Combine(unpackVariables.ExtractDir, filelistVariables.DirectoryPath)))
                                {
                                    Directory.CreateDirectory(Path.Combine(unpackVariables.ExtractDir, filelistVariables.DirectoryPath));
                                }
                                if (File.Exists(filelistVariables.FullFilePath))
                                {
                                    File.Delete(filelistVariables.FullFilePath);
                                    unpackVariables.CountDuplicates++;
                                }

                                UnpackProcess.UnpackFile(filelistVariables, whiteBinStream, unpackVariables);

                                Console.WriteLine(unpackVariables.UnpackedState + " _" +  Path.Combine(unpackVariables.ExtractDirName, filelistVariables.MainPath));

                                chunkStringReaderPos = (uint)chunkStringReader.BaseStream.Position;
                            }
                        }
                    }

                    filelistVariables.ChunkFNameCount++;
                }
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