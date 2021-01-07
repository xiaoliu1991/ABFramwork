using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    public Button login;
	// Use this for initialization
	void Start ()
    {
        login.onClick.AddListener(OnLogin);
    }

    private void OnLogin()
    {
        Transform uiRoot = GameObject.Find("UIRoot").transform;
        Transform tips = ResManager.Inst.LoadRes<GameObject>(Def.ModulesType.Basic, "TipsPanel").transform;
        tips.SetParent(uiRoot, false);
        tips.localEulerAngles = Vector3.zero;
        tips.localScale = Vector3.one;
        tips.localPosition = Vector3.zero;
    }
}   
