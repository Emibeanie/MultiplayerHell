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
    [SerializeField] TMP_InputField playerNicknameField;
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

    private List<PlayerUIItem> _currentInstantiatedPlayerList = new();
    private Dictionary<string, RoomInfo> _cachedRoomList = new();
    private string _currentLobbyName;
    private bool _isInitialConnection = true;

    void Start()
    {
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = "Connecting to the main server...";
        PhotonNetwork.ConnectUsingSettings();
        
        ResetErrorMessageText();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        if (_isInitialConnection)
        {
            connectingMassage.gameObject.SetActive(false);
            enterLobbyPanel.SetActive(true);
            _isInitialConnection = false;
        }
        else if (!string.IsNullOrEmpty(_currentLobbyName))
        {
            Debug.Log($"Reconnected to Master. Rejoining lobby: {_currentLobbyName}");
            TypedLobby lobby = new TypedLobby(_currentLobbyName, LobbyType.Default);
            PhotonNetwork.JoinLobby(lobby);
        }

        ResetErrorMessageText();
    }

    public void CreateAndJoinLobbyByName()
    {
        if (playerNicknameField.text == string.Empty || playerNicknameField.text.Length < 3)
        {
            ChangeErrorMessageText("Player Nickname must contain at least 3 characters!");
            return;
        }
        
        PhotonNetwork.LocalPlayer.NickName = playerNicknameField.text;
        
        if (lobbyNameField.text == string.Empty)
        {
            ChangeErrorMessageText("Lobby name must contain at least 1 character");
            return;
        }

        _currentLobbyName = lobbyNameField.text;
        TypedLobby lobby = new TypedLobby(_currentLobbyName, LobbyType.Default);
        PhotonNetwork.JoinLobby(lobby);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        enterLobbyPanel.SetActive(false);
        roomListPanel.SetActive(true);
        createRoomPanel.SetActive(true);

        _cachedRoomList.Clear();
        ResetErrorMessageText();
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();

        _cachedRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        
        foreach (var info in roomList)
        {
            if (info.RemovedFromList)
            {
                _cachedRoomList.Remove(info.Name);
            }
            else
            {
                _cachedRoomList[info.Name] = info;
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

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayerInt,
            IsVisible = true
        };

        TypedLobby currentLobby = new TypedLobby(_currentLobbyName, LobbyType.Default);

        PhotonNetwork.CreateRoom(roomNameStr, roomOptions, currentLobby);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        
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
        inRoomMassage.text = $"Room '{PhotonNetwork.CurrentRoom.Name}' | Lobby '{_currentLobbyName}'";
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

        inRoomPanel.SetActive(false);
        createRoomPanel.SetActive(true);
        roomListPanel.SetActive(true);

        roomName.text = "";
        maxPlayers.text = "";

        _cachedRoomList.Clear();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        inRoomPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomListPanel.SetActive(false);
        enterLobbyPanel.SetActive(false);
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = $"Disconnected: {cause}. Reconnecting...";

        _isInitialConnection = true;

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

        foreach (var room in _cachedRoomList)
        {
            RoomItem roomItem = Instantiate(roomPrefab, roomListParentObject);

            roomItem.SetRoomInfo(room.Value.Name, room.Value.PlayerCount, room.Value.MaxPlayers);
        }
    }
    private void CreateNewPlayerUI(Player player)
    {
        var newPlayer = Instantiate(playerPrefab, playerListParent.transform);
        _currentInstantiatedPlayerList.Add(newPlayer);
        newPlayer.UpdatePlayerName(player.NickName);
    } 
    private void RemovePlayerUI(Player player)
    {
        var foundPlayerObject = _currentInstantiatedPlayerList.Find(playerObject => playerObject.GetPlayerName() == player.NickName);
        if(foundPlayerObject == null)
        {
            return;
        }
        _currentInstantiatedPlayerList.Remove(foundPlayerObject);
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
        foreach (var player in _currentInstantiatedPlayerList)
            Destroy(player.gameObject);

        _currentInstantiatedPlayerList.Clear();
    }
}
