using UnityEngine;
using UnityEngine.UI;

public class UpdatePanel : MonoBehaviour
{
    public Text mTips;
    public Image mProcess;
         
	// Use this for initialization
	void Start ()
    {
        mProcess.fillAmount = 0;
        HotUpdateManager.Inst.OnUpdate(OnFileUpdateFish, OnError, OnUpdateComplete);
    }


    private void OnFileUpdateFish(UpdateInfo info, float process)
    {
        mProcess.fillAmount = process;
        mTips.text = info.ToString();
    }

    private void OnError(string error)
    {
        Debug.LogError(error);
    }

    private void OnUpdateComplete()
    {
        StartGame();
    }


    private void StartGame()
    {
        new GameObject("Game").AddComponent<Game>();
    }
}
