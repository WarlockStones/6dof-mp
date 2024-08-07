using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    void Start()
    {
        if (!IsLocalPlayer)
        {
            GetComponent<Camera>().enabled = false;
        }
    }
}
