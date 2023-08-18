using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ServerHandle
{
    public static void PinggedBack(int fromClient, Packet packet)
    {
        int clientId = packet.ReadInt();
        string userName = packet.ReadString();
        if(clientId != fromClient)
        {
            Server.clients[fromClient].Disconnect();
        }
        Debug.Log("cliend sent Ping Back: " + clientId);
        Server.clients[fromClient].SendIntoGame(userName);
    }
    public static void PlayerMovement(int fromClient, Packet packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }
        Quaternion rotation = packet.ReadQuaternion();

        if (Server.clients[fromClient] != null && Server.clients[fromClient].player != null)
        {
            Server.clients[fromClient].player.SetInputs(inputs, rotation);
        }
    }

    public static void PlayerShoot(int fromClient, Packet packet)
    {

        Vector3 shootdir = packet.ReadVector3();
        if (Server.clients[fromClient] != null && Server.clients[fromClient].player != null)
        {
            Server.clients[fromClient].player.Shoot(shootdir);
        }
    }

    public static void Reload(int fromClient, Packet packet)
    {
        if (Server.clients[fromClient] != null && Server.clients[fromClient].player != null)
        {
            Server.clients[fromClient].player.gun.Reload();
            ServerSend.SendNextShot(fromClient, (float)(Server.clients[fromClient].player.gun.nextShot - DateTime.Now).TotalMilliseconds / 1000f);
            ServerSend.SendBullets(fromClient, Server.clients[fromClient].player.gun.RemainBullets);
        }
    }



    private static bool Comperation(Tuple<int, string> t1, Tuple<int, string> t2)
    {
        return t1.Item1 < t2.Item1;
    }

    public static void LeaderBoard(int fromClient, Packet packet)
    {

        List<Tuple<int, string>> list = new List<Tuple<int, string>>();
        foreach (Client client in Server.clients)
        {
            if(client != null && client.player != null)
            {
                list.Add(new Tuple<int, string>(client.player.kills, client.player.userName));
            }
        }
        list.Sort(delegate (Tuple<int, string> t1, Tuple<int, string> t2) { return -1 * t1.Item1.CompareTo(t2.Item1); });

        string leaderBoardStr = "";

        int count = 0;
        foreach(var item in list)
        {
            count++;
            leaderBoardStr += count +"-"+ item.Item2 + ": " + item.Item1+" kills\n";
            if (count == 3)
                break;
        }

        ServerSend.SendLeaderBoard(fromClient, leaderBoardStr);
    }

    public static void AskMap(int fromClient, Packet packet)
    {
        ServerSend.SendMap(fromClient);
    }

    public static void GetChatMassage(int fromClient, Packet packet)
    {
        string massage = packet.ReadString();
        ServerSend.SendChatMassage(massage);
    }


}

