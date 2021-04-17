using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private float camSensitivity;
    [SerializeField]
    private float cameraRotationLimit;

    private Rigidbody playerRigid;
    private float currentCameraRotationX = 0f;
    private float currentCameraRotationY = 0f;

    void Start()
    {
        cam = FindObjectOfType<Camera>();
        player = FindObjectOfType<PlayerController>();
    }
    
    void Update()
    {
        CameraRotation();
        CharacterRotation();
    }

    private void CharacterRotation()
    {
        float yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 characterRotationY = new Vector3(0f, yRotation, 0f) * camSensitivity;
        playerRigid = player.getPlyaerRigid();        
        playerRigid.MoveRotation(playerRigid.rotation * Quaternion.Euler(characterRotationY));
    }

    private void CameraRotation()
    {
        float xRotation = Input.GetAxisRaw("Mouse Y");
        float cameraRotationX = xRotation * camSensitivity;
        currentCameraRotationX -= cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);       
    }

}
