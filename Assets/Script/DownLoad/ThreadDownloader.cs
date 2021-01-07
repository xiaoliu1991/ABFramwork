using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;
public enum DownLoadType
{
    NewFile,
    CacheResumeFile,
}

public enum DownloadState
{
    None,
    Init,
    Loading,
    Finished
}

/// <summary>
/// 下载进度
/// </summary>
public class DownLoadProgress
{
    /// <summary>
    /// 已经下载的大小：Byte
    /// </summary>
    public long Size
    {
        get;
        private set;
    }
    /// <summary>
    /// 已经下载的大小：KB
    /// </summary>
    public float SizeKB
    {
        get
        {
            if (Size <= 0)
            {
                return 0;
            }
            return 1f * Size / 1024;
        }
    }

    /// <summary>
    /// 已经下载的大小：MB
    /// </summary>
    public float SizeMB
    {
        get
        {
            if (Size <= 0)
            {
                return 0;
            }
            return 1f * Size / 1024 / 1024;
        }
    }

    /// <summary>
    /// 需要下载的大小:Byte
    /// </summary>
    public long TotalSize
    {
        get;
        private set;
    }

    /// <summary>
    /// 需要下载的大小:KB
    /// </summary>
    public float TotalSizeKB
    {
        get
        {
            return 1f * TotalSize / 1024;
        }
    }


    /// <summary>
    /// 需要下载的大小:MB
    /// </summary>
    public float TotalSizeMB
    {
        get
        {
            return 1f * TotalSize / 1024 / 1024;
        }
    }

    /// <summary>
    /// 进度
    /// </summary>
    public float Progress
    {
        get
        {
            if (Size <= 0) return 0f;
            if (TotalSize == 0) return 0f;
            return Mathf.Clamp01(1f * Size / TotalSize);
        }
    }
    /// <summary>
    /// 下载速度
    /// </summary>
    public float LoadSpeed
    {
        get;
        private set;
    }

    /// <summary>
    /// 更新进度
    /// </summary>
    /// <param name="size">已经下载的大小</param>
    /// <param name="totalSize">需要下载的大小</param>
    internal void UpdateProgress(long size, long totalSize, float speed)
    {
        this.Size = size;
        this.TotalSize = totalSize;
        this.LoadSpeed = speed;
    }

    /// <summary>
    /// 重置
    /// </summary>
    internal void Reset()
    {
        this.Size = 0;
        this.TotalSize = 0;
    }
}
/// <summary>
/// 线程下载器
/// </summary>
public class ThreadDownloader
{
    /// <summary>
    /// 正则表达式
    /// </summary>
    public static Regex httpsReg = new Regex("^https.*");
    /// <summary>
    /// 默认的缓冲区大小
    /// </summary>
    const int DEFAULT_BUFFER_SIZE = 1024 * 64; // 64k 
                                               /// <summary>
                                               /// 超时时间
                                               /// </summary>
    const uint MAX_WAIT_TIME = 3 * 1000;

    /// <summary>
    /// 下载进度
    /// </summary>
    public DownLoadProgress progress
    {
        get;
        private set;
    }

    public float speed
    {
        get;
        private set;
    }

    /// <summary>
    /// 下载的URL
    /// </summary>
    public string url
    {
        get;
        private set;
    }
    /// <summary>
    /// 文件保存路径
    /// </summary>
    public string savePath
    {
        get;
        private set;
    }
    /// <summary>
    /// 本地缓存文件
    /// </summary>
    public string fileLocalCache
    {
        get;
        private set;
    }
    /// <summary>
    /// 下载文件的大小
    /// </summary>
    public long fileSize
    {
        get;
        private set;
    }
    /// <summary>
    /// 下载文件的CRC
    /// </summary>
    public string fileCRC
    {
        get;
        private set;
    }
    /// <summary>
    /// 文件名
    /// </summary>
    public string fileName
    {
        get;
        private set;
    }
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool isSuccess
    {
        get;
        private set;
    }

    /// <summary>
    /// 文件总大小
    /// </summary>
    public long totalByteSize
    {
        get
        {
            return localBytes + totalBytes;
        }
    }

    /// <summary>
    /// 本地已经下载的大小
    /// </summary>
    public long localByteSize
    {
        get { return bytesSize + localBytes; }
    }

    /// <summary>
    /// 当前下载的大小
    /// </summary>
    public long bytesSize
    {
        get;
        private set;
    }
    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadState downLoadState
    {
        get;
        private set;
    }


    /// <summary>
    /// 下载类型
    /// </summary>
    public DownLoadType downLoadType
    {
        get;
        private set;
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string errorMsg
    {
        get;
        private set;
    }
    /// <summary>
    /// readBuffer
    /// </summary>
    byte[] readBuffer;
    /// <summary>
    /// 总共需要下载的大小
    /// </summary>
    long totalBytes;
    /// <summary>
    /// 本地已经下载的大小
    /// </summary>
    long localBytes;
    /// <summary>
    /// 上下文
    /// </summary>
    byte[] content;
    /// <summary>
    /// 请求参数
    /// </summary>
    string postContent;
    /// <summary>
    /// http请求
    /// </summary>
    HttpWebRequest webRequest;
    /// <summary>
    /// http响应
    /// </summary>
    HttpWebResponse webResponse;
    /// <summary>
    /// 
    /// </summary>
    Stream responseStream;
    /// <summary>
    /// 
    /// </summary>
    Stream destStream;
    /// <summary>
    /// 完成回调
    /// </summary>
    Action<string, bool> finishAction;
    /// <summary>
    /// 进度回调
    /// </summary>
    Action<string, DownLoadProgress> progressAction;
    /// <summary>
    /// 下载线程
    /// </summary>
    Thread thread = null;

    DateTime startTime;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        if (progress == null) progress = new DownLoadProgress();
        if (readBuffer == null) readBuffer = new byte[DEFAULT_BUFFER_SIZE];
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="url">下载链接</param>
    /// <param name="savePath">文件保存路径</param>
    /// <param name="progressAction">进度回调</param>
    /// <param name="finishAction">下载完成回调</param>
    /// <param name="posContent">http参数</param>
    public void Init(string fileName, string url, string savePath, Action<string, DownLoadProgress> progressAction = null, Action<string, bool> finishAction = null, string posContent = "")
    {
        this.downLoadState = DownloadState.Init;
        this.downLoadType = DownLoadType.NewFile;
        this.fileName = fileName;
        this.url = url;
        this.savePath = savePath;
        this.progressAction = progressAction;
        this.finishAction = finishAction;
        this.postContent = posContent;
    }


    /// <summary>
    /// 初始化参数
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="url">下载链接</param>
    /// <param name="savePath">文件保存路径</param>
    /// <param name="size"></param>
    /// <param name="crc"></param>
    /// <param name="progressAction"></param>
    /// <param name="finishAction"></param>
    /// <param name="posContent"></param>
    public void Init(string fileName, string url, string savePath, long size, string crc, Action<string, DownLoadProgress> progressAction = null, Action<string, bool> finishAction = null)
    {
        this.downLoadState = DownloadState.Init;
        this.downLoadType = DownLoadType.CacheResumeFile;
        this.url = url;
        this.fileName = fileName;
        this.savePath = savePath;
        this.fileSize = size;
        this.fileCRC = crc;
        this.progressAction = progressAction;
        this.finishAction = finishAction;
        this.postContent = "";
    }

    /// <summary>
    /// 开始多线程下载
    /// </summary>
    public void Start()
    {
        thread = new Thread(Download);
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    void Download()
    {
        isSuccess = false;
        downLoadState = DownloadState.Loading;
        startTime = DateTime.Now;
        try
        {
            if (!string.IsNullOrEmpty(httpsReg.Match(url).ToString()))
            {
//                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ResourceDownloadManager.CheckValidationResult);
            }

            var stringBuilder = new StringBuilder();
            var fullUrl = stringBuilder.Append(url).Append(fileName).ToString();
            webRequest = (HttpWebRequest)WebRequest.Create(fullUrl);
            if (downLoadType == DownLoadType.CacheResumeFile)
            {
                stringBuilder.Length = 0;
                fileLocalCache = stringBuilder.Append(savePath).Append(Def.Down_Tmp_SUFFIX).ToString();
                localBytes = FileUtils.GetFileBytesSize(fileLocalCache);
                webRequest.AddRange((int)localBytes);
            }
            else
            {
                FileUtils.DelFile(savePath);
            }

            webRequest.ProtocolVersion = HttpVersion.Version11;
            if (!string.IsNullOrEmpty(postContent))
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                content = Encoding.UTF8.GetBytes(postContent);
                webRequest.ContentLength = content.Length;
            }
            else
            {
                var ar = webRequest.BeginGetResponse(new AsyncCallback(BeginGetResponseCallback), null);
                ThreadPool.RegisterWaitForSingleObject(ar.AsyncWaitHandle, new WaitOrTimerCallback(BeginGetResponseTimeout), webRequest, MAX_WAIT_TIME, true);
            }
        }
        catch (WebException e)
        {
            if (webRequest != null) webRequest.Abort();
            errorMsg = "web exception";
            FinishDownload(false);
            Debug.LogWarning(string.Format("BeginGetResponse web exception. message: {0}, status: {1}.StackTrace:{2}", e.Message, e.Status, e.StackTrace));
        }
        catch (Exception e)
        {
            if (webRequest != null) webRequest.Abort();
            errorMsg = "download exception";
            FinishDownload(false);
            Debug.LogWarning(string.Format("BeginGetResponse exception. source: {0}, message: {1}.StackTrace:{2}", e.Source, e.Message, e.StackTrace));
        }
    }


    /// <summary>
    /// 获取响应超时
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isTimedOut"></param>
    void BeginGetResponseTimeout(object state, bool isTimedOut)
    {
        if (!isTimedOut)
        {
            return;
        }
        errorMsg = "response timeout";
        if (webRequest != null) webRequest.Abort();
        FinishDownload(false);
    }


    /// <summary>
    /// 获取响应异步回调
    /// </summary>
    /// <param name="asynchronousResult"></param>
    void BeginGetResponseCallback(IAsyncResult asynchronousResult)
    {
        try
        {
            speed = 0;
            webResponse = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
            totalBytes = webResponse.ContentLength;
            if (fileSize != 0 && fileSize != totalBytes + localBytes) // TODO: 校验文件大小!
            {
                Debug.LogWarning(string.Format("Http file invalid File {0} size {1}, expected {2}!", fileName, totalBytes, fileSize));
                errorMsg = "size verify failed";
                FinishDownload(false);
                return;
            }

            destStream = FileUtils.ForceOpenFileStream(downLoadType == DownLoadType.CacheResumeFile ? fileLocalCache : savePath);

            if (destStream == null)
            {
                errorMsg = "open file error";
                FinishDownload(false);
                return;
            }

            if (downLoadType == DownLoadType.CacheResumeFile)
                destStream.Seek(destStream.Length, SeekOrigin.Current);

            responseStream = webResponse.GetResponseStream();
            if (responseStream != null)
            {
                var ar = responseStream.BeginRead(readBuffer, 0, readBuffer.Length, ReadCallBack, null);
                var signalled = ar.AsyncWaitHandle.WaitOne((int)MAX_WAIT_TIME);
                BeginReadTimeOut(null, !signalled);
            }
        }
        catch (Exception e)
        {
            errorMsg = "response callback exception";
            FinishDownload(false);
            Debug.LogWarning(string.Format("ResCallback exception. message: {0}.", e.Message));
        }
    }

    /// <summary>
    /// 读取数据回调
    /// </summary>
    /// <param name="asyncResult"></param>
    void ReadCallBack(IAsyncResult asyncResult)
    {
        try
        {
            var bytesRead = responseStream.EndRead(asyncResult);

            destStream.Write(readBuffer, 0, bytesRead);

            if (bytesRead > 0)
            {
                bytesSize += bytesRead;
                TimeSpan span = DateTime.Now - startTime;
                float second = (float)span.TotalSeconds;
                if (second > 0.0001)
                {
                    speed = bytesSize / 1024 / second;
                }

                var ar = responseStream.BeginRead(readBuffer, 0, readBuffer.Length, ReadCallBack, null);
                var signalled = ar.AsyncWaitHandle.WaitOne((int)MAX_WAIT_TIME);
                BeginReadTimeOut(null, !signalled);
                return;
            }
            else
            {
                destStream.Flush();
                FinishDownload(bytesSize == totalBytes);
            }
        }
        catch (Exception ex)
        {
             Debug.LogWarning(string.Format("ReadCallBack EndRead exception. message: {0}. StackTrace:{1}", ex.Message, ex.StackTrace));
            errorMsg = "read bytes exception";
            FinishDownload(false);
        }
    }

    void BeginReadTimeOut(object state, bool isTimedOut)
    {
        if (!isTimedOut)
        {
            return;
        }
        errorMsg = "read timeout";
        Debug.LogWarning(string.Format("Failed to download file {0}, BeginRead timeout!", fileName));
        FinishDownload(false);
    }

    /// <summary>
    /// 下载结束
    /// </summary>
    /// <param name="tmpSuccess">是否下载成功</param>
    void FinishDownload(bool tmpSuccess)
    {

        lock (this)
        {
            if (downLoadState != DownloadState.Loading)
                return;

            if (webResponse != null) webResponse.Close();
            if (responseStream != null) responseStream.Close();
            if (destStream != null) destStream.Close();

            var isVerifySuccess = true;
            if (tmpSuccess)
            {
                if (downLoadType == DownLoadType.CacheResumeFile)
                {
                    if (string.Empty != fileCRC)
                    {
                        var localCrc = FileToCRC32.GetFileCRC32(fileLocalCache);
                        if (localCrc == fileCRC)
                        {
                            FileUtils.MoveFile(fileLocalCache, savePath);
                            Debug.Log(string.Format("Finish download success file {0}", fileName));
                        }
                        else
                        {
                            isVerifySuccess = false;
                            FileUtils.DelFile(fileLocalCache);
                            errorMsg = "crc verify failed";
                            Debug.LogWarning(string.Format("Finish download crc check failed {0}", fileName));
                        }
                    }
                    else
                    {
                        FileUtils.MoveFile(fileLocalCache, savePath);
                        Debug.Log(string.Format("Finish download success file {0}", fileName));
                    }
                }
            }
            else
            {
                
                Debug.LogError(string.Format("Finish download failed to download file:: {0},errorReason::{1}", fileName, errorMsg));
                Debug.LogError("=====load fail=======" + (url + fileName));
            }

            this.isSuccess = tmpSuccess && isVerifySuccess;
            downLoadState = DownloadState.Finished;
        }
    }


    /// <summary>
    /// 进度更新回调
    /// </summary>
    public void UpdateCallBack()
    {
        progress.UpdateProgress(localByteSize, fileSize, speed);
        if (progressAction != null)
        {
            progressAction(fileName, progress);
        }
    }

    /// <summary>
    /// 下载完成回调
    /// </summary>
    public void FinishCallback()
    {
        if (finishAction != null)
            finishAction(fileName, isSuccess);
    }


    /// <summary>
    /// 清理数据
    /// </summary>
    public void Reset()
    {
        url = string.Empty;
        fileName = string.Empty;
        savePath = string.Empty;
        fileLocalCache = string.Empty;
        fileSize = 0;
        fileCRC = string.Empty;
        isSuccess = false;
        readBuffer = null;
        totalBytes = 0;
        bytesSize = 0;
        localBytes = 0;
        content = null;
        postContent = string.Empty;
        webRequest = null;
        webResponse = null;
        responseStream = null;
        destStream = null;
        finishAction = null;
        progressAction = null;
        downLoadState = DownloadState.None;
        errorMsg = string.Empty;
        if (thread != null) thread.Abort();
        thread = null;
    }
}