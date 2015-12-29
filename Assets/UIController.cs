using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        EventDispatcher.Instance().RegistEventListener("LOGIN_RESPOND", LodingEventCallBack);
        EventDispatcher.Instance().RegistEventListener("PLAY_FRAME_END_RESPOND", PlayFrameEndRespondEventCallBack);
        EventDispatcher.Instance().RegistEventListener("PLAYSCRIPT_END", PlayScriptEndRespondEventCallBack);
	}

    public void PlayScriptEndRespondEventCallBack(EventBase eb)
    {
        Debug.Log(">>>>PlayScriptEndRespondEventCallBack<<<<");
        GameObject.Find("StartPlayBtn").GetComponent<UIButton>().isEnabled = true;
        GameObject.Find("CurrentFrameNumLabel").GetComponent<UILabel>().text = "未播放";
    }

    public void PlayFrameEndRespondEventCallBack(EventBase eb)
    {
        Debug.Log(">>>>PlayFrameEndRespondEventCallBack<<<<");
        int[] data_arr = eb.eventValue as int[];
        int frame_num = data_arr[0];
        int is_auto_play = data_arr[1];

        GameObject.Find("CurrentFrameNumLabel").GetComponent<UILabel>().text = frame_num + "";
        if (is_auto_play != 1)
        {
            GameObject.Find("StartPlayBtn").GetComponent<UIButton>().isEnabled = true;
        }

    }

    public void LodingEventCallBack(EventBase eb)
    {
        Debug.Log("LodingEventCallBack");
        List<string> result = eb.eventValue as List<string>;

        GameObject.Find("ScriptNamePopList").GetComponent<UIPopupList>().items = result;
        GameObject.Find("ConnectSettingPanel").GetComponent<UIPanel>().alpha = 0;
    }
	
	// Update is called once per frame
	void Update () {
	}
}
