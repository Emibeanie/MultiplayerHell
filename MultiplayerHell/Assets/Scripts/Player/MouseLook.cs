using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] float moueHorizontalSpeed;
    [SerializeField] float mouseVerticalSpeed;
    [SerializeField] Transform playerBody;

    private float xRotation;

    private float swordRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        AimMovment();
    }

    private void AimMovment()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply the horizontal rotation directly to the transform
        Vector3 horizontalRotation = Vector3.up * mouseX * moueHorizontalSpeed;

        // Accumulate vertical rotation separately and clamp it
        xRotation -= mouseY * mouseVerticalSpeed;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        // Apply clamped vertical rotation and unclamped horizontal rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(horizontalRotation);
    }
}
