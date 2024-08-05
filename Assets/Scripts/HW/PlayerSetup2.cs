using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] PlayerMovment2 playerMovment;

    public void IsPlayerOwner()
    {
        playerMovment.enabled = true;
    }
}
