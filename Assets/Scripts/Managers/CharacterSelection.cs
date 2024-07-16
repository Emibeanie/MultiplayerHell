using Photon.Pun;
using Photon.Realtime;
using Unity.XR.OpenVR;
using UnityEngine;

public class CharacterSelection : MonoBehaviourPunCallbacks
{
    private const string CHOSEN_CHARACTERS_KEY = "ChosenCharacters";

    private int[] choosenCharacters = { 0, 0, 0, 0 };

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Initialize room property when first player joins
            UpdateChosenCharactersProperty();
        }
        else
        {
            // non-master clients, return the current state
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CHOSEN_CHARACTERS_KEY, out object chosenCharsObj))
            {
                choosenCharacters = (int[])chosenCharsObj;
            }
        }
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        // scaleable amount of players
        /*  choosenCharacters = new int[PhotonNetwork.CurrentRoom.MaxPlayers];*/
    }

    public void PlayerSelectCharacter(int index)
    {
        if (index < 1 || index > choosenCharacters.Length)
        {
            Debug.LogError("Invalid character index");
            return;
        }

        int arrayIndex = index - 1;
        if (choosenCharacters[arrayIndex] == 0)
        {
            choosenCharacters[arrayIndex] = index;
            UpdateChosenCharactersProperty();
            NetworkManager.Instance.SpawnNewPlayer();
        }
        else
        {
            UIManager.Instance.DisplayMassage();
            Debug.Log("Character has already been taken");
        }
    }

    private void UpdateChosenCharactersProperty()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { CHOSEN_CHARACTERS_KEY, choosenCharacters }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue(CHOSEN_CHARACTERS_KEY, out object chosenCharsObj))
        {
            choosenCharacters = (int[])chosenCharsObj;
            // update UI here
        }
    }
}