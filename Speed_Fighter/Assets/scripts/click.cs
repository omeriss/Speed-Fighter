using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class click : MonoBehaviour
{
    public int port = 0;
    public string ip = "";
    public Text Connected;
    public Text Name;
    public int map = 1;

    public void connect()
    {
        GameManager.instance.map = map;
        SceneManager.LoadSceneAsync(map, LoadSceneMode.Additive);
        Client.instance.ConnectToServer(ip, port);
    }

}
