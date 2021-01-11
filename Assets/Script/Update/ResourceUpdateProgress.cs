/// <summary>
/// 资源更新进度
/// </summary>
public class ResourceUpdateProgress
{
    /// <summary>
    /// 下载成功数量
    /// </summary>
    public int SuccessNum { get; set; }
    /// <summary>
    /// 下载失败数量 
    /// </summary>
    public int FailedNum { get; set; }
    /// <summary>
    /// 所有需要下载的数量
    /// </summary>
    public int TotalNum { get; set; }
    /// <summary>
    /// 已经下载完成的Size:Byte
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 已经下载完成的Size:KB
    /// </summary>
    public float SizeKB
    {
        get
        {
            if (Size <= 0) return 0;
            return 1f * Size / 1024;
        }
    }
    /// <summary>
    /// 已经下载完成的Size:MB
    /// </summary>
    public float SizeMB
    {
        get
        {
            if (Size <= 0) return 0;
            return 1f * Size / 1024 / 1024;
        }
    }
    /// <summary>
    /// 所有需要下载的Size:Byte
    /// </summary>
    public long TotalSize { get; set; }
    /// <summary>
    /// 所有需要下载的Size:KB
    /// </summary>
    public float totalSizeKB
    {
        get { return 1f * TotalSize / 1024; }
    }

    /// <summary>
    /// 所有需要下载的Size:MB
    /// </summary>
    public float TotalSizeMB
    {
        get { return 1f * TotalSize / 1024 / 1024; }
    }

    public float Progress
    {
        get
        {
            if (TotalSize == 0)
                return 0;
            return 1f * Size / TotalSize;
        }
    }

    /// <summary>
    /// 是否完成
    /// </summary>
    public bool IsFinish
    {
        get
        {
            return TotalNum == (FailedNum + SuccessNum);
        }
    }

    /// <summary>
    /// 下载速度
    /// </summary>
    public float LoadSpeed
    {
        get;
        set;
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess
    {
        get
        {
            return FailedNum == 0;
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        SuccessNum = 0;
        FailedNum = 0;
        TotalNum = 0;
        Size = 0;
        TotalSize = 0;
    }
}