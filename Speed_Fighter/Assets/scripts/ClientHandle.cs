using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClientHandle : MonoBehaviour
{



    public static void Ping(Packet packet)
    {
        string msg = packet.ReadString();
        int id = packet.ReadInt();
        Debug.Log(msg);
        Client.instance.myId = id;
        ClientSend.PingBack();
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);



        //close start ui
        if (!GuestJoin.IsGuest)
        {
            UIManager.instance.ClosePage(UIManager.instance.LogInMenue);
            UIManager.instance.ClosePage(UIManager.instance.SignUpMenue);
            UIManager.instance.ClosePage(UIManager.instance.AccountMenue);
            UIManager.instance.ClosePage(UIManager.instance.MainMenue);
            UIManager.instance.ClosePage(UIManager.instance.guestJoin);
        }
        else
        {
            ClientSend.AskMap();
        }
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        if (GameManager.players.ContainsKey(id))
            GameManager.players[id].transform.position = position;
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();
        if(GameManager.players.ContainsKey(id))
            GameManager.players[id].transform.rotation = rotation;
    }

    public static void DisconnectPlayer(Packet packet)
    {
        int id = packet.ReadInt();

        UnityEngine.GameObject.Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
        Debug.Log(id + " diconnected");
    }

    public static void DesplayChatMassage(Packet packet)
    {
        string massage = packet.ReadString();
        UIManager.instance.DesplayMassage(massage);
    }

    public static void GetState(Packet packet)
    {
        int id = packet.ReadInt();
        float hp = packet.ReadFloat();
        GameManager.players[id].SetHp(hp);
        if(id == Client.instance.myId)
            UIManager.instance.hp.text = hp.ToString();
    }

    public static void GetHit(Packet packet)
    {
        //my id
        int id = packet.ReadInt();
        bool hit = packet.ReadBool();//todo: make hit sound
        bool kill = packet.ReadBool();
        if (kill)
        {
            GameManager.players[id].kills++;
        }

        GameManager.players[id].PlayLocalShootParticles();

        Debug.Log($"shot - hit:{hit}  kill:{kill}");
    }

    public static void SetBullets(Packet packet)
    {
        int id = packet.ReadInt();
        int bullets = packet.ReadInt();
        UIManager.instance.bullets.text = bullets.ToString();
    }

    public static void SetTimeToNextShot(Packet packet)
    {
        int id = packet.ReadInt();
        float nextShot = packet.ReadFloat();
        UIManager.instance.StartBulletTimer(nextShot);
    }

    public static void LeaderBoard(Packet packet)
    {
        string leaderBoard = packet.ReadString();
        UIManager.instance.leaderBoard.text = leaderBoard;
    }





    public static void SetAnimation(Packet packet)
    {
        int id = packet.ReadInt();
        int animation = packet.ReadInt();
        if (GameManager.players.TryGetValue(id, out PlayerManager managerp))
        {
            managerp.SetAnimation(animation);
        }
    }

    public static void SetMap(Packet packet)
    {
        int map = packet.ReadInt();
        GameManager.instance.map = map;
        SceneManager.LoadSceneAsync(map, LoadSceneMode.Additive);

        UIManager.instance.ClosePage(UIManager.instance.LogInMenue);
        UIManager.instance.ClosePage(UIManager.instance.SignUpMenue);
        UIManager.instance.ClosePage(UIManager.instance.AccountMenue);
        UIManager.instance.ClosePage(UIManager.instance.MainMenue);
        UIManager.instance.ClosePage(UIManager.instance.guestJoin);
    }



}
