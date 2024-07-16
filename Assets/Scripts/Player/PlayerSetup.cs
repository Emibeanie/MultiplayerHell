using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] PlayerMovment playerMovment;
    [SerializeField] GameObject playerCamera;

    private void Start()
    {
        IsLocalPlayer();
    }

    public void IsLocalPlayer()
    {
        playerMovment.enabled = true;
        playerCamera.SetActive(true);
    }

}
