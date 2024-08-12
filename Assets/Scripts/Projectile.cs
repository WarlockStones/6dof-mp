using Unity.Netcode;
using System.Threading.Tasks;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField]
    float movementSpeed = 200;

    bool isReady = false;

    Transform shooter;
    NetworkObject networkObject;

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    private float lifetimeRemaining = 1.5f;
    // TODO: Is the movement of the projectile now client authorative?
    private void Update()
    {
        if (IsServer)
        {
            transform.position += transform.forward * movementSpeed * Time.deltaTime;

            if (lifetimeRemaining > 0)
            {
                lifetimeRemaining -= Time.deltaTime;
            }
            else
            {
                Die();
            }
        }
    }

    public void SetupProjectile(Transform inShooter, Vector3 MovementAdjustment)
    {
        shooter = inShooter;
        isReady = true;
        networkObject.Spawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && isReady && other != null && other.transform.root != shooter)
        {
            Debug.Log("Projectile hit: "+other.gameObject.name);
            var health = other.transform.root.GetComponent<Health>();
            if (health != null)
            {
                health.Hurt();
            }

            Die();
        }
    }

    private void Die()
    {

        if (networkObject.IsSpawned)
        {
            networkObject.Despawn();
        }
        else if (gameObject)
        {
            Destroy(gameObject);
        }
    }
}
