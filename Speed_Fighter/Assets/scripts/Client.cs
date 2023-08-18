using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;


public enum ServerPackets
{
    ping = 1, spawnPlayer = 2, playerPosition, PlayerRotation, ChatMassage, Disconnect, state, hit, bullets, nextShot, leaderBoard, animation, sendMap
}



public enum ClientPackets
{
    pingBack = 1, playerMovement = 2, shoot, reload, askLeaderBoard, askMap, sendChatMassage
}


public class TCP
{
    public TcpClient socket;
    private NetworkStream stream;
    private Packet recivedData;
    private byte[] receiveBuffer;


    public void Connect()
    {
        socket = new TcpClient { ReceiveBufferSize = Client.DataBufferSize, SendBufferSize = Client.DataBufferSize };
        receiveBuffer = new byte[Client.DataBufferSize];
        socket.BeginConnect(Client.instance.serverIp, Client.instance.port, Connected, socket);
        
    }

    private void Connected(IAsyncResult result)
    {
        socket.EndConnect(result);
        if (!socket.Connected)
            return;
        //Debug.Log("yey in works");
        stream = socket.GetStream();

        recivedData = new Packet();

        stream.BeginRead(receiveBuffer, 0, Client.DataBufferSize, ReceiveData, null);

    }


    private void ReceiveData(IAsyncResult result)
    {
        try
        {
            int byteLength = stream.EndRead(result);
            if (byteLength <= 0)
            {
                //change disconnect
                Client.instance.Disconnect();
                return;
            }

            byte[] data = new byte[byteLength];
            Array.Copy(receiveBuffer, data, byteLength);

            recivedData.Reset(HandleData(data));

            stream.BeginRead(receiveBuffer, 0, Client.DataBufferSize, ReceiveData, null);
        }
        catch
        {
            Disconnect();
        }
    }

    public void SendData(Packet packet)
    {
        try
        {
            if (socket != null)
            {
                stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }
    }


    private bool HandleData(byte[] data)
    {
        int packetLen = 0;
        recivedData.SetBytes(data);
        if (recivedData.UnreadLength() >= 4)
        {
            packetLen = recivedData.ReadInt();
            if (packetLen <= 0)
                return true;
        }
        if (packetLen > recivedData.UnreadLength())
            Debug.Log("test");
        while (packetLen > 0 && packetLen <= recivedData.UnreadLength())
        {
            byte[] packetBytes = recivedData.ReadBytes(packetLen);
            Executor.ExecuteAction(() =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Client.PacketHandlers[packetId](packet);
                }
            });
            packetLen = 0;

            if (recivedData.UnreadLength() >= 4)
            {
                packetLen = recivedData.ReadInt();
                if (packetLen <= 0)
                    return true;
            }

            if (packetLen > recivedData.UnreadLength())
                Debug.Log("test");
        }
        if (packetLen > 0)
            Debug.Log("test1");
        if (packetLen <= 1)
            return true;

        return false;


    }

    private void Disconnect()
    {
        Client.instance.Disconnect();
        stream = null;
        receiveBuffer = null;
        recivedData = null;
        socket = null;
    }

}

public class UDP
{
    public UdpClient udpSocket;
    public IPEndPoint endPoint;

    public UDP()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(Client.instance.serverIp), Client.instance.port);
    }

    public void Connect(int port)
    {
        udpSocket = new UdpClient(port);
        udpSocket.Connect(endPoint);
        udpSocket.BeginReceive(ReceiveData, null);
        using(Packet packet = new Packet())
        {
            packet.WriteLength();
            SendData(packet);
        }
    }

    public void SendData(Packet packet)
    {
        try
        {
            packet.InsertInt(Client.instance.myId);
            if(udpSocket != null)
            {
                udpSocket.BeginSend(packet.ToArray(), packet.Length(), null, null);

            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    private void ReceiveData(IAsyncResult result)
    {
        try
        {
            byte[] data = udpSocket.EndReceive(result, ref endPoint);
            udpSocket.BeginReceive(ReceiveData, null);
            if (data.Length < 4)
            {
                Client.instance.Disconnect();
                return;
            }
            HandleData(data);
        }
        catch (Exception e)
        {
            Disconnect();
        }
    }
    private void HandleData(byte[] data)
    {
        using(Packet packet = new Packet(data))
        {
            int len = packet.ReadInt();
            data = packet.ReadBytes(len);
        }
        Executor.ExecuteAction(() =>
        {
            using(Packet packet = new Packet(data))
            {
                int packetid = packet.ReadInt();
                Client.PacketHandlers[packetid](packet) ;
            }
        });
    }
    private void Disconnect()
    {
        Client.instance.Disconnect();

        endPoint = null;
        udpSocket = null;
    }
}



public class Client : MonoBehaviour
{
    public static int DataBufferSize = 4096;
    public static Client instance;

    public int myId;
    public TCP tcp;
    public UDP udp;

    public delegate void PacketHandler(Packet packet);
    public static Dictionary<int, PacketHandler> PacketHandlers;


    public bool isConnected = false;

    public int port = 16016;
    public string serverIp = "5.29.137.8";

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this);
        }
    }


    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        ClientStart();
    }

    public void ClientStart()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer(string ip = "", int ConPort = -1)
    {
        udp.endPoint = new IPEndPoint(IPAddress.Parse(serverIp), Client.instance.port);
        if (ip != "")
            serverIp = ip;
        if (ConPort != -1)
            port = ConPort;

        Debug.Log($"connecting to {serverIp}:{port}");

        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.ping, ClientHandle.Ping},
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            { (int)ServerPackets.playerPosition,  ClientHandle.PlayerPosition},
            { (int)ServerPackets.PlayerRotation,  ClientHandle.PlayerRotation},
            { (int)ServerPackets.Disconnect,  ClientHandle.DisconnectPlayer},
            { (int)ServerPackets.ChatMassage,  ClientHandle.DesplayChatMassage},
            { (int)ServerPackets.state,  ClientHandle.GetState},
            { (int)ServerPackets.hit, ClientHandle.GetHit},
            { (int)ServerPackets.bullets,  ClientHandle.SetBullets},
            { (int)ServerPackets.nextShot,  ClientHandle.SetTimeToNextShot},
            { (int)ServerPackets.leaderBoard,  ClientHandle.LeaderBoard},
            { (int)ServerPackets.animation,  ClientHandle.SetAnimation},
            { (int)ServerPackets.sendMap,  ClientHandle.SetMap},
        };

        isConnected = true;
        tcp.Connect();
    }



    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.udpSocket.Close();
            Debug.Log("disconnected");
            UIManager.instance.gameObject.SetActive(true);
        }

    }

}
