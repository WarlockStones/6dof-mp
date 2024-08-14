using UnityEngine;
using Unity.Netcode;


public class PlayerOnSpawn : NetworkBehaviour
{
    private bool  isFirstSpawn = true;
    void Start()
    {
        if (IsServer)
        {
            GameState.instance.AddPlayer(transform);
        }

        if (isFirstSpawn)
        {
            GameState.instance.RespawnPlayer(transform);
            isFirstSpawn = true;
        }
    }
}
