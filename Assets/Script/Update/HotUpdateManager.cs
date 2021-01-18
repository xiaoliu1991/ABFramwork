
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public struct UpdateInfo
{
    public string name;
    public int size;
    public string crc;

    public override string ToString()
    {
        return "名称: " + name + " 大小:" + size;
    }
}


public class HotUpdateManager : UnitySingleton<HotUpdateManager>
{
    private string mServerUrl = "https://gameres-1304728229.cos.ap-chengdu.myqcloud.com/";

    private Dictionary<string,UpdateInfo> mUpdateList = new Dictionary<string, UpdateInfo>();
    private ResourceUpdateProgress mProgress = new ResourceUpdateProgress();

    private Action<UpdateInfo, float> mUpdateFinishCall;
    private Action<string> mErrorCall;
    private Action mCompleteCall;

    public void OnUpdate(Action<UpdateInfo, float> updateFinish,Action<string> error,Action complete)
    {
        this.mUpdateFinishCall = updateFinish;
        this.mErrorCall = error;
        this.mCompleteCall = complete;
#if UNITY_EDITOR
        if (Main.Inst.UseABLoad)
        {
            StartCoroutine(LoadVersion());
        }
        else
        {
            mCompleteCall();
        }
#else
        StartCoroutine(LoadVersion());
#endif
    }

    IEnumerator LoadVersion()
    {
        string netVersion = string.Empty;
        string localVersion = string.Empty;
        string path = Application.streamingAssetsPath + "/version.txt";
        WWW www = new WWW(path);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning("流目录无热更文件！");
        }
        else
        {
            localVersion = www.text;
        }

        www = new WWW(mServerUrl + "version.txt");
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning("服务器无热更文件！");
        }
        else
        {
            netVersion = www.text;
        }

        if (!string.IsNullOrEmpty(localVersion)) Main.Inst.Version = localVersion;
        if (!string.IsNullOrEmpty(netVersion))
        {
            Main.Inst.Version = netVersion;
        }
        else
        {
            if (string.IsNullOrEmpty(Main.Inst.Version))
            {
                mErrorCall("资源未配置！");
            }
            else
            {
                //无热更
                mCompleteCall();
            }
            yield break; 
        }

        path = mServerUrl + string.Format("{0}/{1}/{2}", Main.Inst.Version, Paths.PlatformName, "files.txt");
        www = new WWW(path);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            yield break;
        }
        string netFiles = www.text;
        string streamFiles = string.Empty;
        path = Paths.StreamPath + string.Format("{0}/{1}", Paths.PlatformName, "files.txt");
        if (File.Exists(path))
        {
            streamFiles = File.ReadAllText(path);
        }
        CollectDownFile(netFiles,streamFiles);
        DownLoadFiles();
    }
    private void CollectDownFile(string netFiles,string streamFiles)
    {
        mUpdateList.Clear();
        string[] perFiles = null;
        if (Directory.Exists(Paths.PersistentDataPath))
        {
            perFiles = Directory.GetFiles(Paths.PersistentDataPath, "*", SearchOption.AllDirectories);
        }
        string[] netFilesArr = netFiles.Split(new string[] { "\r\n" }, StringSplitOptions.None);

        for (int i = 0; i < netFilesArr.Length; i++)
        {
            if (string.IsNullOrEmpty(netFilesArr[i])) continue;
            string[] fileArr = netFilesArr[i].Split('|');
            UpdateInfo info = new UpdateInfo();
            string netFile = fileArr[0].Replace("\\", "/");
            string netCrc = fileArr[1];
            string netSize = fileArr[2];
            info.name = netFile;
            info.crc = netCrc;
            info.size = int.Parse(netSize);
            bool isNewFile = true;
            if (perFiles != null && perFiles.Length > 0)
            {
                for (int j = 0; j < perFiles.Length; j++)
                {
                    string fileName = perFiles[j].Replace(Paths.PersistentDataPath, "").Replace("\\", "/");
                    if (netFile.Equals(fileName))
                    {
                        isNewFile = false;
                        if (!FileToCRC32.GetFileCRC32(perFiles[j]).Equals(netCrc))
                        {
                            mUpdateList.Add(info.name, info);
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(streamFiles))
            {
                string[] streamFilesArr = streamFiles.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                for (int j = 0; j < streamFilesArr.Length; j++)
                {
                    if (string.IsNullOrEmpty(streamFilesArr[j])) continue;
                    string[] sfileArr = streamFilesArr[i].Split('|');
                    if (sfileArr[0].Replace("\\", "/").Equals(netFile))
                    {
                        isNewFile = false;
                        if (!sfileArr[1].Equals(netCrc))
                        {
                            mUpdateList.Add(info.name, info);
                        }
                    }
                }
            }

            if (isNewFile)
            {
                mUpdateList.Add(info.name, info);
            }
        }
    }

    private void DownLoadFiles()
    {
        if (mUpdateList.Count<=0)
        {
            mCompleteCall();
            return;
        }
        FileUtils.CreateDirectory(Paths.PersistentDataPath);
        string path = mServerUrl + string.Format("{0}/{1}/{2}", Main.Inst.Version, Paths.PlatformName, "files.txt");
        string savePath = Paths.PersistentDataPath + "files.txt";
        bool state = HttpDownLoad.DownLoadFile(path, savePath);
        if (!state)
        {
            mErrorCall("热更差异化文件下载失败！");
            return;
        }

        mProgress.Reset();
        mProgress.TotalNum = mUpdateList.Count;
        foreach (var dic in mUpdateList)
        {
            mProgress.TotalSize += dic.Value.size;
        }
        foreach (var dic in mUpdateList)
        {
            var info = dic.Value;
            string url = mServerUrl + string.Format("{0}/{1}/", Main.Inst.Version, Paths.PlatformName);
            savePath = Paths.PersistentDataPath + info.name.Replace("\\", "/");
            DownLoadManager.Inst.StartDownload(info.name, url, savePath, info.size,info.crc, OnProcess, OnFinish);
        }
    }

    private void OnProcess(string name, DownLoadProgress progress)
    {
        mProgress.Size = progress.Size;
        //Debug.LogFormat("OnProcess===>Success:{0}  {1}",name, progress.SizeKB);
    }


    private void OnFinish(string name, bool result)
    {
        if (result)
        {
            mProgress.SuccessNum++;
            mUpdateFinishCall(mUpdateList[name], mProgress.SuccessNum / (float)mProgress.TotalNum);
            mUpdateList.Remove(name);
        }
        else
        {
            mProgress.FailedNum++;
        }

        Debug.LogFormat("OnDownloadFinish===>Success:{0}  {1},Failed:{2}",name, mProgress.SuccessNum, mProgress.FailedNum);

        if (mProgress.IsFinish)
        {
            if (mProgress.IsSuccess)
            {
                mCompleteCall();
            }
        }
    }
}