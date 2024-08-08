using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    void Start()
    {
        if (!IsLocalPlayer)
        {
            Debug.Log(OwnerClientId + " is local player? " + IsLocalPlayer);
            GetComponent<Camera>().enabled = false;
        }
    }
}
