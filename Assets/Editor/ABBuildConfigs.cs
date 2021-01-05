using UnityEditor;
using UnityEngine;

public enum CompressType
{
    //LZ4
    LZ4,
    //LZMA
    LZMA
}

[CreateAssetMenu(menuName = "创建打包配置文件")]
public class ABBuildConfigs: ScriptableObject
{
    /// <summary>
    /// 一个文件夹一个AB包
    /// </summary>
    public ABMarkConfig[] OneFloderOneBundle;
    /// <summary>
    /// 一个子文件夹一个AB包
    /// </summary>
    public ABMarkConfig[] SubFloderOneBundle;
    /// <summary>
    /// 一个资源一个AB包
    /// </summary>
    public ABMarkConfig[] OneAssetOneBundle;
}


[System.Serializable]
public class ABMarkConfig
{
    /// <summary>
    /// 资源引用
    /// </summary>
    [SerializeField]
    Object asset;
//    [SerializeField]
//    /// <summary>
//    /// 压缩类型
//    /// </summary>
//    CompressType compressType = CompressType.LZMA;
//
//    public CompressType CompressType
//    {
//        get
//        {
//            return compressType;
//        }
//    }
    public string AssetPath
    {
        get
        {
            if (asset == null)
            {
                return null;
            }
            return AssetDatabase.GetAssetPath(asset);
        }
    }

    public string GamePath
    {
        get
        {
            if (asset == null)
            {
                return null;
            }
            return AssetPath.Replace("Assets/GameRes/", "");
        }
    }
}