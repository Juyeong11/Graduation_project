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
    static public int[] ping_data = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
    static public int ping_index;

    const int BUFSIZE = 512;

    Socket ClientSocket;

    byte[] receiveBytes = new byte[BUFSIZE];


    public bool debugOnline = false;
    public bool isOnline = false;

    int pre_buf_size = 0;
    void receiveComplet(System.IAsyncResult ar)
    {
        Socket c_Socket = (Socket)ar.AsyncState;
        int strLength = c_Socket.EndReceive(ar);

        int data_size = pre_buf_size + strLength;
        int packet_start_index = 0;
        int packet_size = receiveBytes[packet_start_index];

        while (packet_size <= data_size)
        {
            //
            byte[] packet = new byte[packet_size];
            Buffer.BlockCopy(receiveBytes, packet_start_index, packet, 0, packet_size);
            MessQueue.Enqueue(packet);

            //
            data_size -= packet_size;
            packet_start_index += packet_size;
            if (data_size > 0) packet_size = receiveBytes[packet_start_index];
        }
        pre_buf_size = data_size;

        // 
        if (data_size > 0) Buffer.BlockCopy(receiveBytes, data_size, receiveBytes, 0, data_size);

        ClientSocket.BeginReceive(receiveBytes, pre_buf_size, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveComplet), ClientSocket);


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
            //Debug.Log("재연결 시도");
            StartConnect();
            return;
        }
        Debug.Log("서버 연결 성공");
        isOnline = true;
        tempSocket.EndConnect(ar);
        ClientSocket.BeginReceive(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveComplet), ClientSocket);
        var r = new System.Random();
        SendLogIn("Happy" + r.Next(0, 128));
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

    static public int GetPingAvg()
    {
        int i = 0;
        int sum = 0;
        for (; i < ping_data.Length; ++i)
        {
            if (ping_data[i] == -1) break;
            sum += ping_data[i];
        }
        return sum / i;
    }
    public void SendLogIn(string name)
    {
        Protocol.cs_packet_login pk = new Protocol.cs_packet_login();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_login));
        pk.type = Protocol.CONSTANTS.CS_PACKET_LOGIN;
        pk.name = System.Text.Encoding.UTF8.GetBytes(name + '\0');
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);

    }
    public void CloseSocket()
    {
        if (isOnline)
            ClientSocket.Close();
    }
    public void SendMovePacket(byte dir)
    {

        Protocol.cs_packet_move pk = new Protocol.cs_packet_move();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_move));
        pk.type = Protocol.CONSTANTS.CS_PACKET_MOVE;

        pk.direction = dir;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }
    public void SendParryingPacket()
    {
        Protocol.cs_packet_parrying pk = new Protocol.cs_packet_parrying();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_parrying));
        pk.type = Protocol.CONSTANTS.CS_PACKET_PARRYING;


        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendUseSkillPacket()
    {
        Protocol.cs_packet_use_skill pk = new Protocol.cs_packet_use_skill();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_use_skill));
        pk.type = Protocol.CONSTANTS.CS_PACKET_USE_SKILL;


        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }
    public void SendChangeSkillPacket(byte skill_type)
    {
        Protocol.cs_packet_change_skill pk = new Protocol.cs_packet_change_skill();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_change_skill));
        pk.type = Protocol.CONSTANTS.CS_PACKET_CHANGE_SKILL;
        pk.skill_type = skill_type;


        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendChangeSceneReadyPacket(byte isReady)
    {

        Protocol.cs_packet_change_scene_ready pk = new Protocol.cs_packet_change_scene_ready();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_change_scene_ready));
        pk.type = Protocol.CONSTANTS.CS_PACKET_CHANGE_SCENE_READY;

        pk.is_ready = isReady;

        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }
    public void SendChangeSceneDonePacket(byte scene_num)
    {

        Protocol.cs_packet_change_scene_done pk = new Protocol.cs_packet_change_scene_done();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_change_scene_done));
        pk.type = Protocol.CONSTANTS.CS_PACKET_CHANGE_SCENE_DONE;
        pk.scene_num = scene_num;

        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendGameStartReadyPacket()
    {

        Protocol.cs_packet_game_start_ready pk = new Protocol.cs_packet_game_start_ready();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_game_start_ready));
        pk.type = Protocol.CONSTANTS.CS_PACKET_GAME_START_READY;

        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendreadPacket()
    {
        Protocol.cs_packet_read_map pk = new Protocol.cs_packet_read_map();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_read_map));
        pk.type = Protocol.CONSTANTS.CS_PACKET_READ_MAP;

        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendPingTestPacket()
    {
        Protocol.cs_packet_ping_test pk = new Protocol.cs_packet_ping_test();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_ping_test));
        pk.type = Protocol.CONSTANTS.CS_PACKET_PING_TEST;
        pk.ping_time = System.DateTime.Now.Millisecond;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }
    public void SendPartyRequestPacket(int id)
    {
        Protocol.cs_packet_party_request pk = new Protocol.cs_packet_party_request();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_party_request));
        pk.type = Protocol.CONSTANTS.CS_PACKET_PARTY_REQUEST;
        pk.id = id;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendPartyRequestAnwserPacket(byte type, int id)
    {
        Protocol.cs_packet_party_request_anwser pk = new Protocol.cs_packet_party_request_anwser();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_party_request_anwser));
        pk.type = Protocol.CONSTANTS.CS_PACKET_PARTY_REQUEST_ANWSER;
        pk.requester = id;
        pk.anwser = type;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendChatMess(string mess, byte sendType, int reciver)
    {
        Protocol.cs_packet_chat pk = new Protocol.cs_packet_chat();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_chat));
        pk.type = Protocol.CONSTANTS.CS_PACKET_CHAT;
        pk.sendType = sendType;
        pk.reciver = reciver;
        Buffer.BlockCopy(System.Text.Encoding.UTF8.GetBytes(mess), 0, pk.mess, 0, System.Text.Encoding.UTF8.GetBytes(mess).Length);


        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendSetPath(short x, short z)
    {
        Protocol.cs_packet_set_path pk = new Protocol.cs_packet_set_path();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_set_path));
        pk.type = Protocol.CONSTANTS.CS_PACKET_SET_PATH;
        pk.x = x;
        pk.z = z;


        if (isOnline)
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
    public void SendTeleportPacket(byte pos)
    {
        Protocol.cs_packet_teleport pk = new Protocol.cs_packet_teleport();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_teleport));
        pk.type = Protocol.CONSTANTS.CS_PACKET_TELEPORT;
        pk.pos = pos;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendBuyPacket(byte itemType)
    {
        Protocol.cs_packet_buy pk = new Protocol.cs_packet_buy();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_buy));
        pk.type = Protocol.CONSTANTS.CS_PACKET_BUY;
        pk.itemType = itemType;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }

    public void SendUseItemPacket(byte itemType)
    {
        Protocol.cs_packet_use_item pk = new Protocol.cs_packet_use_item();
        pk.size = (byte)Marshal.SizeOf(typeof(Protocol.cs_packet_use_item));
        pk.type = Protocol.CONSTANTS.CS_PACKET_USE_ITEM;
        pk.itemType = itemType;
        if (isOnline)
            ClientSocket.BeginSend(pk.GetBytes(), 0, pk.size, SocketFlags.None, new System.AsyncCallback(sendComplet), ClientSocket);
    }
    public bool isServerOnline()
    {
        return debugOnline || isOnline;
    }
}
