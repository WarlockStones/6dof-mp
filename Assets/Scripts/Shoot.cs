using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

public class Shoot : NetworkBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform spawnPosition;

    IAC_Default inputActions;

    private void Start()
    {
        if (IsLocalPlayer)
        {
            inputActions = new IAC_Default();
            inputActions.Gameplay.Enable();
            inputActions.Gameplay.Shoot.performed += context => FireRPC();
        }
    }

    private void Update()
    {
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }
    }


    float fireCooldownsInSeconds = 0.1f;
    float fireTimer;
    // RPC
    [Rpc(SendTo.Server)]
    void FireRPC()
    {
        if (fireTimer <= 0)
        {
            GameObject go = Instantiate(projectilePrefab, transform.position, transform.rotation);
            go.GetComponent<Projectile>().SetupProjectile(transform, new Vector3(0, 0, 0));
            fireTimer = fireCooldownsInSeconds;
        }
    }
}
