using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CharacterSelection : MonoBehaviourPunCallbacks
{
    private const string CHOSEN_CHARACTERS_KEY = "ChosenCharacters";

    private int[] choosenCharacters = { 0, 0, 0, 0 };

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Initialize the room property when the first player joins
            UpdateChosenCharactersProperty();
        }
        else
        {
            // For non-master clients, retrieve the current state
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CHOSEN_CHARACTERS_KEY, out object chosenCharsObj))
            {
                choosenCharacters = (int[])chosenCharsObj;
            }
        }
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
            // You might want to update UI here to reflect the changes
        }
    }
}