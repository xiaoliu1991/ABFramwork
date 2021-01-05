
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public struct UpdateInfo
{
    public string name;
    public int size;

    public override string ToString()
    {
        return "名称: " + name + " 大小:" + size;
    }
}

public class HotUpdateManager : UnitySingleton<HotUpdateManager>
{
    private string mServerUrl = "http://192.168.0.75:8080/Resource/";
    private string mVersionUrl = "http://192.168.0.75:8080/Resource/version.txt";

    private Action<UpdateInfo,bool> mUpdateCall;
    private Action mErrorCall;
    private Action<bool> mCompleteCall;
    private HttpDownLoad mDownLoad;

    public void OnUpdate(Action<UpdateInfo, bool> update,Action error,Action<bool> complete)
    {
        mDownLoad = new HttpDownLoad();
        this.mUpdateCall = update;
        this.mErrorCall = error;
        this.mCompleteCall = complete;
#if UNITY_EDITOR
        if (Main.Inst.IsABLoad)
        {
            StartCoroutine(LoadVersion());
        }
        else
        {
            mCompleteCall(false);
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
                mErrorCall();
            }
            else
            {
                //无热更
                mCompleteCall(false);
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
    }

    private void CollectDownFile(string netFiles,string perFiles,string streamFiles)
    {
        bool isLoadLoalAsset = string.IsNullOrEmpty(netFiles) && string.IsNullOrEmpty(perFiles) && !string.IsNullOrEmpty(streamFiles);
        string[] netFilesArr = netFiles.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        List<UpdateInfo> list = new List<UpdateInfo>();
        if (string.IsNullOrEmpty(streamFiles) && string.IsNullOrEmpty(perFiles))
        {
            for (int i = 0; i < netFilesArr.Length; i++)
            {
                if (string.IsNullOrEmpty(netFilesArr[i])) continue;
                string[] fileArr = netFilesArr[i].Split('|');
                UpdateInfo info = new UpdateInfo();
                info.name = fileArr[0];
                info.size = int.Parse(fileArr[2]);
                list.Add(info);
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
                                info.name = netFileArr[0];
                                info.size = int.Parse(netFileArr[2]);
                                list.Add(info);
                            }
                        }
                    }

                    if (!isFind)
                    {
                        UpdateInfo info = new UpdateInfo();
                        info.name = netFileArr[0];
                        info.size = int.Parse(netFileArr[2]);
                        list.Add(info);
                    }
                }
            }
        }


        string path = mServerUrl + string.Format("{0}/{1}/{2}", Main.Inst.Version, Paths.PlatformName, "files.txt");
        string savePath = "";
#if UNITY_EDITOR
        savePath = Paths.PersistentDataPath + string.Format("{0}/{1}", Paths.PlatformName, "files.txt");
#else
        savePath = Paths.PersistentDataPath + "files.txt";
#endif
        FileUtils.DelFile(savePath);
        FileUtils.CreateDirectory(savePath);
        bool state = mDownLoad.DownLoadFile(path, savePath);
        foreach (var info in list)
        {
            path = mServerUrl + string.Format("{0}/{1}/{2}", Main.Inst.Version, Paths.PlatformName, info.name);
#if UNITY_EDITOR
            savePath = Paths.PersistentDataPath + string.Format("{0}/{1}", Paths.PlatformName, info.name.Replace("\\","/"));
#else
            savePath = Paths.PersistentDataPath + info.name.Replace("\\","/");
#endif
            FileUtils.DelFile(savePath);
            FileUtils.CreateDirectory(savePath);
            state = mDownLoad.DownLoadFile(path, savePath);
            Debug.Log("下载文件：" + savePath);
            mUpdateCall(info, state);
        }

        mCompleteCall(isLoadLoalAsset);
    }
}