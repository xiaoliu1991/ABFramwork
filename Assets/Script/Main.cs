using System.Collections;
using UnityEngine;

public class Main : MonoBehaviour
{
    public static Main Inst;
    public bool IsABLoad;

    public static ResManager ResMgr;
    public string Version;
    public bool LoadLocalAsset;
	// Use this for initialization
	void Start ()
    {
        Inst = this;
        ResMgr = ResManager.Inst;
        new GameObject("HotUpdateManger").AddComponent<HotUpdateManager>();
        HotUpdateManager.Inst.OnUpdate(OnUpdate, OnError, OnUpdateComplete);
    }

    private void OnUpdate(UpdateInfo info,bool state)
    {
        if (!state)
        {
            Debug.LogError(info);
        }
        else
        {
            Debug.Log("[热更新] " + info);
        }
    }

    private void OnError()
    {
        Debug.LogError("热更错误！");
    }

    private void OnUpdateComplete(bool state)
    {
        LoadLocalAsset = state;
        StartGame();
    }

    private void StartGame()
    {
        Transform uiRoot = GameObject.Find("UIRoot").transform;
        Transform login = ResMgr.LoadRes<GameObject>(Def.ModulesType.Basic,"LoginPanel").transform;
        login.SetParent(uiRoot, false);
        login.localEulerAngles = Vector3.zero;
        login.localScale = Vector3.one;
        login.localPosition = Vector3.zero;
    }
}

