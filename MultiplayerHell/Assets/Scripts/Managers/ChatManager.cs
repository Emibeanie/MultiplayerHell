using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public int maxMessage = 25;
    
    [SerializeField]
    List<Message> messageList = new List<Message>();

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToChat("you pressed the space bar");
            Debug.Log("Space");
        }
    }

    public void SendMessageToChat(string text)
    {
        if (messageList.Count >= maxMessage)
            messageList.Remove(messageList[0]);

        Message newMessage = new Message();
        newMessage.text = text;
        messageList.Add(newMessage);
    }
}
[System.Serializable]
public class Message
{
    public string text;
}