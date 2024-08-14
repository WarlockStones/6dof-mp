using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private uint maxHealth = 5;
    private NetworkVariable<uint> health = new NetworkVariable<uint>();

    private void Awake()
    {
        textXPos = Screen.width / 2;
        textXPos -= 90;
        textYPos = Screen.height - 50;
    }

    private void Start()
    {
        health.Value = maxHealth;
    }

    int textXPos, textYPos;
    private void OnGUI()
    {
        if (IsLocalPlayer)
        {
            GUI.Label(new Rect(textXPos, textYPos, 400, 100), $"You are Player {OwnerClientId}. Hitpoints: {health.Value}");
        }
    }

    public void Hurt(ulong shooterID)
    {
        if (IsServer)
        {
            --health.Value;
            if (health.Value <= 0)
            {
                GameState.instance.ScoreRPC(shooterID);
                GameState.instance.RespawnPlayer(transform);
            }
        }
    }

    public void Reset()
    {
        health.Value = maxHealth;
    }
}
