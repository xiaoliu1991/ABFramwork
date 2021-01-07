
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
    private string mServerUrl = "http://192.168.0.75:8080/Resource/";
    private string mVersionUrl = "http://192.168.0.75:8080/Resource/version.txt";

    private Dictionary<string,UpdateInfo> mUpdateList = new Dictionary<string, UpdateInfo>();
    private ResUpdateProgress mProgress = new ResUpdateProgress();

    private Action<UpdateInfo, float> mUpdateFinishCall;
    private Action<string> mErrorCall;
    private Action mCompleteCall;

    public void OnUpdate(Action<UpdateInfo, float> updateFinish,Action<string> error,Action complete)
    {
        this.mUpdateFinishCall = updateFinish;
        this.mErrorCall = error;
        this.mCompleteCall = complete;
#if UNITY_EDITOR
        if (Main.Inst.IsABLoad)
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
        string localUpdateVersion = string.Empty;
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

        path = Paths.PersistentDataPath + "version.txt";
        www = new WWW(path);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning("持久化目录无热更文件！");
        }
        else
        {
            localUpdateVersion = www.text;
        }

        www = new WWW(mVersionUrl);
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
        if (!string.IsNullOrEmpty(localUpdateVersion)) Main.Inst.Version = localUpdateVersion;
        if (!string.IsNullOrEmpty(netVersion))
        {
            Main.Inst.Version = netVersion;
            string p = Paths.PersistentDataPath + "version.txt";
            FileUtils.CreateDirectory(p);
            File.WriteAllText(p, netVersion);
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
        string perFiles = string.Empty;
        string streamFiles = string.Empty;
        path = Paths.PersistentDataPath + string.Format("{0}/{1}", Paths.PlatformName, "files.txt");
        if (File.Exists(path))
        {
            perFiles = File.ReadAllText(path);
        }
        path = Paths.StreamPath + string.Format("{0}/{1}", Paths.PlatformName, "files.txt");
        if (File.Exists(path))
        {
            streamFiles = File.ReadAllText(path);
        }
        CollectDownFile(netFiles, perFiles, streamFiles);
        DownLoadFiles();
    }
    private void CollectDownFile(string netFiles, string perFiles, string streamFiles)
    {
        mUpdateList.Clear();
        Main.Inst.LoadLocalAsset = string.IsNullOrEmpty(netFiles) && string.IsNullOrEmpty(perFiles) && !string.IsNullOrEmpty(streamFiles);
        string[] netFilesArr = netFiles.Split(new string[] { "\r\n" }, StringSplitOptions.None);

        if (string.IsNullOrEmpty(streamFiles) && string.IsNullOrEmpty(perFiles))
        {
            for (int i = 0; i < netFilesArr.Length; i++)
            {
                if (string.IsNullOrEmpty(netFilesArr[i])) continue;
                string[] fileArr = netFilesArr[i].Split('|');
                UpdateInfo info = new UpdateInfo();
                info.name = fileArr[0].Replace("\\", "/");
                info.crc = fileArr[1];
                info.size = int.Parse(fileArr[2]);
                mUpdateList.Add(info.name,info);
            }
        }
        else
        {
            string[] targetFilesArr = null;
            if (!string.IsNullOrEmpty(perFiles) && string.IsNullOrEmpty(streamFiles))
            {
                targetFilesArr = perFiles.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
            else if (string.IsNullOrEmpty(perFiles) && !string.IsNullOrEmpty(streamFiles))
            {
                targetFilesArr = streamFiles.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }

            if (null != targetFilesArr)
            {
                for (int i = 0; i < netFilesArr.Length; i++)
                {
                    if (string.IsNullOrEmpty(netFilesArr[i])) continue;
                    string[] netFileArr = netFilesArr[i].Split('|');
                    bool isFind = false;
                    for (int j = 0; j < targetFilesArr.Length; j++)
                    {
                        if (string.IsNullOrEmpty(targetFilesArr[j])) continue;
                        string[] targetFileArr = targetFilesArr[j].Split('|');
                        if (netFileArr[0].Equals(targetFileArr[0]))
                        {
                            isFind = true;
                            if (!netFileArr[1].Equals(targetFileArr[1]))
                            {
                                UpdateInfo info = new UpdateInfo();
                                info.name = netFileArr[0].Replace("\\", "/");
                                info.crc = netFileArr[1];
                                info.size = int.Parse(netFileArr[2]);
                                mUpdateList.Add(info.name, info);
                            }
                        }
                    }

                    if (!isFind)
                    {
                        UpdateInfo info = new UpdateInfo();
                        info.name = netFileArr[0].Replace("\\", "/");
                        info.crc = netFileArr[1];
                        info.size = int.Parse(netFileArr[2]);
                        mUpdateList.Add(info.name, info);
                    }
                }
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

        string path = mServerUrl + string.Format("{0}/{1}/{2}", Main.Inst.Version, Paths.PlatformName, "files.txt");
        string savePath = "";
#if UNITY_EDITOR
        savePath = Paths.PersistentDataPath + string.Format("{0}/{1}", Paths.PlatformName, "files.txt");
#else
        savePath = Paths.PersistentDataPath + "files.txt";
#endif
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
#if UNITY_EDITOR
            savePath = Paths.PersistentDataPath + string.Format("{0}/{1}", Paths.PlatformName, info.name);
#else
            savePath = Paths.PersistentDataPath + info.name.Replace("\\","/");
#endif
            DownLoadManager.Inst.StartDownload(info.name, url, savePath, info.size,info.crc, OnProcess, OnFinish);
        }
    }

    private void OnProcess(string name, DownLoadProgress progress)
    {
        mProgress.Size = progress.Size;
        Debug.LogFormat("OnProcess===>Success:{0}  {1}",name, progress.SizeKB);
    }


    private void OnFinish(string name, bool result)
    {
        if (result)
        {
            mUpdateFinishCall(mUpdateList[name],mProgress.Progress);
            mUpdateList.Remove(name);
            mProgress.SuccessNum++;
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