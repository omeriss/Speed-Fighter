using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{

    public float lookSensitivity = 200f;
    public Transform Camera;
    float UpDownRot = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;
        UpDownRot -= mouseY;
        UpDownRot = Mathf.Clamp(UpDownRot, -90, 90);

        Camera.localRotation = Quaternion.Euler(UpDownRot, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}
