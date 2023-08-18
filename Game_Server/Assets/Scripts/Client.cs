using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;


public class TCP
{
    public TcpClient socket;
    private int id;
    private NetworkStream stream;
    private Packet recivedData;
    private byte[] receiveBuffer;

    public TCP(int id)
    {
        this.id = id;
    }

    public void Connect(TcpClient socket)
    {
        this.socket = socket;
        this.socket.ReceiveBufferSize = Client.MaxBufferSize;
        this.socket.SendBufferSize = Client.MaxBufferSize;
        this.stream = socket.GetStream();
        receiveBuffer = new byte[Client.MaxBufferSize];
        recivedData = new Packet();
        Debug.Log("connected:" + id);
        stream.BeginRead(receiveBuffer, 0, Client.MaxBufferSize, ReceiveData, null);
        ServerSend.Ping(id, "you are connected to the server");
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
        catch (Exception exception)
        {
            Debug.Log("eror while sending data to " + id + ":" + exception);
        }
    }

    private void ReceiveData(IAsyncResult result)
    {
        try
        {
            int byteLength = stream.EndRead(result);
            if (byteLength <= 0)
            {
                Server.clients[id].Disconnect();
                return;
            }

            byte[] data = new byte[byteLength];
            Array.Copy(receiveBuffer, data, byteLength);

            recivedData.Reset(HandleData(data));

            stream.BeginRead(receiveBuffer, 0, Client.MaxBufferSize, ReceiveData, null);
        }
        catch
        {
            Server.clients[id].Disconnect();
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
                    Server.packetHandelers[packetId](id, packet);
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

    public void Disconnect()
    {
        socket.Close();
        stream = null;
        recivedData = null;
        receiveBuffer = null;
        socket = null;
    }
}

public class UDP
{
    public IPEndPoint endPoint;
    private int id;

    public UDP(int id)
    {
        this.id = id;
    }
    public void Connect(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;
    }
    public void SendData(Packet packet)
    {
        Server.SendUdpData(endPoint, packet);
    }
    public void HandleData(Packet packet)
    {
        int len = packet.ReadInt();
        byte[] packetBytes = packet.ReadBytes(len);
        Executor.ExecuteAction(() =>
        {
            using (Packet packeth = new Packet(packetBytes))
            {
                int packetid = packeth.ReadInt();
                Server.packetHandelers[packetid](id, packeth);
            }
        });
    }
    public void Disconnect()
    {
        endPoint = null;
    }
}

public class Client
{
    public static int MaxBufferSize = 4096;
    public int id;
    public TCP tcp;
    public UDP udp;
    public Player player;

    public Client(int id)
    {
        this.id = id;
        this.tcp = new TCP(id);
        this.udp = new UDP(id);
    }

    public void SendIntoGame(string userName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Intitialize(id, userName);
        foreach (Client c in Server.clients)
        {
            if (c != null && c.player != null && c.id != id)
            {
                ServerSend.SpawnPlayer(id, c.player);
            }
        }

        foreach (Client c in Server.clients)
        {
            if (c != null && c.player != null)
            {
                ServerSend.SpawnPlayer(c.id, player);
            }
        }
    }
    public void Disconnect()
    {
        Debug.Log(tcp.socket.Client.RemoteEndPoint + " ->disconnected");
        string username = player.userName;

        Executor.ExecuteAction(() =>
        {
            if(player!=null)
                UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();
        FirebaseManager.UpdateConnected();
        ServerSend.SendChatMassage(username + " disconnected");
        ServerSend.DisconnectPlayer(id);


    }
}

