using System.Transactions;
using Unity.Netcode;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    IAC_Default inputActions;
    NetworkVariable<float> pitch, roll, yaw, surge, heave, sway;
    Rigidbody rb;
    bool inputsEnabled = false;


    [SerializeField] private float maxSpeed = 0.5f;
    private float maxHeave = 0.3f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float rotationSpeed = 0.25f;

    PlayerMovement()
    {
        pitch = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        roll  = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        yaw   = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        surge = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        heave = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
        sway  = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    }

    void Start()
    {
        if (IsLocalPlayer)
        {
            inputActions = new IAC_Default();
            inputActions.Gameplay.Enable();
        }
    }

    void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
             pitch.Value = inputActions.Gameplay.Pitch.ReadValue<float>();
             roll.Value  = inputActions.Gameplay.Roll .ReadValue<float>();
             yaw.Value   = inputActions.Gameplay.Yaw  .ReadValue<float>();
             surge.Value = inputActions.Gameplay.Surge.ReadValue<float>();
             heave.Value = inputActions.Gameplay.Heave.ReadValue<float>();
             sway.Value  = inputActions.Gameplay.Sway .ReadValue<float>();
        }

        if (IsServer)
        {
            // Debug.Log($"Pitch: {Pitch.Value}, Roll: {Roll.Value}, Yaw: {Yaw.Value}, Surge: {Surge.Value}, Heave: {Heave.Value}, Sway: {Sway.Value}");
            // Clamp inputs and document sus behaviour. Clamp would only work if not multithreaded. Otherwise you could change it when it is adding force suddenly Surge.Value becomes 300!
            currentSurge = UpdateSpeedValue(currentSurge, surge.Value, acceleration, maxSpeed);
            transform.position += transform.forward * currentSurge;

            currentSway = UpdateSpeedValue(currentSway, sway.Value, acceleration, maxSpeed);
            transform.position += transform.right * currentSway;

            curretHeave = UpdateSpeedValue(curretHeave, heave.Value, acceleration, maxHeave); // This way I can modify without brining in more variables
            transform.position += transform.up * curretHeave;

            // ROTATION
            // Start with pitch then try other things
            /*
            rb.AddTorque(transform.forward * Roll.Value  * Torque);
            rb.AddTorque(transform.right   * Pitch.Value * Torque);
            rb.AddTorque(transform.up      * Yaw.Value   * Torque);
           
            rb.AddForce(transform.forward * Surge.Value * Acceleration);
            rb.AddForce(transform.right   * Sway.Value  * Acceleration);
            rb.AddForce(transform.up      * Heave.Value * Acceleration);
            */
        }
    }

    private float currentSurge, curretHeave, currentSway;
    // Accelerate and deaccelerate for Surge, Heave, Sway
    static float UpdateSpeedValue(float inCurrentSpeed, float inputValue, float inAcceleration, float inMaxSpeed)
    {
        float speed = inCurrentSpeed;
        // targetSurge = surge.Value * maxSpeed;
        float targetSpeed = inputValue * inMaxSpeed;
        if (speed < targetSpeed)
        {
            speed += inAcceleration;
            Mathf.Clamp(speed, inCurrentSpeed, targetSpeed);
        }
        else if (inCurrentSpeed > targetSpeed)
        {
            speed -= inAcceleration;
            Mathf.Clamp(speed, targetSpeed, inCurrentSpeed);
        }
        if (targetSpeed == 0)
        {
            if (speed < 0.01f && speed > -0.01f)
            {
                speed = 0;
            }
        }
        
        return speed;
    }
}
