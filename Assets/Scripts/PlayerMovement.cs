using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/* TODO:
 * - Server must handle the NetworkVariables and update Owner
 * 
 */

public class PlayerMovement : NetworkBehaviour
{
    IAC_Default InputActions;
    NetworkVariable<float> Pitch, Roll, Yaw, Surge, Heave, Sway;
    Rigidbody rb;
    bool InputsEnabled = false;

    [SerializeField] private float Acceleration = 50;
    [SerializeField] private float Torque = 20;

    PlayerMovement()
    {
        Pitch = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        Roll  = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        Yaw   = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        Surge = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        Heave = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        Sway  = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    }

    void Start()
    {
        if (IsLocalPlayer)
        {
            InputActions = new IAC_Default();
            InputActions.Gameplay.Enable();
            InputsEnabled = true; // TODO: Needed?
        }

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (IsLocalPlayer && InputsEnabled)
        {

             Pitch.Value = InputActions.Gameplay.Pitch.ReadValue<float>();
             Roll.Value  = InputActions.Gameplay.Roll .ReadValue<float>();
             Yaw.Value   = InputActions.Gameplay.Yaw  .ReadValue<float>();
             Surge.Value = InputActions.Gameplay.Surge.ReadValue<float>();
             Heave.Value = InputActions.Gameplay.Heave.ReadValue<float>();
             Sway.Value  = InputActions.Gameplay.Sway .ReadValue<float>();
            //.. etc
        }

        if (IsServer)
        {
            Debug.Log($"Pitch: {Pitch.Value}, Roll: {Roll.Value}, Yaw: {Yaw.Value}, Surge: {Surge.Value}, Heave: {Heave.Value}, Sway: {Sway.Value}");
            // Clamp inputs and document sus behaviour. Clamp would only work if not multithreaded. Otherwise you could change it when it is adding force suddenly Surge.Value becomes 300!

            // Rotation
            rb.AddTorque(transform.forward * Roll.Value  * Torque);
            rb.AddTorque(transform.right   * Pitch.Value * Torque);
            rb.AddTorque(transform.up      * Yaw.Value   * Torque);
           
            rb.AddForce(transform.forward * Surge.Value * Acceleration);
            rb.AddForce(transform.right   * Sway.Value  * Acceleration);
            rb.AddForce(transform.up      * Heave.Value * Acceleration);
        }
    }
}
