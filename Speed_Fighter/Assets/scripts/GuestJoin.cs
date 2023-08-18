using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuestJoin : MonoBehaviour
{


    public InputField ip;
    public InputField port;
    public static bool IsGuest = false;


    public void Join()
    {
        IsGuest = true;
        Client.instance.ConnectToServer(ip.text,  int.Parse(port.text));
    }
}
