using Photon.Pun;
using TMPro;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;

    private string roomName;



    public void SetRoomInfo(string name, int playerCount, int maxPlayers)
    {
        roomName = name;
        roomNameText.text = name;
        playerCountText.text = $"{playerCount}/{maxPlayers}";
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}