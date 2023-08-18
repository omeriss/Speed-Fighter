using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System;



public enum ServerPackets
{
    ping = 1, spawnPlayer = 2, playerPosition, PlayerRotation, ChatMassage, Disconnect, state, hit, bullets, nextShot, leaderBoard, animation, sendMap
}



public enum ClientPackets
{
    pingBack = 1, playerMovement = 2, shoot, reload, askLeaderBoard, askMap, sendChatMassage
}


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

    public const int TickRate = 60;

    private int port = 16016;

    public int MAP = 2;

    private int MAXPLAYERS = 16;

    private string serverName  = "server";

    private string settingsFile = @"\Server_Settings.txt";

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


    private void ReadDataFromFile()
    {
        settingsFile = Directory.GetCurrentDirectory() + settingsFile;

        if (File.Exists(settingsFile))
        {
            List<string> fileLines = File.ReadAllLines(settingsFile).ToList();


            foreach (string line in fileLines)
            {
                string[] values = line.Split('=');
                if (values.Length == 2)
                {
                    values[0] = values[0].Replace(" ", "");
                    values[1] = values[1].Replace(" ", "");


                    if (values[0] == "max_players")
                    {
                        try
                        {
                            int temp = int.Parse(values[1]);
                            MAXPLAYERS = temp;
                        }
                        catch
                        {
                            Debug.Log("worng syntax in settings file");
                        }
                    }

                    if (values[0] == "map")
                    {
                        try
                        {
                            int temp = int.Parse(values[1]);
                            MAP = temp;
                        }
                        catch
                        {
                            Debug.Log("worng syntax in settings file");
                        }
                    }

                    if (values[0] == "server_name")
                    {
                        serverName = values[1];
                    }
                }
            }
        }
        else
        {
            try
            {

                List<string> fileLines = new List<string>();

                fileLines.Add("max_players=16");
                fileLines.Add("map=1");
                fileLines.Add("server_name=server");
                MAP = 1;

                File.WriteAllLines(settingsFile, fileLines.ToArray());

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        }
    }

    private void Start()
    {

        ReadDataFromFile();
        


        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;


        SceneManager.LoadSceneAsync(MAP, LoadSceneMode.Additive);



        Server.Start(MAXPLAYERS, port);

        StartCoroutine(FirebaseManager.AddGame(serverName, port));
        Debug.Log("port:"+port);
    }

    private void OnApplicationQuit()
    {
        if (Server.tcpListener != null && Server.udpLisener != null)
        {
            Server.tcpListener.Stop();
            Server.udpLisener.Close();
            Debug.Log("server stopped");
        }
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
