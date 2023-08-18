using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform cam;


    private void Update()
    {
        if ((!UIManager.instance.stopMenue.activeSelf) && (!UIManager.instance.chatInput.gameObject.activeSelf))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                ClientSend.Shoot(cam.forward);
            if (Input.GetKeyDown(KeyCode.R))
            {
                ClientSend.Reload();
            }
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }
    private void SendInputToServer()
    {
        if ((!UIManager.instance.stopMenue.activeSelf) && (!UIManager.instance.chatInput.gameObject.activeSelf))
        {
            bool[] inputs = { Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.Space) };
            ClientSend.PlayerMovement(inputs);
        }
        else
        {
            bool[] inputs = { false, false, false, false, false };
            ClientSend.PlayerMovement(inputs);
        }
    }
}
