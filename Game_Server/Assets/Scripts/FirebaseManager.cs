using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Net;

public class FirebaseManager : MonoBehaviour
{


    public static FirebaseAuth auth;
    public static DatabaseReference database;
    public static DependencyStatus dependencyStatus;
    public static string ip = "";


    public void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                StartFireBase();
            }
            else
            {
                Debug.Log("cant connect to firebase");
            }

        }

        );
    }

    private void StartFireBase()
    {
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void OnApplicationQuit()
    {
        database.Child("game_servers").Child(ip.Replace('.', ' ')).RemoveValueAsync();
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static IEnumerator AddGame(string name, int port)
    {
        try
        {
            ip = new WebClient().DownloadString("http://ifconfig.me");
            ip = ip.Replace("\n", "");
        }
        catch
        {
            try
            {

                ip = new WebClient().DownloadString("http://ipv4.icanhazip.com");
                ip = ip.Replace("\n", "");
            }
            catch
            {
                ip = "";
            }
        }

        yield return new WaitUntil(predicate: () => database!=null);


        Debug.Log($"ip:{ip}");
        var a1 = database.Child("game_servers").Child(ip.Replace('.', ' ')).Child("ip").SetValueAsync(ip);
        var a2 = database.Child("game_servers").Child(ip.Replace('.', ' ')).Child("name").SetValueAsync(name);
        var a3 = database.Child("game_servers").Child(ip.Replace('.', ' ')).Child("port").SetValueAsync(port);
        var a4 = database.Child("game_servers").Child(ip.Replace('.', ' ')).Child("connected").SetValueAsync(0);
        var a5 = database.Child("game_servers").Child(ip.Replace('.', ' ')).Child("map").SetValueAsync(NetworkManager.instance.MAP);
        yield return new WaitUntil(predicate: () =>  a2.IsCompleted && a3.IsCompleted && a4.IsCompleted);
        if (a1.Exception != null || a1.Exception != null || a1.Exception != null || a1.Exception != null)
            Debug.Log($"can't send data to firebase: {a1.Exception.GetBaseException()}");

        UpdateConnected();


    }

    public static void UpdateConnected()
    {
        int totalClients = 0;
        foreach(Client c in Server.clients)
        {
            if (c != null && c.tcp != null && c.tcp.socket != null)
            {
                totalClients++;
            }
        }

        //Debug.Log($"clients connected: {totalClients}");
        try
        {
            ip = new WebClient().DownloadString("http://ifconfig.me");
            ip = ip.Replace("\n", "");
        }
        catch
        {
            try
            {

                ip = new WebClient().DownloadString("http://ipv4.icanhazip.com");
                ip = ip.Replace("\n", "");
            }
            catch
            {
                ip = "";
            }
        }

        database.Child("game_servers").Child(ip.Replace('.', ' ')).Child("connected").SetValueAsync(totalClients);
    }

}
