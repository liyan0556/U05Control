using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using KBEngine;
using System;

public class UnitySocketClient {
    private int m_Port = 2810;
    private string m_IP = "192.168.11.2";
    private Thread m_HeartbeatThread = null;
    public Socket m_Socket = null;

    public SocketReceive m_SocketReceive = null;

    public delegate void SocketRecvDataHandler(byte[] data);
    public static event SocketRecvDataHandler m_SocketRecvDataHandler;

    public UnitySocketClient(string ip, int port)
    {
        this.m_Port = port;
        this.m_IP = ip;
    }

    public void SocketConnection()
    {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress ip = IPAddress.Parse(m_IP);
            IPEndPoint ipe = new IPEndPoint(ip, m_Port);
            m_Socket.Connect(ipe);
            MonoBehaviour.print("Connect Server <" + m_IP + "," + m_Port + "> Success");
        }
        catch (System.Exception e)
        {
            MonoBehaviour.print(e.ToString());
        }
    }

    public void CreateReceiveThread()
    {
        m_SocketReceive = new SocketReceive(m_Socket);
        m_SocketReceive.StartReceive();
    }

    public void StartHeartbeatThread()
    {
        m_HeartbeatThread = new Thread(SendHeartbeat);
        m_HeartbeatThread.Start();
    }

    public void SendHeartbeat()
    {
        while (true)
        {
            //Debug.Log("I am Heartbeat ...");
            Send(DataHandler.PackingHeartbeatInfo());
            Thread.Sleep(500);
        }
    }

    public bool IsUnitySocketClientConnected()
    {
        return IsSocketConnected(m_Socket);
    }

    /// 另一种判断connected的方法，但未检测对端网线断开或ungraceful的情况
    /// 
    /// 
    /// 
    static bool IsSocketConnected(Socket s)
    {
        #region remarks
        /* As zendar wrote, it is nice to use the Socket.Poll and Socket.Available, but you need to take into conside                ration 
             * that the socket might not have been initialized in the first place. 
             * This is the last (I believe) piece of information and it is supplied by the Socket.Connected property. 
             * The revised version of the method would looks something like this: 
             * from：http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c */
        #endregion

        #region 过程

        return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);

        /* The long, but simpler-to-understand version:

                bool part1 = s.Poll(1000, SelectMode.SelectRead);
                bool part2 = (s.Available == 0);
                if ((part1 && part2 ) || !s.Connected)
                    return false;
                else
                    return true;

        */
        #endregion
    }

    public void Close()
    {
        if (m_Socket != null)
        {
            m_Socket.Close(0);
            m_Socket = null;
        }

        if (m_SocketReceive != null)
        {
            m_SocketReceive.m_ReceiveThread.Abort();
        }

        if (m_HeartbeatThread != null)
        {
            m_HeartbeatThread.Abort();
        }
    }

    public void Send(byte[] data)
    {
        m_Socket.Send(data);
    }

    public class SocketReceive
    {
        Socket m_Socket = null;
        public Thread m_ReceiveThread = null;
        byte[] m_Buff = null;
        int m_BuffSize = 0;
        bool continueRecveive = false;

        public SocketReceive(Socket socket)
        {
            this.m_Buff = new byte[1024];
            this.m_Socket = socket;
            CreateReceiveThread();
        }

        public void CreateReceiveThread()
        {
            m_ReceiveThread = new Thread(ReceiveHandler);
        }

        public void StartReceive()
        {
            m_ReceiveThread.Start();
        }

        public void ReceiveHandler()
        {
            MonoBehaviour.print("Start ReceiveHandler");
            while (true)
            {
                try
                {
                    int length = m_Socket.Available;
                    if (length > 0)
                    {
                        byte[] recvBytes = new byte[length];
                        int recvcount = m_Socket.Receive(recvBytes, length, 0);
                        if (continueRecveive)
                        {
                            recvBytes.CopyTo(m_Buff, m_BuffSize);
                        }
                        else
                        {
                            recvBytes.CopyTo(m_Buff, 0);
                        }
                        m_BuffSize += recvcount;
                        string result = "";
                        for (int i = 0; i < length; i++)
                        {
                            result += recvBytes[i].ToString("X2") + " ";
                        }
                        MonoBehaviour.print(result + "[recvCount is " + m_BuffSize + "]");



                        MemoryStream stream = new MemoryStream();
                        stream.setData(m_Buff);
                        byte head0 = stream.readUint8();
                        byte head1 = stream.readUint8();

                        if (head0 == DataHandler.DH_Head && head1 == DataHandler.DL_Head)
                        {
                            short dealNum = stream.readInt16();
                            int datalength = stream.readInt32();
                            if (datalength == (m_BuffSize - 8))
                            {
                                //TODO 接受到完整命令包，进行下一步命令解析。
                                DataHandleThread m_Thread = new DataHandleThread();
                                byte[] finalData = new byte[m_BuffSize];
                                Array.Copy(m_Buff, finalData, datalength);
                                //m_Buff.CopyTo(finalData, 0);
                                m_Thread.SetData(finalData);
                                m_Thread.StartHandleThread();

                                m_BuffSize = 0;
                                m_Buff.Initialize();
                                continueRecveive = false;
                            }
                            else if (m_BuffSize <= (datalength - 8))
                            {
                                continueRecveive = true;
                                continue;
                            }
                            else
                            {
                                m_BuffSize = 0;
                                m_Buff.Initialize();
                                continueRecveive = false;
                            }
                        }
                        else
                        {
                            m_Buff.Initialize();
                            m_BuffSize = 0;
                            continueRecveive = false;
                        }

                    }

                    Thread.Sleep(5);
                }
                catch (System.Exception e)
                {
                    MonoBehaviour.print(e.ToString());
                    m_ReceiveThread.Abort();
                }
            }
        }

        class DataHandleThread
        {
            private Thread _HandleThread;
            private byte[] data;

            public void SetData(byte[] _data)
            {
                this.data = _data;
            }

            public DataHandleThread()
            {
                _HandleThread = new Thread(DataHandleFunction);
            }

            private void DataHandleFunction()
            {
                m_SocketRecvDataHandler(data);
            }

            public void StartHandleThread()
            {
                _HandleThread.Start();
            }
        }
    }
}
