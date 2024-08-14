using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkDebugMenu : MonoBehaviour
{
    NetworkManager NetworkManager;
    bool inMainMenu = true;
    int xPos;

    void Awake()
    {
        NetworkManager = GetComponent<NetworkManager>();
        xPos = Screen.width / 2;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Host"))
        {
            NetworkManager.StartHost();
            inMainMenu = false;
        }

        if (GUILayout.Button("Join"))
        {
            NetworkManager.StartClient();
            inMainMenu = false;
        }

        if (inMainMenu)
        {
            GUI.Label(new Rect(xPos, 40, 400, 1000),
                "KEYBINDINGS\n" +
                "===========\n" +
                "WASD and IJKL:" + '\n' +
                "W - Move forward" + '\n' +
                "S - Move back" + '\n' +
                "A - Strafe left" + '\n' +
                "D - Strafe right" + '\n' +
                "E - Move up" + '\n' +
                "Q - Move down" + '\n' +
                "\n" +
                "I - Tilt forward" + '\n' +
                "K - Tilt back" + '\n' +
                "J - Turn left" + '\n' +
                "L - Turn right" + '\n' +
                "U - Roll left" + '\n' +
                "O - Roll right" + '\n' +
                "\n" +
                "Actions:" +'\n'+
                "Space - Shoot" + '\n' +
                "V - Open chat menu\n"
            );
        }
    }
}
