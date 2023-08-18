using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class AccountManager : MonoBehaviour
{


    public static FirebaseAuth auth;
    public static FirebaseUser user;
    public static DatabaseReference database;
    public static DependencyStatus dependencyStatus;


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
                UIManager.instance.CantConnectMenue.SetActive(true);
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

    public void TryToReconnect()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                UIManager.instance.CantConnectMenue.SetActive(false);
                StartFireBase();

            }
            else
            {
                Debug.Log("cant connect to firebase");
            }

        }
        );
    }


    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }


}
