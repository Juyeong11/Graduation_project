using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

using System.Runtime.InteropServices;

public class Network
{
    static public Queue<byte[]> MessQueue = new Queue<byte[]>();

    const int BUFSIZE = 256;

    Socket ClientSocket;

    byte[] receiveBytes = new byte[BUFSIZE];
    byte[] tempBytes = new byte[BUFSIZE*2];

    public bool isOnline = false;

    int pre_buf_size = 0;
    void receiveComplet(System.IAsyncResult ar)
    {
        Socket c_Socket = (Socket)ar.AsyncState;
        int strLength = c_Socket.EndReceive(ar);

        int index = 0;

        //이전에 받던 데이터가 있으면
        if(pre_buf_size != 0)
        {
            Buffer.BlockCopy(receiveBytes, 0, tempBytes, pre_buf_size, strLength);
            //Array.Clear(receiveBytes, 0, receiveBytes.Length);
           
        }
        else
        {
            Buffer.BlockCopy(receiveBytes, 0, tempBytes, 0, strLength);
        }
        //.수정필요
        while (true)
        {
            int size = tempBytes[index];
            if (index + size > strLength + pre_buf_size)
            {
                //패킷이 덜 옴
                Buffer.BlockCopy(tempBytes, 0, tempBytes, index, strLength - index);
                pre_buf_size = strLength - index;
                break;
            }
            //Debug.Log(size);
            byte[] temp = new byte[size];
            Buffer.BlockCopy(tempBytes, index, temp, 0, size);
            index += size;
           
            if(index == strLength + pre_buf_size)
            {
                pre_buf_size = 0;
                MessQueue.Enqueue(temp);
                break;
            }
            MessQueue.Enqueue(temp);
        }

        
        ClientSocket.BeginReceive(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveComplet), ClientSocket);
    }
    void sendComplet(System.IAsyncResult ar)
    {
        Socket c_s = (Socket)ar.AsyncState;
        int strLength = c_s.EndSend(ar);
    }

    void connectComplet(System.IAsyncResult ar)
    {

        Socket tempSocket = (Socket)ar.AsyncState;
        IPEndPoint severEP = (IPEndPoint)tempSocket.RemoteEndPoint;
        if (severEP == null && tempSocket != null)
        {
            Debug.Log("재연결 시도");
            StartConnect();
            return;
        }
        Debug.Log("서버 연결 성공");
        isOnline = true;
        tempSocket.EndConnect(ar);
        ClientSocket.BeginReceive(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveComplet), ClientSocket);

        SendLogIn();



    }
    public void CreateAndConnect()
    {
        ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        StartConnect();
    }
    public void StartConnect()
    {
        try
        {
            ClientSocket.BeginConnect("127.0.0.1", 4000, new AsyncCallback(connectComplet), ClientSocket);
        }
        catch (SocketException ex)
        {
            // if(ex.)
            Debug.Log(ex.SocketErrorCode);
            CreateAndConnect();
        }
    }
    public void SendLogIn()
    {
        Protocol.cs_packet_login pk = new Protocol.cs_packet_login();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_login));
        pk.type = Protocol.CONSTANTS.CS_PACKET_LOGIN;

        ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);

    }
    public void CloseSocket()
    {
        ClientSocket.Close();
    }
    public void SendMovePacket(byte dir)
    {

        Protocol.cs_packet_move pk = new Protocol.cs_packet_move();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_move));
        pk.type = Protocol.CONSTANTS.CS_PACKET_MOVE;

        pk.direction = dir;

        ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendChangeSceneReadyPacket(byte isReady)
    {

        Protocol.cs_packet_change_scene_ready pk = new Protocol.cs_packet_change_scene_ready();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_change_scene_ready));
        pk.type = Protocol.CONSTANTS.CS_PACKET_CHANGE_SCENE_READY;

        pk.is_ready = isReady;

        ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendGameStartReadyPacket()
    {

        Protocol.cs_packet_game_start_ready pk = new Protocol.cs_packet_game_start_ready();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_game_start_ready));
        pk.type = Protocol.CONSTANTS.CS_PACKET_GAME_START_READY;

        ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendreadPacket()
    {
        Protocol.cs_packet_read_map pk = new Protocol.cs_packet_read_map();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_read_map));
        pk.type = Protocol.CONSTANTS.CS_PACKET_READ_MAP;


        ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }
    public void SendWriteMapPacket(ArrayList datas)
    {
        foreach (Protocol.Map m in datas)
        {
            Protocol.cs_packet_write_map pk = new Protocol.cs_packet_write_map();
            pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_write_map));
            pk.type = Protocol.CONSTANTS.CS_PACKET_WRITE_MAP;

            pk.id = m.id;
            pk.x = m.x;
            pk.y = m.y;
            pk.z = m.z;
            pk.w = m.w;

            pk.color = m.color;
            pk.block_type = m.type;

            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
        }

        Protocol.cs_packet_write_map end_pk = new Protocol.cs_packet_write_map();
        end_pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_write_map));
        end_pk.type = Protocol.CONSTANTS.CS_PACKET_WRITE_MAP;

        end_pk.id = -1;
        end_pk.x = -1;
        end_pk.y = -1;
        end_pk.z = -1;
        end_pk.w = -1;

        end_pk.color = -1;
        end_pk.block_type = -1;

        ClientSocket.BeginSend(end_pk.GetBytes(), 0, end_pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);

    }
}
