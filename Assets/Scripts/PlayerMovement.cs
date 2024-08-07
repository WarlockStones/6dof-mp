using System.Transactions;
using Unity.Netcode;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    IAC_Default inputActions;
    NetworkVariable<float> pitch, roll, yaw, surge, heave, sway;
    Rigidbody rb;

    [SerializeField] private float maxSpeed = 0.5f;
    [SerializeField] private float maxHeave = 0.3f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float maxRotationSpeed = 1.2f;

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
            currentSway  = UpdateSpeedValue(currentSway,  sway.Value,  acceleration, maxSpeed);
            currentHeave = UpdateSpeedValue(currentHeave, heave.Value, acceleration, maxHeave);
            transform.position += transform.forward * currentSurge;
            transform.position += transform.right   * currentSway;
            transform.position += transform.up      * currentHeave;

            // ROTATION
            currentPitchSpeed = UpdateSpeedValue(currentPitchSpeed, pitch.Value, rotationSpeed, maxRotationSpeed);
            currentYawSpeed   = UpdateSpeedValue(currentYawSpeed,   yaw.Value,   rotationSpeed, maxRotationSpeed);
            currentRollSpeed  = UpdateSpeedValue(currentRollSpeed,  roll.Value,  rotationSpeed, maxRotationSpeed);
            transform.Rotate(currentPitchSpeed, currentYawSpeed, currentRollSpeed * -1);
        }
    }

    private float currentSurge, currentHeave, currentSway, currentYawSpeed, currentPitchSpeed, currentRollSpeed;
    // Accelerate and deaccelerate for Surge, Heave, Sway
    static float UpdateSpeedValue(float inSpeed, float inputValue, float inAccleration, float maxSpeed)
    {
        float speed = inSpeed;
        float targetSpeed = inputValue * maxSpeed;
        if (speed < targetSpeed)
        {
            speed += inAccleration;
            Mathf.Clamp(speed, inSpeed, targetSpeed);
        }
        else if (inSpeed > targetSpeed)
        {
            speed -= inAccleration;
            Mathf.Clamp(speed, targetSpeed, inSpeed);
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
