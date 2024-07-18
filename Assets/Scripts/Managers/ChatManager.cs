using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class ChatManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Dropdown colorSelect;
    [SerializeField] TMP_InputField chatBox;
    [SerializeField] GameObject chatPanel;
    [SerializeField] GameObject textObject;

    private int maxMessage = 25;
    private List<Message> messageList = new List<Message>();
    private Color selectedColor = Color.white;

    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string playerName = PhotonNetwork.NickName;
                SendMessageToChat(playerName + ": "+ chatBox.text);
                chatBox.text = "";
            }
        }
        else
        {
            if(!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }
        }

    }

    public void SendMessageToChat(string text)
    {
        photonView.RPC("RPC_SendMessageToChat", RpcTarget.All, text, selectedColor.r, selectedColor.g, selectedColor.b);
    }

    [PunRPC]
    public void RPC_SendMessageToChat(string text, float r, float g, float b)
    {
        if (messageList.Count >= maxMessage)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        Message newMessage = new Message();
        newMessage.text = text;
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TextMeshProUGUI>();
        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = new Color(r, g, b);
        messageList.Add(newMessage);
    }

    public void ChatColorSelect()
    {
        switch (colorSelect.value)
        {
            case 0: //white
                selectedColor = Color.white;
                break;
            case 1: //red
                selectedColor = Color.red;
                break;
            case 2: //blue
                selectedColor = Color.blue;
                break;
            case 3: //green
                selectedColor = Color.green;
                break;
            case 4: //purple
                selectedColor = new Color(0.5f, 0f, 0.5f);
                break;
            case 5: //yellow
                selectedColor = Color.yellow;
                break;
            default:
                selectedColor = Color.white;
                break;
        }
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public TextMeshProUGUI textObject;
}