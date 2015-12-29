using UnityEngine;
using System.Collections;

public class PopupListChangeEventControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPopupListValueChanged()
    {
        UnitySocketClient client = GameObject.Find("SocketControl").GetComponent<SocketController>().m_UnitySocketClient;
        if (client != null && client.IsUnitySocketClientConnected())
        {
            UIPopupList poplist = UIPopupList.current;
            Debug.Log("Current Value is " + poplist.value);
            client.Send(DataHandler.PackingSetScriptInfo(poplist.value));
        }
    }
}
