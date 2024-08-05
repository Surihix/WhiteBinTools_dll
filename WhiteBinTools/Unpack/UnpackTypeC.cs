﻿using System;
using System.IO;
using WhiteBinTools.Filelist;
using WhiteBinTools.Support;
using static WhiteBinTools.Support.WhiteBinEnums;

namespace WhiteBinTools.Unpack
{
    public class UnpackTypeC
    {
        public static void UnpackMultiple(GameCodes gameCode, string filelistFile, string whiteBinFile, string whiteVirtualDirPath)
        {
            whiteVirtualDirPath = whiteVirtualDirPath.Replace("*", "");

            var filelistVariables = new FilelistVariables();
            var unpackVariables = new UnpackVariables();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFile);
            UnpackProcesses.PrepareBinVars(whiteBinFile, unpackVariables);

            if (!Directory.Exists(unpackVariables.ExtractDir))
            {
                Directory.CreateDirectory(unpackVariables.ExtractDir);
            }


            FilelistCrypto.DecryptProcess(gameCode, filelistVariables);

            using (var filelistStream = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelistStream))
                {
                    FilelistChunksPrep.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistChunksPrep.BuildChunks(filelistStream, filelistVariables);
                }
            }

            if (gameCode.Equals(GameCodes.ff132))
            {
                filelistVariables.CurrentChunkNumber = -1;
            }

            if (filelistVariables.IsEncrypted)
            {
                CommonMethods.IfFileExistsDel(filelistVariables.TmpDcryptFilelistFile);
                filelistVariables.MainFilelistFile = filelistFile;
            }


            var hasExtracted = false;

            using (var entriesStream = new MemoryStream())
            {
                entriesStream.Write(filelistVariables.EntriesData, 0, filelistVariables.EntriesData.Length);
                entriesStream.Seek(0, SeekOrigin.Begin);

                using (var entriesReader = new BinaryReader(entriesStream))
                {

                    // Extracting files section 
                    long entriesReadPos = 0;
                    unpackVariables.CountDuplicates = 0;
                    string[] currentPathDataArray;
                    string assembledDir;

                    for (int f = 0; f < filelistVariables.TotalFiles; f++)
                    {
                        FilelistProcesses.GetCurrentFileEntry(gameCode, entriesReader, entriesReadPos, filelistVariables);
                        entriesReadPos += 8;

                        UnpackProcesses.PrepareExtraction(filelistVariables.PathString, filelistVariables, unpackVariables.ExtractDir);

                        // Extract files from a specific dir
                        currentPathDataArray = filelistVariables.MainPath.Split('\\');
                        assembledDir = string.Empty;

                        foreach (var dir in currentPathDataArray)
                        {
                            assembledDir += dir;
                            assembledDir += "\\";

                            if (assembledDir == whiteVirtualDirPath)
                            {
                                break;
                            }
                        }

                        if (assembledDir == whiteVirtualDirPath)
                        {
                            using (var whiteBinStream = new FileStream(whiteBinFile, FileMode.Open, FileAccess.Read))
                            {
                                if (!Directory.Exists(Path.Combine(unpackVariables.ExtractDir, filelistVariables.DirectoryPath)))
                                {
                                    Directory.CreateDirectory(Path.Combine(unpackVariables.ExtractDir, filelistVariables.DirectoryPath));
                                }
                                if (File.Exists(filelistVariables.FullFilePath))
                                {
                                    File.Delete(filelistVariables.FullFilePath);
                                    unpackVariables.CountDuplicates++;
                                }

                                UnpackProcesses.UnpackFile(filelistVariables, whiteBinStream, unpackVariables);
                            }

                            hasExtracted = true;

                            Console.WriteLine(unpackVariables.UnpackedState + " _" + Path.Combine(unpackVariables.ExtractDirName, filelistVariables.MainPath));
                        }
                    }
                }
            }

            if (!hasExtracted)
            {
                Console.WriteLine("Specified directory does not exist. please specify the correct directory.");
            }
            else
            {
                Console.WriteLine("\nFinished unpacking multiple files from " + "\"" + unpackVariables.WhiteBinName + "\"");

                if (unpackVariables.CountDuplicates > 0)
                {
                    Console.WriteLine(unpackVariables.CountDuplicates + " duplicate file(s)");
                }
            }
        }
    }
}