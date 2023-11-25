using System;
using System.Diagnostics;
using System.IO;
using WhiteBinTools.RepackClasses;
using WhiteBinTools.SupportClasses;

namespace WhiteBinTools.FilelistClasses
{
    public partial class FilelistProcesses
    {
        public static void PrepareFilelistVars(FilelistProcesses filelistVariables, string filelistFileVar)
        {
            filelistVariables.MainFilelistFile = filelistFileVar;

            var inFilelistFilePath = Path.GetFullPath(filelistVariables.MainFilelistFile);
            filelistVariables.MainFilelistDirectory = Path.GetDirectoryName(inFilelistFilePath);
            filelistVariables.TmpDcryptFilelistFile = Path.Combine(filelistVariables.MainFilelistDirectory, "filelist_tmp.bin");
        }


        public static void DecryptProcess(CmnEnums.GameCodes gameCodeVar, FilelistProcesses filelistVariables)
        {
            // Check if the filelist is encrypted
            // or not
            if (gameCodeVar.Equals(CmnEnums.GameCodes.ff132))
            {
                filelistVariables.IsEncrypted = CheckIfEncrypted(filelistVariables.MainFilelistFile);
            }

            // If the filelist is encrypted then decrypt the filelist file
            // by first creating a temp copy of the filelist 
            if (filelistVariables.IsEncrypted.Equals(true))
            {
                filelistVariables.TmpDcryptFilelistFile.IfFileExistsDel();
                File.Copy(filelistVariables.MainFilelistFile, filelistVariables.TmpDcryptFilelistFile);

                var cryptFilelistCode = " filelist";
                FFXiiiCryptTool(" -d ", "\"" + filelistVariables.TmpDcryptFilelistFile + "\"", ref cryptFilelistCode);

                filelistVariables.MainFilelistFile = filelistVariables.TmpDcryptFilelistFile;
            }
        }


        public static bool CheckIfEncrypted(string filelistFileVar)
        {
            var isEncrypted = false;
            using (var encStream = new FileStream(filelistFileVar, FileMode.Open, FileAccess.Read))
            {
                using (var encStreamReader = new BinaryReader(encStream))
                {
                    encStreamReader.BaseStream.Position = 20;
                    var encHeaderNumber = encStreamReader.ReadUInt32();

                    if (encHeaderNumber == 501232760)
                    {
                        isEncrypted = true;
                    }
                }
            }

            return isEncrypted;
        }


        public static uint GetFilesInChunkCount(string chunkToRead)
        {
            var filesInChunkCount = (uint)0;
            using (var fileCountReader = new StreamReader(chunkToRead))
            {
                while (!fileCountReader.EndOfStream)
                {
                    var currentNullChar = fileCountReader.Read();
                    if (currentNullChar == 0)
                    {
                        filesInChunkCount++;
                    }
                }
            }

            return filesInChunkCount;
        }


        public static void EncryptProcess(RepackProcesses repackVariables)
        {
            var filelistDataSize = (uint)0;

            // Check filelist size if divisibile by 8
            // and pad in null bytes if not divisible.
            // Then write some null bytes for the size 
            // and hash offsets
            using (var preEncryptedfilelist = new FileStream(repackVariables.NewFilelistFile, FileMode.Append, FileAccess.Write))
            {
                filelistDataSize = (uint)preEncryptedfilelist.Length - 32;

                if (filelistDataSize % 8 != 0)
                {
                    // Get remainder from the division and
                    // reduce the remainder with 8. set that
                    // reduced value to a variable
                    var remainder = filelistDataSize % 8;
                    var increaseByteAmount = 8 - remainder;

                    // Increase the filelist size with the
                    // increase byte variable from the previous step and
                    // set this as a variable
                    // Then get the amount of null bytes to pad by subtracting 
                    // the new size with the filelist size
                    var newSize = filelistDataSize + increaseByteAmount;
                    var padNulls = newSize - filelistDataSize;

                    preEncryptedfilelist.Seek((uint)preEncryptedfilelist.Length, SeekOrigin.Begin);
                    for (int pad = 0; pad < padNulls; pad++)
                    {
                        preEncryptedfilelist.WriteByte(0);
                    }

                    filelistDataSize = newSize;
                }

                // Add 8 bytes for the size and hash
                // offsets and 8 null bytes
                preEncryptedfilelist.Seek((uint)preEncryptedfilelist.Length, SeekOrigin.Begin);
                for (int ofs = 0; ofs < 16; ofs++)
                {
                    preEncryptedfilelist.WriteByte(0);
                }
            }

            using (var filelistToEncrypt = new FileStream(repackVariables.NewFilelistFile, FileMode.Open, FileAccess.Write))
            {
                using (var filelistToEncryptWriter = new BinaryWriter(filelistToEncrypt))
                {
                    filelistToEncrypt.Seek(0, SeekOrigin.Begin);

                    filelistToEncryptWriter.AdjustBytesUInt32(16, filelistDataSize, CmnEnums.Endianness.BigEndian);
                    filelistToEncryptWriter.AdjustBytesUInt32((uint)filelistToEncrypt.Length - 16, filelistDataSize, CmnEnums.Endianness.LittleEndian);
                }
            }


            // Write checksum to the filelist file
            filelistDataSize += 32;
            var asciiSize = filelistDataSize.ToString("x8");
            var cryptCheckSumCode = " write";
            var checkSumActionArg = " " + asciiSize + cryptCheckSumCode;
            FFXiiiCryptTool(" -c ", "\"" + repackVariables.NewFilelistFile + "\"", ref checkSumActionArg);
            Console.WriteLine("\nFinished writing checksum for new filelist");


            // Encrypt the filelist file
            var cryptFilelistCode = " filelist";
            FFXiiiCryptTool(" -e ", "\"" + repackVariables.NewFilelistFile + "\"", ref cryptFilelistCode);
            Console.WriteLine("\nFinished encrypting new filelist");
        }


        static void FFXiiiCryptTool(string actionSwitch, string filelistName, ref string actionType)
        {
            using (Process xiiiCrypt = new Process())
            {
                xiiiCrypt.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                xiiiCrypt.StartInfo.FileName = "ffxiiicrypt.exe";
                xiiiCrypt.StartInfo.Arguments = actionSwitch + filelistName + actionType;
                xiiiCrypt.StartInfo.UseShellExecute = true;
                xiiiCrypt.Start();
                xiiiCrypt.WaitForExit();
            }
        }
    }
}