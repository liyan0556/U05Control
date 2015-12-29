using UnityEngine;
using System.Collections;
using KBEngine;
using System.Text;
using System.Collections.Generic;

public class DataHandler {

    public static byte DH_Head = 0x80;                                                         //起始位0
    public static byte DL_Head = 0x00;                                                         //起始位1

    public const short DEALNUM_HEARTBEAT = 0x03;                                               //心跳包
    public const short DEALNUM_HEARTBEAT_RESPOND = 0x04;                                       //心跳响应包
    public const short DEALNUM_LOGIN_REQUEST = 0x05;                                           //登录信息包
    public const short DEALNUM_LOGIN_RESPOND = 0x06;                                           //响应登录包

    public const short DEALNUM_PLAY_FRAME_END_RESPOND = 0x09;                                   //脚本播放帧结束通知
    
    public const short DEALNUM_KEY_SEND = 0x0A;                                                //发送键盘信息
    public const short DEALNUM_SET_SCRIPT = 0x0C;                                              //设置脚本
    public const short DEALNUM_SET_SCRIPT_RESPOND = 0x0D;                                      //响应设置脚本
    
    public const short DEALNUM_PLAY_SCRIPT = 0x0E;                                             //脚本播放
    public const short DEALNUM_PLAY_SCRIPT_RESPOND = 0x0F;                                      //响应脚本播放


    public enum DancingNum : byte
    {
        DANCING_NUM_0 = 48,
        DANCING_NUM_1,
        DANCING_NUM_2,
        DANCING_NUM_3,
        DANCING_NUM_4,
        DANCING_NUM_5,
        DANCING_NUM_6,
        DANCING_NUM_7,
        DANCING_NUM_8,
        DANCING_NUM_9
    }

    public enum LoginRetCode : int
    {
        LOGIN_SUCCESS = 1,
        LOGIN_FAILED = 2,
        LOGIN_NONSUPPORT = 98
    }

    public enum SetScriptRetCode : int
    {
        SETSCRIPT_FAILED = 0,
        SETSCRIPT_SUCCESS,
        SETSCRIPT_NO_SCRIPT
    }

    public enum PlayScriptRetCode : int
    {
        PLAYSCRIPT_FAILED = 0,
        PLAYSCRIPT_SUCCESS,
        PLAYSCRIPT_END
    }

    public static void DataPacking(byte[] data, byte dealnum)
    {

    }

    public static byte[] PackingKeyControlInfo(byte key)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.writeUint8(DH_Head);
        mStream.writeUint8(DL_Head);

        mStream.writeInt16(DEALNUM_KEY_SEND);

        mStream.writeInt32(0x01);

        mStream.writeUint8(key);

        byte[] result = mStream.getbuffer();

        return result;
    }

    public static byte[] PackingMovingControlInfo(string move)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.writeUint8(DH_Head);
        mStream.writeUint8(DL_Head);

        mStream.writeInt16(DEALNUM_KEY_SEND);

        mStream.writeInt32(0x01);

        mStream.writeUint8(getMoveType(move));

        byte[] result = mStream.getbuffer();
        return result;
    }

    public static byte getMoveType(string movetype)
    {
        byte type = 0xFF;
        switch (movetype)
        {
            case "开启底盘":
                type = (byte)'e';
                break;
            case "关闭底盘":
                type = (byte)'c';
                break;
            case "左转":
                type = (byte)'r';
                break;
            case "左平移":
                type = (byte)'a';
                break;
            case "前进":
                type = (byte)'w';
                break;
            case "后退":
                type = (byte)'s';
                break;
            case "右转":
                type = (byte)'t';
                break;
            case "右平移":
                type = (byte)'d';
                break;
            case "开启控制":
                type = (byte)'p';
                break;
            case "停止":
                type = (byte)' ';
                break;
            default:
                break;
        }

        MonoBehaviour.print("The Byte Typte is " + type);

        return type;
    }

    public static byte[] PackingLoginInfo(string data)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.writeUint8(DH_Head);
        mStream.writeUint8(DL_Head);

        mStream.writeInt16(DEALNUM_LOGIN_REQUEST);

        mStream.writeInt32(0x28);
        mStream.writeInt32(3);
        mStream.writeInt32(10);

        byte[] username = Encoding.UTF8.GetBytes("yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy");
        Debug.Log("Username length is " + username.Length);
        mStream.append(username, 0, 32);

        return mStream.getbuffer();
    }

    public static byte[] PackingHeartbeatInfo()
    {
        MemoryStream mStream = new MemoryStream();
        mStream.writeUint8(DH_Head);
        mStream.writeUint8(DL_Head);

        mStream.writeInt16(DEALNUM_HEARTBEAT);
        mStream.writeInt32(0);

        return mStream.getbuffer();
    }

    public static short UnPackDataType(byte[] data)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.setData(data);
        byte head0 = mStream.readUint8();
        byte head1 = mStream.readUint8();

        if (!(head0 == DH_Head && head1 == DL_Head))
        {
            Debug.LogError("THE DATA HEAD IS ERROR <" + head0 + ","  + head1 + ">");
            return 0xFF;
        }

        short dealNum = mStream.readInt16();
        if (!(dealNum == DEALNUM_LOGIN_RESPOND || dealNum == DEALNUM_SET_SCRIPT_RESPOND || dealNum == DEALNUM_PLAY_FRAME_END_RESPOND || dealNum == DEALNUM_PLAY_SCRIPT_RESPOND))
        {
            if (dealNum == DEALNUM_HEARTBEAT_RESPOND)
            {
            }
            else
            {
                Debug.LogError("THE DATA DEALNUM IS ERROR <" + dealNum + ">");
            }
            return 0xFF;
        }

        //int datalength = mStream.readInt32();
        //if (datalength <= 0)
        //{
        //    Debug.LogError("THE DATA LENGTH IS ERROR <" + datalength + ">");
        //    return 0xFF;
        //}

        //int ret_code = mStream.readInt32();
        return dealNum;
    }

    public static int GetRetCode(byte[] data)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.setData(data);

        mStream.readSkip(8);

        int ret_code = mStream.readInt32();
        return ret_code;
    }

    public static List<string> GetScriptNameList(byte[] data)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.setData(data);

        mStream.readSkip(44);

        int statueinfolength = mStream.readInt32();
        int enableflag = mStream.readInt32();
        int errorcode = mStream.readInt32();

        int list_length = mStream.readInt32();
        byte[] str_byte_data = new byte[list_length];
        for (int i = 0; i < list_length; i++)
        {
            str_byte_data[i] = mStream.readUint8();
        }

        string strdata = Encoding.UTF8.GetString(str_byte_data);
        Debug.Log("The Action Data string is <" + strdata + ">");

        string[] action_data_arr = strdata.Split('|');

        List<string> action_data_list = new List<string>(action_data_arr);
        //action_data_list.RemoveAt(0);
        //action_data_list.RemoveAt(action_data_list.Count - 1);
        action_data_list.Remove("");
        action_data_list.Remove("");
        return action_data_list;
    }

    public static byte[] PackingSetScriptInfo(string scriptname)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.writeUint8(DH_Head);
        mStream.writeUint8(DL_Head);

        mStream.writeInt16(DEALNUM_SET_SCRIPT);
        int packlength = scriptname.Length + 4;
        mStream.writeInt32(packlength);

        mStream.writeInt32(77);
        byte[] namebytes = Encoding.UTF8.GetBytes(scriptname);
        mStream.append(namebytes, 0, (uint)namebytes.Length);

        return mStream.getbuffer();
    }

    public static byte[] PakingPlayScriptInfo()
    {
        MemoryStream mStream = new MemoryStream();
        mStream.writeUint8(DH_Head);
        mStream.writeUint8(DL_Head);

        mStream.writeInt16(DEALNUM_PLAY_SCRIPT);
        mStream.writeInt32(4);
        mStream.writeInt32(0);
        return mStream.getbuffer();
    }


    public static int[] GetFrameEndRespondInfo(byte[] data)
    {
        MemoryStream mStream = new MemoryStream();
        mStream.setData(data);

        mStream.readSkip(8);
        int[] info_arr = new int[3];
        info_arr[0] = mStream.readInt32();
        info_arr[1] = mStream.readInt32();
        info_arr[2] = mStream.readInt32();

        return info_arr;
    }
}
