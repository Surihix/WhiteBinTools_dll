# Documentation

Supported GameCodes:
- dirge
- ff131
- ff132

Do note that any errors or failures that occur inside these functions, will throw exception. so make sure to handle this properly in your codebase.

## Unpacking
These set of functions allows you to unpack a valid white bin archive file using its paired filelist as well as allows you to unpack the paired filelist file.

### Unpack Type A
This function will unpack all the files from the white bin archive, into a `_whiteBinFile` folder.
```c#
public static void UnpackFull(GameCode gameCode, string filelistFile, string whiteBinFile)
{

}
```

### Unpack Type B
This function will unpack a single file or duplicates of the file from the white bin archive, into a `_whiteBinFile` folder.
```c#
public static void UnpackSingle(GameCode gameCode, string filelistFile, string whiteBinFile, string whiteFilePath)
{

}
```

### Unpack Type C
This function will unpack a directory containing files or sub directories from the white bin archive, into a `_whiteBinFile` folder. provide the virtual directory path with `*` character at the end of the path.
```c#
public static void UnpackMultiple(GameCode gameCode, string filelistFile, string whiteBinFile, string whiteVirtualDirPath)
{

}
```

### Unpack Type D
This function will unpack the filelist file data as text files, into a `_filelistFile` folder.
```c#
public static void UnpackFilelist(GameCode gameCode, string filelistFile)
{

}
```

### Unpack Type E
This function will unpack the filelist file data, into a Json file.
```c#
public static void UnpackFilelistJson(GameCode gameCode, string filelistFile)
{

}
```

### Unpack Paths
This function will unpack the filepaths from the filelist file, into a text file.
```c#
public static void UnpackFilelistPaths(GameCode gameCode, string filelistFile)
{

}
```

## Repacking
These set of functions allows you to repack unpacked files present in a folder, into a white bin archive file using its paired filelist file. there are also functions that lets you to repack the paired filelist file.
do note that the filelist file should contain the filepaths of the files that you are trying to repack into the white bin archive.

### Repack Type A
This function will repack all the files from a folder, to a white bin archive.
```c# 
public static void RepackAll(GameCode gameCode, string filelistFile, string unpackedDir, bool bckup)
{

}
```

### Repack Type B
This function will repack a single file or duplicates of the file, into a white bin archive file.
```c#
public static void RepackSingle(GameCode gameCode, string filelistFile, string whiteBinFile, string whiteFilePath, bool bckup)
{

}
```

### Repack Type C
This function will repack a folder, containing the specified virtual directory, into a white bin archive file. provide the virtual directory path with `*` character at the end of the path.
```c#
public static void RepackMultiple(GameCode gameCode, string filelistFile, string whiteBinFile, string whiteExtractedDir, bool bckup)
{

}
```

### Repack Type D
This function will repack an unpacked filelist folder, that is unpacked by the `Unpack Type D` function, into a filelist file.

```c#
public static void RepackFilelist(GameCode gameCode, string extractedFilelistDir, bool bckup)
{

}
```

### Repack Type E
This function will repack a filelist json file, that is unpacked by the `Unpack Type E` function, into a filelist file.
```c#
public static void RepackJsonFilelist(GameCode gameCode, string jsonFile, bool bckup)
{

}
```