using UnityEngine;

public class Paths
{
    public static string GameResRoot = "GameRes";
    public static string BuildABPath = Application.dataPath + "/../BuildABs/";

    public static string PlatformName
    {
        get
        {
            string name = "Android";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    name = "Android";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    name = "IOS";
                    break;
                case RuntimePlatform.WindowsPlayer:
                    name = "Windows";
                    break;
            }
            return name;
        }
    }
    /// <summary>
    /// 持久化目录
    /// </summary>
#if UNITY_EDITOR
    public static string PersistentDataPath = Application.dataPath + "/../HotUpdate/";
#elif UNITY_ANDROID
        public static string PersistentDataPath = Application.persistentDataPath + "/Android/";
#elif UNITY_IOS
        public static string PersistentDataPath = Application.persistentDataPath + "/IOS/";
#elif UNITY_STANDALONE_WIN
        public static string PersistentDataPath = Application.persistentDataPath + "/Windows/";
#else
        public static string PersistentDataPath = Application.persistentDataPath + "/Other/";
#endif


    /// <summary>
    /// 流媒体目录
    /// </summary>
#if UNITY_ANDROID
    public static string StreamPath = Application.streamingAssetsPath + "/Android/";
#elif UNITY_IOS
        public static string StreamPath = Application.streamingAssetsPath + "/IOS/";
#elif UNITY_STANDALONE_WIN
        public static string StreamPath = Application.streamingAssetsPath + "/Windows/";
#else
        public static string StreamPath = Application.streamingAssetsPath + "/Other/";
#endif
}