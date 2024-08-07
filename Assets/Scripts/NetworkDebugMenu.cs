using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkDebugMenu : MonoBehaviour
{
    NetworkManager NetworkManager;
    void Awake()
    {
        NetworkManager = GetComponent<NetworkManager>();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Host")) NetworkManager.StartHost();
        if (GUILayout.Button("Join")) NetworkManager.StartClient();
    }

}
