using UnityEngine;

public class Main : MonoBehaviour
{
    public static Main Inst;
    [XTip("使用AB模式加载")]
    public bool UseABLoad;
    [XTip("使用LuaAB模式加载")]
    public bool UseLuaABLoad;
    [XTip("版本号")]
    public string Version;
	// Use this for initialization
	void Start ()
    {
        Inst = this;
        InitManager();
        BeginUpdate();
    }

    private void InitManager()
    {
        new GameObject("ResManager").AddComponent<ResManager>();
        new GameObject("HotUpdateManger").AddComponent<HotUpdateManager>();
        new GameObject("DownLoadManager").AddComponent<DownLoadManager>();
    }

    private void BeginUpdate()
    {
        Transform uiRoot = GameObject.Find("UIRoot").transform;
        GameObject res = Resources.Load<GameObject>("Update/UpdatePanel");
        Transform update = Instantiate(res).transform;
        update.SetParent(uiRoot, false);
        update.localEulerAngles = Vector3.zero;
        update.localScale = Vector3.one;
        update.localPosition = Vector3.zero;
    }
}

