using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KBEngine;
using System.Text;

public class SocketController : MonoBehaviour {
    public UnitySocketClient m_UnitySocketClient = null;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if (m_UnitySocketClient != null)
        {
            if (m_UnitySocketClient.IsUnitySocketClientConnected())
            {
                GameObject.Find("ConnectionStateLabel").GetComponent<UILabel>().text = "机器人已连接";
            }
            else
            {
                GameObject.Find("ConnectionStateLabel").GetComponent<UILabel>().text = "机器人未连接";
            }
        }
	
	}

    public void CreateSocketClientConnecttion(string ip, int port)
    {
        m_UnitySocketClient = new UnitySocketClient(ip, port);
        m_UnitySocketClient.SocketConnection();

        UnitySocketClient.m_SocketRecvDataHandler += UnitySocketClient_m_SocketRecvDataHandler;
    }

    void UnitySocketClient_m_SocketRecvDataHandler(byte[] data)
    {
        //TODO 解析数据
        string result = "";
        for (int i = 0; i < data.Length; i++)
        {
            result += data[i].ToString("X2") + " ";
        }
        Debug.Log("this is UnitySocketClient_m_SocketRecvDataHandler data is <" + result + ">");

        short dealnum = DataHandler.UnPackDataType(data);

        if (dealnum == 0xFF)
        {
            return;
        }

        int ret_code = DataHandler.GetRetCode(data);

        Debug.Log("DealNum is <" + dealnum + "> Ret_Code is <" + ret_code + ">");

        switch (dealnum)
        {
            case DataHandler.DEALNUM_LOGIN_RESPOND:
                List<string> script_name_list = DataHandler.GetScriptNameList(data);
                if (ret_code == (int)DataHandler.LoginRetCode.LOGIN_SUCCESS)
                {
                    Debug.Log("-------------------" + ret_code);
                    Debug.Log("SUCCESS");
                    EventDispatcher.Instance().DispatchEvent("LOGIN_RESPOND", script_name_list);
                    m_UnitySocketClient.StartHeartbeatThread();
                }
                else if (ret_code == (int)DataHandler.LoginRetCode.LOGIN_NONSUPPORT)
                {
                    Debug.Log("FAILED No Support Version");
                }
                else
                {
                    Debug.Log("FAILED");
                }
                break;
            case DataHandler.DEALNUM_SET_SCRIPT_RESPOND:
                if (ret_code == (int)DataHandler.SetScriptRetCode.SETSCRIPT_SUCCESS)
                {
                    Debug.Log("Set Script Success");
                }
                else if (ret_code == (int)DataHandler.SetScriptRetCode.SETSCRIPT_NO_SCRIPT)
                {
                    Debug.Log("Set Script Failed <Has No Script On The Server");
                }
                else
                {
                    Debug.Log("Set Script Error");
                }
                break;

            case DataHandler.DEALNUM_PLAY_SCRIPT_RESPOND:
                if (ret_code == (int)DataHandler.PlayScriptRetCode.PLAYSCRIPT_SUCCESS)
                {
                    Debug.Log("Play Script Success");
                }
                else if (ret_code == (int)DataHandler.PlayScriptRetCode.PLAYSCRIPT_END)
                {
                    EventDispatcher.Instance().DispatchEvent("PLAYSCRIPT_END", "playend");
                    Debug.Log("Play Script End");
                }
                else
                {
                    Debug.Log("Play Script Error");
                }
                break;

            case DataHandler.DEALNUM_PLAY_FRAME_END_RESPOND:
                int[] data_arr = DataHandler.GetFrameEndRespondInfo(data);
                EventDispatcher.Instance().DispatchEvent("PLAY_FRAME_END_RESPOND", data_arr);
                break;
            default:
                break;
        }

        
        
    }

    public void SendDataToServer(byte[] data)
    {
        m_UnitySocketClient.Send(data);
    }

    public void DisConnecttion()
    {
        if (m_UnitySocketClient != null)
        {
            m_UnitySocketClient.Close();
        }
    }

    void OnDisable()
    {
        if (m_UnitySocketClient != null)
        {
            m_UnitySocketClient.m_SocketReceive.m_ReceiveThread.Abort();
        }

        DisConnecttion();
    }
}
