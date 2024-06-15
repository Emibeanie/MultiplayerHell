using TMPro;
using UnityEngine;

public class PlayerUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;

    public void UpdatePlayerName(string name)
    {
        playerName.text = name;
    }

    public string GetPlayerName()
    {
        return playerName.text;
    }
}
