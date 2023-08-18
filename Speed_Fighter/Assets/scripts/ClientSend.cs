using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }
    private static void SendUdpData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    public static void PingBack()
    {
        using(Packet packet = new Packet((int)ClientPackets.pingBack))
        {
            packet.Write(Client.instance.myId);
            packet.Write((AccountManager.user != null) ? AccountManager.user.DisplayName : "guest");
            SendTcpData(packet);
        }
    }

    public static void PlayerMovement(bool[] inputs)
    {
        using(Packet packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);
            for(int i =0;i<inputs.Length; i++)
            {
                packet.Write(inputs[i]);
            }
            packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
            SendUdpData(packet);
        }
    }

    public static void Shoot(Vector3 look)
    {
        using (Packet packet = new Packet((int)ClientPackets.shoot))
        {
            packet.Write(look);
            SendTcpData(packet);
            
        }
    }

    public static void Reload()
    {
        using (Packet packet = new Packet((int)ClientPackets.reload))
        {
            SendTcpData(packet);

        }
    }

    public static void AskLeaderBoard()
    {
        using (Packet packet = new Packet((int)ClientPackets.askLeaderBoard))
        {
            SendTcpData(packet);
        }
    }

    public static void AskMap()
    {
        using (Packet packet = new Packet((int)ClientPackets.askMap))
        {
            SendTcpData(packet);
        }
    }

    public static void SendChatMassage(string massage)
    {
        using (Packet packet = new Packet((int)ClientPackets.sendChatMassage))
        {
            massage = "<"+((AccountManager.user != null) ? AccountManager.user.DisplayName : "guest")+"> " + massage;
            packet.Write(massage);
            SendTcpData(packet);
        }
    }


}
