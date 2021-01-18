
using System;
using System.IO;
using UnityEngine;

public class FileUtils
{
    public static void CreateDirectory(string path)
    {
        path = path.Replace("\\", "/");
        path = path.Substring(0, path.LastIndexOf("/"));
        Directory.CreateDirectory(path);
    }

    public static void DelFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void CopyDirectory(string srcPath, string destPath)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)     //判断是否文件夹
                {
                    if (!Directory.Exists(destPath + "\\" + i.Name))
                    {
                        Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                    }
                    CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
                }
                else
                {
                    File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    /// <param name="srcFilePath"></param>
    /// <param name="destFilePath"></param>
    public static void MoveFile(string srcFilePath, string destFilePath)
    {
        if (File.Exists(srcFilePath))
        {
            if (File.Exists(destFilePath))
                File.Delete(destFilePath);
            File.Move(srcFilePath, destFilePath);
        }
    }


    /// <summary>
    /// 打开文件流，如果不存在，就创建一个文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Stream ForceOpenFileStream(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Stream fileStream = null;
        try
        {
            fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        }
        catch (Exception e)
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
            Debug.LogWarning(string.Format("Failed to force open file stream: {0}, error {1}!", filePath, e.Message));
        }

        return fileStream;
    }


    public static long GetFileBytesSize(string filePath)
    {
        if (!File.Exists(filePath))
            return 0;
        long byteSize = 0;
        var stream = OpenFileStream(filePath);
        if (stream != null)
        {
            byteSize = stream.Length;
            stream.Close();
        }
        return byteSize;
    }

    /// <summary>
    /// 打开文件流
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Stream OpenFileStream(string filePath)
    {
        Stream fileStream = null;
        try
        {
            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

        return fileStream;
    }
}