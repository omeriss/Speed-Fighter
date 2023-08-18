using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    public static void SendUdpData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            if (Server.clients[i] != null && Server.clients[i].tcp != null && Server.clients[i].tcp.socket != null)
                Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(Packet packet, int ex)
    {
        packet.WriteLength();
        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            if (i != ex && Server.clients[i] != null && Server.clients[i].tcp != null && Server.clients[i].tcp.socket != null)
                Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendUdpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            if (Server.clients[i] != null)
                Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUdpDataToAll(Packet packet, int ex)
    {
        packet.WriteLength();
        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            if (Server.clients[i] != null && i != ex)
                Server.clients[i].udp.SendData(packet);
        }
    }

    public static void Ping(int toClient, string messege)
    {
        using (Packet packet = new Packet((int)ServerPackets.ping))
        {
            packet.Write(messege);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.userName);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(toClient, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUdpDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.PlayerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUdpDataToAll(packet, player.id);
        }
    }

    public static void DisconnectPlayer(int disconnectedId)
    {
        using (Packet packet = new Packet((int)ServerPackets.Disconnect))
        {
            packet.Write(disconnectedId);
            SendTCPDataToAll(packet);
        }
    }

    public static void SendChatMassage(string massage)
    {
        using (Packet packet = new Packet((int)ServerPackets.ChatMassage))
        {
            packet.Write(massage);
            SendTCPDataToAll(packet);
        }
    }

    public static void SendChatMassage(string massage, int ex)
    {
        using (Packet packet = new Packet((int)ServerPackets.ChatMassage))
        {
            packet.Write(massage);
            SendTCPDataToAll(packet, ex);
        }
    }

    public static void SendState(int toClient, float hp)
    {
        using (Packet packet = new Packet((int)ServerPackets.state))
        {
            packet.Write(toClient);
            packet.Write(hp);
            SendTCPDataToAll(packet);
        }
    }

    public static void SendHit(int toClient, bool hit, bool killled)
    {
        using (Packet packet = new Packet((int)ServerPackets.hit))
        {
            packet.Write(toClient);
            packet.Write(hit);
            packet.Write(killled);
            SendTCPData(toClient, packet);
        }
    }

    public static void SendBullets(int toClient, int bullets)
    {
        using (Packet packet = new Packet((int)ServerPackets.bullets))
        {
            packet.Write(toClient);
            packet.Write(bullets);
            SendTCPData(toClient, packet);
        }
    }

    public static void SendNextShot(int toClient, float nextShot)
    {
        using (Packet packet = new Packet((int)ServerPackets.nextShot))
        {
            packet.Write(toClient);
            packet.Write(nextShot);
            SendTCPData(toClient, packet);
        }
    }

    public static void SendLeaderBoard(int toClient, string leaderBoard)
    {
        using (Packet packet = new Packet((int)ServerPackets.leaderBoard))
        {
            packet.Write(leaderBoard);
            SendTCPData(toClient, packet);
        }
    }

    public static void ChangeAnimation(int ofClient, int animation)
    {
        using (Packet packet = new Packet((int)ServerPackets.animation))
        {
            packet.Write(ofClient);
            packet.Write(animation);
            SendTCPDataToAll(packet, ofClient);
        }
    }

    public static void SendMap(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.sendMap))
        {
            packet.Write(NetworkManager.instance.MAP);
            SendTCPData(toClient, packet);
        }
    }

}
