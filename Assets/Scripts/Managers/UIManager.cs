using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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

    [Header("Character Selction UI")]
    [SerializeField] GameObject characterSelectionPanel;
    [SerializeField] TextMeshProUGUI characterSelectionMassage;

    [Header("Errors")]
    [SerializeField] TextMeshProUGUI errorMessageText;

    private void Awake()
    {
        Instance = this;
    }

    public void DisplayStartMassage()
    {
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = "Connecting to the main server...";
    }

    public void DisplayLobbyPanel()
    {
        connectingMassage.gameObject.SetActive(false);
        enterLobbyPanel.SetActive(true);
    }

    public void DisplayRoomList()
    {
        enterLobbyPanel.SetActive(false);
        roomListPanel.SetActive(true);
        createRoomPanel.SetActive(true);
    }

    public void DisplayInRoomPanel()
    {
        createRoomPanel.SetActive(false);
        roomListPanel.SetActive(false);

        inRoomPanel.SetActive(true);
        inRoomMassage.text = $"Room '{PhotonNetwork.CurrentRoom.Name}' | Lobby '{PhotonNetwork.CurrentLobby.Name}'";
    }

    public void DisplayExitRoom()
    {
        inRoomPanel.SetActive(false);
        createRoomPanel.SetActive(true);
        roomListPanel.SetActive(true);

        roomName.text = "";
        maxPlayers.text = "";
    }
    public void DisplayCharacterSelection()
    {
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);
        characterSelectionMassage.gameObject.SetActive(false);
    }
    public void DisplayGamplay()
    {
        characterSelectionPanel.SetActive(false);
    }
    public void DisplayMassage()
    {
        characterSelectionMassage.gameObject.SetActive(true);
    }

    public void DisplayDisconnecting(DisconnectCause cause)
    {
        inRoomPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomListPanel.SetActive(false);
        enterLobbyPanel.SetActive(false);
        connectingMassage.gameObject.SetActive(true);
        connectingMassage.text = $"Disconnected: {cause}. Reconnecting...";
    }

    public void ResetErrorMessageText()
    {
        errorMessageText.text = "";
    }
    public void ChangeErrorMessageText(string text)
    {
        errorMessageText.text = text;
    }

    public void UpdateLobbyRoomListUI(Dictionary<string, RoomInfo> cachedRoomList)
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
    public void CreateNewPlayerUI(Photon.Realtime.Player player, List<PlayerUIItem> currentInstantiatedPlayerList)
    {
        var newPlayer = Instantiate(playerPrefab, playerListParent.transform);
        currentInstantiatedPlayerList.Add(newPlayer);
        newPlayer.UpdatePlayerName(player.NickName);
    }

    public string GetLobbyTextField()
    {
        return lobbyNameField.text;
    }

    public string GetPlayerNickNameTextField()
    {
        return playerNicknameField.text;
    }

    public string GetRoomNameTextField()
    {
        return roomName.text;
    }

    public string GetMaxPlayerTextField()
    {
        return maxPlayers.text;
    }
}
