using UnityEngine;
using System.Collections;
using System.Text;
using KBEngine;
using System.Collections.Generic;
using System.Threading;

public class ButtonEventControl : MonoBehaviour {
    public SocketController m_SocketController;

    void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            Screen.SetResolution(640, 960, false);
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnConnectRobotBtnClick()
    {
        string ip = GameObject.Find("IPInput").GetComponent<UIInput>().value;
        Debug.Log("IP is " + ip);
        if (ip != "")
        {
            m_SocketController.CreateSocketClientConnecttion(ip, 2810);
            m_SocketController.SendDataToServer(DataHandler.PackingLoginInfo("cao"));
            GameObject.Find("ConnectSettingPanel").GetComponent<UIPanel>().alpha = 1;

            m_SocketController.m_UnitySocketClient.CreateReceiveThread();
        }
    }

    public void OnDisConnectRobotBtnClick()
    {
        m_SocketController.DisConnecttion();

    }

    public void OnQuitAppBtnClick()
    {
        Application.Quit();
    }

    public void OnDancingKeyClick()
    {
        UIButton btn = UIButton.current;
        string currentText = btn.GetComponentInChildren<UILabel>().text;
        Debug.Log("Current Button Text is " + currentText);
        
        if (m_SocketController.m_UnitySocketClient != null)
        {
            byte[] data = DataHandler.PackingKeyControlInfo((byte)(int.Parse(currentText) + 48));
            m_SocketController.SendDataToServer(data);
        }
    }

    public void OnMovingControlBtnClick()
    {
        UIButton btn = UIButton.current;
        string currentText = btn.GetComponentInChildren<UILabel>().text;
        Debug.Log("Current Button Text is " + currentText);
        if (m_SocketController.m_UnitySocketClient != null)
        {
            m_SocketController.SendDataToServer(DataHandler.PackingMovingControlInfo(currentText));
        }
    }

    public void OnFunctionControlBtnClick()
    {
        UIButton btn = UIButton.current;
        string currentText = btn.GetComponentInChildren<UILabel>().text;
        Debug.Log("Current Button Text is " + currentText);
        if (m_SocketController.m_UnitySocketClient != null)
        {
            m_SocketController.SendDataToServer(Encoding.UTF8.GetBytes(currentText));
        }
    }

    public void OnStartPlayScriptBtnClick()
    {
        UIButton btn = UIButton.current;
        GameObject.Find("SocketControl").GetComponent<SocketController>().SendDataToServer(DataHandler.PakingPlayScriptInfo());
        Debug.Log("Play Script");
        btn.isEnabled = false;
    }
}
