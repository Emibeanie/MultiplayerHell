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
    [SerializeField] RoomItem roomPrefab;

    [Header("Create Room UI")]
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] TMP_InputField roomName;
    [SerializeField] TMP_InputField maxPlayers;

    [Header("In Room UI")]
    [SerializeField] GameObject inRoomPanel;
    [SerializeField] TextMeshProUGUI inRoomMassage;
    [SerializeField] VerticalLayoutGroup playerListParent;
    [SerializeField] PlayerUIItem playerPrefab;

    [Header("Errors")]
    [SerializeField] TextMeshProUGUI errorMessageText;

    private List<PlayerUIItem> currentInstantiatedPlayerList;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private string currentLobbyName;
    private bool isInitialConnection = true;

    void Start()
    {
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = "Connecting to the main server...";
        PhotonNetwork.ConnectUsingSettings();

        currentInstantiatedPlayerList = new List<PlayerUIItem>();

        ResetErrorMessageText();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

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

        ResetErrorMessageText();
    }

    public void CreateAndJoinLobbyByName()
    {
        if (lobbyNameField.text == string.Empty)
        {
            ChangeErrorMessageText("Lobby name must contain at least one character");
            return;
        }

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

        cachedRoomList.Clear();
        ResetErrorMessageText();
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();

        cachedRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        Debug.Log($"cached rooms: {cachedRoomList.Count} room list: {roomList.Count}");

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        UpdateLobbyRoomListUI();
    }


    public void CreateRoomInCurrentLobby()
    {
        if (roomName.text == string.Empty)
        {
            ChangeErrorMessageText("Room name must contain at least one character");
            return;
        }

        if (maxPlayers.text == string.Empty || !int.TryParse(maxPlayers.text,out int maxPlayerInt) || maxPlayerInt <= 1)
        {
            ChangeErrorMessageText("Room max players must be filled and contain only a number bigger than 1");
            return;
        }

        string roomNameStr = roomName.text;
       // int maxPlayerInt = int.Parse(maxPlayers.text);

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

        ResetErrorMessageText();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        ChangeErrorMessageText(message);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        createRoomPanel.SetActive(false);
        roomListPanel.SetActive(false);

        inRoomPanel.SetActive(true);
        inRoomMassage.text = $"You joined room '{PhotonNetwork.CurrentRoom.Name}' in lobby '{currentLobbyName}'";
        InitPlayerList();

        ResetErrorMessageText();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        ChangeErrorMessageText(message);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        CreateNewPlayerUI(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RemovePlayerUI(otherPlayer);
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

        cachedRoomList.Clear();
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
    private void ResetErrorMessageText()
    {
        errorMessageText.text = "";
    }
    private void ChangeErrorMessageText(string text)
    {
        errorMessageText.text = text;
    }

    private void UpdateLobbyRoomListUI()
    {
        foreach (Transform roomItem in roomListParentObject)
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
        {
            RoomItem roomItem = Instantiate(roomPrefab, roomListParentObject);

            roomItem.SetRoomInfo(room.Value.Name, room.Value.PlayerCount, room.Value.MaxPlayers);
        }
    }
    private void CreateNewPlayerUI(Player player)
    {
        var newPlayer = Instantiate(playerPrefab, playerListParent.transform);
        currentInstantiatedPlayerList.Add(newPlayer);
        newPlayer.UpdatePlayerName(player.NickName);
    } 
    private void RemovePlayerUI(Player player)
    {
        var foundPlayerObject = currentInstantiatedPlayerList.Find(playerObject => playerObject.GetPlayerName() == player.NickName);
        if(foundPlayerObject == null)
        {
            return;
        }
        currentInstantiatedPlayerList.Remove(foundPlayerObject);
        Destroy(foundPlayerObject.gameObject);
    }

    private void InitPlayerList()
    {
        ClearRoomPlayerList();

        var playersInRoom = PhotonNetwork.CurrentRoom.Players;

        foreach (var player in playersInRoom)
        {
            CreateNewPlayerUI(player.Value);
        }
    }

    private void ClearRoomPlayerList()
    {
        foreach (var player in currentInstantiatedPlayerList)
            Destroy(player.gameObject);

        currentInstantiatedPlayerList.Clear();
    }
}
