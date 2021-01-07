using UnityEngine;

public class Main : MonoBehaviour
{
    public static Main Inst;
    public bool IsABLoad;

    public string Version;
    public bool LoadLocalAsset;
	// Use this for initialization
	void Start ()
    {
        Inst = this;
        new GameObject("ResManager").AddComponent<ResManager>();
        new GameObject("HotUpdateManger").AddComponent<HotUpdateManager>();
        new GameObject("DownLoadManager").AddComponent<DownLoadManager>();
        BeginUpdate();
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

