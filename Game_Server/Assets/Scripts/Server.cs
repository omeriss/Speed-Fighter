using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static TcpListener tcpListener;
    public static Client[] clients;
    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandelers;
    public static UdpClient udpLisener;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;
        clients = new Client[maxPlayers];

        packetHandelers = new Dictionary<int, PacketHandler>()
            {
                {(int) ClientPackets.pingBack, ServerHandle.PinggedBack },
                {(int) ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                {(int) ClientPackets.shoot, ServerHandle.PlayerShoot },
                {(int) ClientPackets.reload, ServerHandle.Reload },
                {(int) ClientPackets.askLeaderBoard, ServerHandle.LeaderBoard },
                {(int) ClientPackets.askMap, ServerHandle.AskMap },
                {(int) ClientPackets.sendChatMassage, ServerHandle.GetChatMassage},

            };

        udpLisener = new UdpClient(port);
        udpLisener.BeginReceive(UdpReceive, null);

        tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnect), null);
    }

    private static void TCPConnect(IAsyncResult result)
    {
        //מאשר את הלקוח ומתחיל את ההאזנה ללקוחות מחדש
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnect), null);

        for (int i = 0; i < MaxPlayers; i++)
        {
            if (clients[i] == null)
            {
                clients[i] = new Client(i);
            }
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                FirebaseManager.UpdateConnected();
                return;
            }
        }


        Debug.Log("server is full");
    }

    public static void UdpReceive(IAsyncResult result)
    {
        try
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpLisener.EndReceive(result, ref iPEndPoint);
            udpLisener.BeginReceive(UdpReceive, null);
            if (data.Length < 4)
                return;
            using (Packet packet = new Packet(data))
            {
                int clientid = packet.ReadInt();
                if (clients[clientid].udp.endPoint == null)
                {
                    clients[clientid].udp.Connect(iPEndPoint);
                    return;
                }
                if (clients[clientid].udp.endPoint.ToString() == iPEndPoint.ToString())
                {
                    clients[clientid].udp.HandleData(packet);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public static void SendUdpData(IPEndPoint iPEndPoint, Packet packet)
    {
        try
        {
            if (iPEndPoint != null)
            {
                udpLisener.BeginSend(packet.ToArray(), packet.Length(), iPEndPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}

