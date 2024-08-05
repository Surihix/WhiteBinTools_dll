using System;
using System.Collections.Generic;
using System.IO;
using WhiteBinTools.Filelist;
using WhiteBinTools.Support;
using static WhiteBinTools.Support.LibaryEnums;

namespace WhiteBinTools.Repack
{
    public class RepackTypeA
    {
        public static void RepackAll(GameCodes gameCode, string filelistFile, string extractedDir, bool bckup)
        {
            var filelistVariables = new FilelistVariables();
            var repackVariables = new RepackVariables();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFile);
            RepackProcesses.PrepareRepackVars(repackVariables, filelistFile, filelistVariables, extractedDir);

            if (bckup)
            {
                RepackProcesses.CreateFilelistBackup(filelistFile, repackVariables);

                repackVariables.OldWhiteBinFileBackup = repackVariables.NewWhiteBinFile + ".bak";
                CommonMethods.IfFileExistsDel(repackVariables.OldWhiteBinFileBackup);

                if (File.Exists(repackVariables.NewWhiteBinFile))
                {
                    File.Move(repackVariables.NewWhiteBinFile, repackVariables.OldWhiteBinFileBackup);
                }
            }

            CommonMethods.IfFileExistsDel(repackVariables.NewWhiteBinFile);

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


            using (var newWhiteBinStream = new FileStream(repackVariables.NewWhiteBinFile, FileMode.Append, FileAccess.Write))
            {
                using (var entriesStream = new MemoryStream())
                {
                    entriesStream.Write(filelistVariables.EntriesData, 0, filelistVariables.EntriesData.Length);
                    entriesStream.Seek(0, SeekOrigin.Begin);

                    using (var entriesReader = new BinaryReader(entriesStream))
                    {

                        // Repacking files section
                        long entriesReadPos = 0;
                        for (int f = 0; f < filelistVariables.TotalFiles; f++)
                        {
                            FilelistProcesses.GetCurrentFileEntry(gameCode, entriesReader, entriesReadPos, filelistVariables);
                            entriesReadPos += 8;

                            RepackProcesses.GetPackedState(filelistVariables.PathString, repackVariables, extractedDir);

                            if (!File.Exists(repackVariables.OgFullFilePath))
                            {
                                var fullFilePathDir = Path.GetDirectoryName(repackVariables.OgFullFilePath);
                                if (!Directory.Exists(fullFilePathDir))
                                {
                                    Directory.CreateDirectory(fullFilePathDir);
                                }

                                var createDummyFile = File.Create(repackVariables.OgFullFilePath);
                                createDummyFile.Close();
                            }

                            RepackProcesses.RepackTypeAppend(repackVariables, newWhiteBinStream, repackVariables.OgFullFilePath);

                            RepackProcesses.BuildPathForChunk(repackVariables, gameCode, filelistVariables, newChunksDict);

                            Console.WriteLine(repackVariables.RepackState + " " + Path.Combine(repackVariables.NewWhiteBinFileName, repackVariables.RepackLogMsg));
                        }
                    }
                }
            }


            Console.WriteLine("\nBuilding filelist....");
            RepackFilelistData.BuildFilelist(filelistVariables, newChunksDict, repackVariables, gameCode);

            if (filelistVariables.IsEncrypted)
            {
                FilelistCrypto.EncryptProcess(repackVariables);
            }

            Console.WriteLine("\nFinished repacking files to " + "\"" + repackVariables.NewWhiteBinFileName + "\"");
        }
    }
}