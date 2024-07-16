using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerSetup : MonoBehaviourPun
    {
        [FormerlySerializedAs("playerMovment")] [SerializeField] PlayerMovment playerMovement;
        [SerializeField] GameObject playerCamera;

        private void Start()
        {
            if (photonView.IsMine)
            {
                TurnOnLocalPlayer();
            }
        }

        private void TurnOnLocalPlayer()
        {
            playerMovement.enabled = true;
            playerCamera.SetActive(true);
        }

    }
}
