using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MessageManager : NetworkBehaviour
{
    enum MessageType : byte
    {
        Hello,
        Shazbot,
        GG,
        Woohoo,
        IAmTheGreatest,
        NiceShot
    }

    IAC_Default inputActions;

    private void Start()
    {
        if (IsLocalPlayer)
        {
            inputActions = new IAC_Default();
            inputActions.Gameplay.Enable();
            inputActions.Chat.Disable();

            inputActions.Gameplay.Chat.performed += ctx => SetChatMode(true);

            inputActions.Chat.H.performed += ctx => AddMessageRequestRPC(MessageType.Hello);
            inputActions.Chat.S.performed += ctx => AddMessageRequestRPC(MessageType.Shazbot);
            inputActions.Chat.W.performed += ctx => AddMessageRequestRPC(MessageType.Woohoo);
            inputActions.Chat.I.performed += ctx => AddMessageRequestRPC(MessageType.IAmTheGreatest);
            inputActions.Chat.N.performed += ctx => AddMessageRequestRPC(MessageType.NiceShot);
            inputActions.Chat.G.performed += ctx => AddMessageRequestRPC(MessageType.GG);
            inputActions.Chat.Quit.performed += ctx => SetChatMode(false);
        }

        chatYPos = Screen.height / 2;
    }

    int chatYPos;
    string chatTextToShow;
    bool isChatMode = false;
    private void OnGUI()
    {
        GUI.Label(new Rect(10, chatYPos, 200, 500), chatTextToShow);

        if (isChatMode)
        {
            GUI.Label(new Rect(250, chatYPos, 200, 500),
                "G - GG\n" +
                "H - Hello!\n" +
                "I - I am the greatest!\n" +
                "N - Nice Shot!\n" +
                "S - Shazbot!\n"+
                "W - Woohoo!\n"+
                "Esc or V - <quit chat>");
        }
    }

    void SetChatMode(bool enterChatMode)
    {
        if (enterChatMode)
        {
            inputActions.Gameplay.Disable();
            inputActions.Chat.Enable();
            isChatMode = true;
        }
        else
        {
            inputActions.Gameplay.Enable();
            inputActions.Chat.Disable();
            isChatMode = false;
        }
    }

    float timer;
    private void Update()
    {
       if (IsServer && recentMessagesCount > 0)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = 1;
                --recentMessagesCount;
            }
        }
    }

    byte recentMessagesCount = 0;
    [Rpc(SendTo.Server)]
    void AddMessageRequestRPC(MessageType messageType)
    {
        SetChatMode(false);

        if (IsServer)
        {
            // Simple example of a spam filter as server authority
            if (recentMessagesCount < 5)
            {
                ++recentMessagesCount;
                SendMessageToClientsRPC(messageType);
            } 
            else
            {
                Debug.Log("Too many messages. Please wait...");
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SendMessageToClientsRPC(MessageType msg)
    {
        switch (msg)
        {
            case MessageType.Hello:
            {
                AddMessageToTextBox("Hello!");
                break;
            }
            case MessageType.Shazbot:
            {
                AddMessageToTextBox("Shazbot!");
                break;
            }
            case MessageType.Woohoo:
            {
                AddMessageToTextBox("Woohoo!");
                break;
            }
            case MessageType.IAmTheGreatest:
            {
                AddMessageToTextBox("I am the greatest!");
                break;
            }
            case MessageType.NiceShot:
            {
                AddMessageToTextBox("Nice shot!");
                break;
            }
            case MessageType.GG:
            {
                AddMessageToTextBox("GG");
                break;
            }
        }
    }

    const int maxMessages = 10;
    Queue chatMessages = new Queue(maxMessages);
    void AddMessageToTextBox(string msg) // Placeholder chat box function
    {
        if (chatMessages.Count == maxMessages)
        {
            chatMessages.Dequeue();
        }

        chatMessages.Enqueue("Player " + OwnerClientId + ": " + msg);

        chatTextToShow = "";
        foreach (var m in chatMessages)
        {
            chatTextToShow += m + "\n";
        }
    }
}
