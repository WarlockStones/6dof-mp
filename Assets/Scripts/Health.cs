using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private int health = 5;
    public void Hurt()
    {
        if (IsServer)
        {
            --health;
            if (health <= 0)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
