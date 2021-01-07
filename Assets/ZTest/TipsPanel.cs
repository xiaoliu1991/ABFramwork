﻿using UnityEngine;
using UnityEngine.UI;

public class TipsPanel : MonoBehaviour
{

    public Button close;
	// Use this for initialization
	void Start () {
        close.onClick.AddListener(onClose);
    }

    private void onClose()
    {
        Destroy(gameObject);
        ResManager.Inst.Remove(Def.ModulesType.Basic,"TipsPanel");
    }
}
