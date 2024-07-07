using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [SerializeField] Camera cameraUI;

    private string playerPrefabPath = "FPSPlayer";
    private string Level1PrefabPath = "Level1";

    UIManager _uiManagerInstance;

    private List<PlayerUIItem> _currentInstantiatedPlayerList = new();
    private Dictionary<string, RoomInfo> _cachedRoomList = new();
    private string _currentLobbyName;
    private bool _isInitialConnection = true;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _uiManagerInstance = UIManager.Instance;

        _uiManagerInstance.DisplayStartMassage();
        PhotonNetwork.ConnectUsingSettings();
        _uiManagerInstance.ResetErrorMessageText();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        if (_isInitialConnection)
        {
            _uiManagerInstance.DisplayLobbyPanel();
            _isInitialConnection = false;
        }
        else if (!string.IsNullOrEmpty(_currentLobbyName))
        {
            Debug.Log($"Reconnected to Master. Rejoining lobby: {_currentLobbyName}");
            TypedLobby lobby = new TypedLobby(_currentLobbyName, LobbyType.Default);
            PhotonNetwork.JoinLobby(lobby);
        }

        _uiManagerInstance.ResetErrorMessageText();
    }

    public void CreateAndJoinLobbyByName()
    {
        if (_uiManagerInstance.GetPlayerNickNameTextField() == string.Empty || _uiManagerInstance.GetPlayerNickNameTextField().Length < 3)
        {
            _uiManagerInstance.ChangeErrorMessageText("Player Nickname must contain at least 3 characters!");
            return;
        }
        
        PhotonNetwork.LocalPlayer.NickName = _uiManagerInstance.GetPlayerNickNameTextField();
        
        if (_uiManagerInstance.GetLobbyTextField() == string.Empty)
        {
            _uiManagerInstance.ChangeErrorMessageText("Lobby name must contain at least 1 character");
            return;
        }

        _currentLobbyName = _uiManagerInstance.GetLobbyTextField();
        TypedLobby lobby = new TypedLobby(_currentLobbyName, LobbyType.Default);
        PhotonNetwork.JoinLobby(lobby);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        _uiManagerInstance.DisplayRoomList();

        _cachedRoomList.Clear();
        _uiManagerInstance.ResetErrorMessageText();
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

        _uiManagerInstance.UpdateLobbyRoomListUI(_cachedRoomList);
    }


    public void CreateRoomInCurrentLobby()
    {
        if (_uiManagerInstance.GetRoomNameTextField() == string.Empty)
        {
            _uiManagerInstance.ChangeErrorMessageText("Room name must contain at least one character");
            return;
        }

        if (_uiManagerInstance.GetMaxPlayerTextField() == string.Empty || !int.TryParse(_uiManagerInstance.GetMaxPlayerTextField(), out int maxPlayerInt) || maxPlayerInt <= 1)
        {
            _uiManagerInstance.ChangeErrorMessageText("Room max players must be filled and contain only a number bigger than 1");
            return;
        }
        string roomNameStr = _uiManagerInstance.GetRoomNameTextField();
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

        _uiManagerInstance.ResetErrorMessageText();
        SpawnMap();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        _uiManagerInstance.ChangeErrorMessageText(message);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        _uiManagerInstance.DisplayCharacterSelection();
        InitPlayerList();

        _uiManagerInstance.ResetErrorMessageText();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        _uiManagerInstance.ChangeErrorMessageText(message);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        _uiManagerInstance.CreateNewPlayerUI(newPlayer, _currentInstantiatedPlayerList);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RemovePlayerUI(otherPlayer);
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        _uiManagerInstance.DisplayExitRoom();

        _cachedRoomList.Clear();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        _uiManagerInstance.DisplayDisconnecting(cause);

        _isInitialConnection = true;

        PhotonNetwork.ConnectUsingSettings();
    }
  
    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
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
            _uiManagerInstance.CreateNewPlayerUI(player.Value, _currentInstantiatedPlayerList);
        }
    }

    public void SpawnNewPlayer()
    {
        _uiManagerInstance.DisplayGamplay();
        var player = PhotonNetwork.Instantiate(playerPrefabPath, new Vector3(0, 1, 0), Quaternion.identity);
        player.GetComponent<PlayerSetup>()?.IsLocalPlayer();
        cameraUI.gameObject.SetActive(false);
    }

    public void SpawnMap()
    {
        PhotonNetwork.InstantiateRoomObject(Level1PrefabPath, Vector3.zero, Quaternion.identity);
    }

    private void ClearRoomPlayerList()
    {
        foreach (var player in _currentInstantiatedPlayerList)
            Destroy(player.gameObject);

        _currentInstantiatedPlayerList.Clear();
    }
}
