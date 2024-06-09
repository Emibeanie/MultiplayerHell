using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Enter Lobby UI")]
    [SerializeField] TextMeshProUGUI connectingMassage;
    [SerializeField] GameObject enterLobbyPanel;
    [SerializeField] TMP_InputField lobbyNameField;

    [Header("Room List UI")]
    [SerializeField] GameObject roomListPanel;
    [SerializeField] Transform roomListParentObject;
    [SerializeField] GameObject roomPrefab;

    [Header("Create Room UI")]
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] TMP_InputField roomName;
    [SerializeField] TMP_InputField maxPlayers;

    [Header("In Room UI")]
    [SerializeField] GameObject inRoomPanel;
    [SerializeField] TextMeshProUGUI inRoomMassage;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    private string currentLobbyName;
    private bool isInitialConnection = true;



    void Start()
    {
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = "Connecting to the main server...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master Server");

        if (isInitialConnection)
        {
            connectingMassage.gameObject.SetActive(false);
            enterLobbyPanel.SetActive(true);
            isInitialConnection = false;
        }
        else if (!string.IsNullOrEmpty(currentLobbyName))
        {
            Debug.Log($"Reconnected to Master. Rejoining lobby: {currentLobbyName}");
            TypedLobby lobby = new TypedLobby(currentLobbyName, LobbyType.Default);
            PhotonNetwork.JoinLobby(lobby);
        }
    }

    public void CreateAndJoinLobbyByName()
    {
        currentLobbyName = lobbyNameField.text;
        TypedLobby lobby = new TypedLobby(currentLobbyName, LobbyType.Default);
        PhotonNetwork.JoinLobby(lobby);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log($"Your are now connected to {currentLobbyName}");

        enterLobbyPanel.SetActive(false);
        roomListPanel.SetActive(true);
        createRoomPanel.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        if (cachedRoomList.Count <= 0)
        {
            cachedRoomList = roomList;
        }
        else
        {
            foreach (RoomInfo room in roomList)
            {
                for (int i = 0; i < cachedRoomList.Count; i++)
                {
                    if (cachedRoomList[i].Name == room.Name)
                    {
                        List<RoomInfo> newList = cachedRoomList;

                        if (room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);
                        }
                        else
                        {
                            newList[i] = room;
                        }

                        cachedRoomList = newList;
                    }
                }
            }

        }
        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (Transform roomItem in roomListParentObject)
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
        {
            GameObject roomGO = Instantiate(roomPrefab, roomListParentObject);

            RoomItem roomItem = roomGO.GetComponent<RoomItem>(); //Find how to not use getcomponent

            roomItem?.SetRoomInfo(room.Name, room.PlayerCount, room.MaxPlayers);
        }
    }

    public void CreateRoomInCurrentLobby()
    {
        string roomNameStr = roomName.text;
        int maxPlayerInt = int.Parse(maxPlayers.text);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayerInt;
        roomOptions.IsVisible = true;

        TypedLobby currentLobby = new TypedLobby(currentLobbyName, LobbyType.Default);

        PhotonNetwork.CreateRoom(roomNameStr, roomOptions, currentLobby);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log($"Room '{PhotonNetwork.CurrentRoom.Name}' created in lobby '{currentLobbyName}'.");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        createRoomPanel.SetActive(false);
        roomListPanel.SetActive(false);

        inRoomPanel.SetActive(true);
        inRoomMassage.text = $"You joined room {PhotonNetwork.CurrentRoom.Name} in {currentLobbyName}";
    }

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log($"Left room. Returning to lobby: {currentLobbyName}");

        inRoomPanel.SetActive(false);
        createRoomPanel.SetActive(true);
        roomListPanel.SetActive(true);

        roomName.text = "";
        maxPlayers.text = "";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogWarning($"Disconnected from Photon servers. Cause: {cause}");

        inRoomPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomListPanel.SetActive(false);
        enterLobbyPanel.SetActive(false);
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = $"Disconnected: {cause}. Reconnecting...";

        isInitialConnection = true;

        PhotonNetwork.ConnectUsingSettings();
    }

}
