

using System;
using System.Collections.Generic;
using UnityEngine;

public class DownLoadManager : UnitySingleton<DownLoadManager>
{
    /// <summary>
    /// 线程下载器
    /// </summary>
    private List<ThreadDownloader> mDownloadList = new List<ThreadDownloader>();
    /// <summary>
    /// 下载完成的数据
    /// </summary>
    private Dictionary<string, bool> mFinishedRequest = new Dictionary<string, bool>();
    /// <summary>
    /// 线程下载器池
    /// </summary>
    private ObjectPool<ThreadDownloader> mDownloadPool = new ObjectPool<ThreadDownloader>(l => l.Init(), l => l.Reset());
    /// <summary>
    /// 下载线程数量
    /// </summary>
    private const int MAX_DOWNLOAD_THREADS_COUNT = 3;

    /// <summary>
    /// 当前正在下载的数量
    /// </summary>
    private int mCurDownloadingCount;

    private void Update()
    {
        if (mDownloadList.Count == 0) return;
        HandleDownloadList();
    }

    /// <summary>
    /// 处理下载列表
    /// </summary>
    private void HandleDownloadList()
    {
        var count = Mathf.Min(MAX_DOWNLOAD_THREADS_COUNT, mDownloadList.Count);
        var i = mCurDownloadingCount;
        for (; i < count; ++i)
        {
            mDownloadList[i].Start();
            mCurDownloadingCount++;
        }
        for (i = 0; i < mCurDownloadingCount;)
        {
            var downloadRequest = mDownloadList[i];
            if (downloadRequest.downLoadState == DownloadState.Finished)
            {
                mFinishedRequest.Add(downloadRequest.fileName, downloadRequest.isSuccess);
                mDownloadList.RemoveAt(i);
                mCurDownloadingCount--;
                downloadRequest.UpdateCallBack();
                downloadRequest.FinishCallback();
                mDownloadPool.Release(downloadRequest);
            }
            else
            {
                downloadRequest.UpdateCallBack();
                ++i;
            }
        }
    }


    /// <summary>
    /// 开始下载文件
    /// </summary>
    /// <param name="file">文件名</param>
    /// <param name="url">下载地址</param>
    /// <param name="saveDirectory">保存的文件夹</param>
    /// <param name="size">文件大小</param>
    /// <param name="crc">文件CRC</param>
    /// <param name="finishAction">完成回调</param>
    /// <param name="postContent">请求参数</param>
    public void StartDownload(string file, string url, string savePath, long size, string crc, Action<string, DownLoadProgress> progressAction = null, Action<string, bool> finishAction = null)
    {
        if (FindInDownLoadList(file) != -1) return;

        if (mFinishedRequest.ContainsKey(file))
        {
            Debug.Log(string.Format("Re download file: {0}.", file));
            mFinishedRequest.Remove(file);
        }

        var downloadRequest = mDownloadPool.Get();
        downloadRequest.Init(file, url, savePath, size, crc, progressAction, finishAction);
        mDownloadList.Add(downloadRequest);
    }

    /// <summary>
    /// 是否在下载中
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    private int FindInDownLoadList(string file)
    {
        var count = mDownloadList.Count;
        for (var i = 0; i < count; ++i)
        {
            if (mDownloadList[i].fileName == file)
            {
                return i;
            }
        }

        return -1;
    }
}