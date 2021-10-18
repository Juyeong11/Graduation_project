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

    void receiveComplet(System.IAsyncResult ar)
    {
        Socket c_Socket = (Socket)ar.AsyncState;
        int strLength = c_Socket.EndReceive(ar);

        int index = 0;

        while (index < strLength)
        {
            int size = receiveBytes[index];
            Debug.Log(size);
            byte[] temp = new byte[size];
            Buffer.BlockCopy(receiveBytes, index, temp, 0, size);
            index += size;

            MessQueue.Enqueue(temp);
        }

        ClientSocket.BeginReceive(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveComplet), ClientSocket);
    }
    void sendComplet(System.IAsyncResult ar)
    {
        Socket c_s = (Socket)ar.AsyncState;
        int strLength = c_s.EndSend(ar);
    }
    public void CreateAndConnect()
    {
        ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        ClientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 4000));

        ClientSocket.BeginReceive(receiveBytes, 0, BUFSIZE, SocketFlags.None, new System.AsyncCallback(receiveComplet), ClientSocket);


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
}
