using System;
using System.Collections.Generic;
using System.IO;
using WhiteBinTools.Filelist;
using WhiteBinTools.Support;
using static WhiteBinTools.Support.WhiteBinEnums;

namespace WhiteBinTools.Repack
{
    public class RepackTypeB
    {
        public static void RepackSingle(GameCodes gameCode, string filelistFile, string whiteBinFile, string whiteFilePath, bool bckup)
        {
            var extractedDir = Path.Combine(Path.GetDirectoryName(whiteBinFile), "_" + Path.GetFileName(whiteBinFile));

            var filelistVariables = new FilelistVariables();
            var repackVariables = new RepackVariables();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFile);
            RepackProcesses.PrepareRepackVars(repackVariables, filelistFile, filelistVariables, extractedDir);

            if (bckup)
            {
                Console.WriteLine("\nBacking up filelist and image bin files....\n");
                RepackProcesses.CreateFilelistBackup(filelistFile, repackVariables);
                RepackProcesses.CreateWhiteBinBackup(whiteBinFile, repackVariables);
            }


            FilelistCrypto.DecryptProcess(gameCode, filelistVariables);

            using (var filelistStream = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelistStream))
                {
                    FilelistChunksPrep.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistChunksPrep.BuildChunks(filelistStream, filelistVariables);

                    if (filelistVariables.IsEncrypted)
                    {
                        filelistStream.Seek(0, SeekOrigin.Begin);
                        filelistVariables.EncryptedHeaderData = new byte[32];
                        filelistStream.Read(filelistVariables.EncryptedHeaderData, 0, 32);

                        filelistStream.Dispose();
                        File.Delete(filelistVariables.MainFilelistFile);
                    }
                }
            }

            CommonMethods.IfFileExistsDel(filelistFile);

            if (gameCode.Equals(GameCodes.ff132))
            {
                filelistVariables.CurrentChunkNumber = -1;
            }

            // Build an empty dictionary
            // for the chunks 
            var newChunksDict = new Dictionary<int, List<byte>>();
            RepackProcesses.CreateEmptyNewChunksDict(filelistVariables, newChunksDict);


            using (var entriesStream = new MemoryStream())
            {
                entriesStream.Write(filelistVariables.EntriesData, 0, filelistVariables.EntriesData.Length);
                entriesStream.Seek(0, SeekOrigin.Begin);

                using (var entriesReader = new BinaryReader(entriesStream))
                {

                    // Repacking files section
                    long entriesReadPos = 0;
                    var packedAs = "";
                    for (int f = 0; f < filelistVariables.TotalFiles; f++)
                    {
                        FilelistProcesses.GetCurrentFileEntry(gameCode, entriesReader, entriesReadPos, filelistVariables);
                        entriesReadPos += 8;

                        RepackProcesses.GetPackedState(filelistVariables.PathString, repackVariables, extractedDir);

                        repackVariables.AsciiFilePos = repackVariables.ConvertedOgStringData[0];
                        repackVariables.AsciiUnCmpSize = repackVariables.ConvertedOgStringData[1];
                        repackVariables.AsciiCmpSize = repackVariables.ConvertedOgStringData[2];

                        // Repack a specific file
                        var currentFilePath = Path.Combine(repackVariables.OgDirectoryPath, repackVariables.OgFileName);
                        if (currentFilePath == whiteFilePath)
                        {
                            if (repackVariables.WasCompressed)
                            {
                                RepackProcesses.CleanOldFile(repackVariables.NewWhiteBinFile, repackVariables.OgFilePos, repackVariables.OgCmpSize);

                                var zlibTmpCmpData = ZlibMethods.ZlibCompress(repackVariables.OgFullFilePath);
                                var zlibCmpFileSize = (uint)zlibTmpCmpData.Length;

                                if (zlibCmpFileSize < repackVariables.OgCmpSize || zlibCmpFileSize == repackVariables.OgCmpSize)
                                {
                                    RepackProcesses.InjectProcess(repackVariables, ref packedAs);
                                }
                                else
                                {
                                    RepackProcesses.AppendProcess(repackVariables, ref packedAs);
                                }
                            }
                            else
                            {
                                RepackProcesses.CleanOldFile(repackVariables.NewWhiteBinFile, repackVariables.OgFilePos, repackVariables.OgUnCmpSize);

                                var dummyFileSize = (uint)new FileInfo(repackVariables.OgFullFilePath).Length;

                                if (dummyFileSize < repackVariables.OgUnCmpSize || dummyFileSize == repackVariables.OgUnCmpSize)
                                {
                                    RepackProcesses.InjectProcess(repackVariables, ref packedAs);
                                }
                                else
                                {
                                    RepackProcesses.AppendProcess(repackVariables, ref packedAs);
                                }
                            }

                            Console.WriteLine(repackVariables.RepackState + " " + Path.Combine(repackVariables.NewWhiteBinFileName, repackVariables.RepackLogMsg) + " " + packedAs);
                        }

                        RepackProcesses.BuildPathForChunk(repackVariables, gameCode, filelistVariables, newChunksDict);
                    }
                }
            }


            Console.WriteLine("\nBuilding filelist....");
            RepackFilelistData.BuildFilelist(filelistVariables, newChunksDict, repackVariables, gameCode);

            if (filelistVariables.IsEncrypted)
            {
                FilelistCrypto.EncryptProcess(repackVariables);
            }

            Console.WriteLine("\nFinished repacking a file into " + "\"" + repackVariables.NewWhiteBinFileName + "\"");
        }
    }
}