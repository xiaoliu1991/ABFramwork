
using UnityEngine;

public class Game : MonoBehaviour
{
    private void Awake()
    {
        //        new GameObject("LuaManager").AddComponent<LuaManager>();
        StartGame();
    }



    private void StartGame()
    {
        Transform uiRoot = GameObject.Find("UIRoot").transform;
        Transform login = ResManager.Inst.LoadRes<GameObject>(Def.ModulesType.Basic, "LoginPanel").transform;
        login.SetParent(uiRoot, false);
        login.localEulerAngles = Vector3.zero;
        login.localScale = Vector3.one;
        login.localPosition = Vector3.zero;
    }
}