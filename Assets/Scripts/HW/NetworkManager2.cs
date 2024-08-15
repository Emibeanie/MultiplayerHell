using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager2 : MonoBehaviourPunCallbacks
{
    // currently a room open imediatly after entring a lobby, if there is a room is already created then we just join it.
    // Later we will add a room list, create room panel and more...
    public void OnEnteringLobby()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnCreatingRoom()
    {

    }

    public void OnQuitGame()
    {
        Application.Quit();
    }
  
    public override void OnConnectedToMaster()
    {
        Debug.Log("You are connected to the master server");

        PhotonNetwork.AutomaticallySyncScene = true;

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)6,
            IsVisible = true,
            IsOpen = true,
            PlayerTtl = 30,
        };

        TypedLobby typedLobby = new TypedLobby("Main", LobbyType.Default);

        PhotonNetwork.JoinOrCreateRoom("Simple_Test", roomOptions, typedLobby);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("A new room is been created");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }


}
