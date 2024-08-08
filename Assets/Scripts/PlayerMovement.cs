using System;
using System.Collections;
using Unity.Netcode;
using Unity.Networking.Transport.Error;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerMovement : NetworkBehaviour
{
    // Change to bitset?
    NetworkVariable<float> pitch, roll, yaw, surge, heave, sway;

    IAC_Default inputActions;
    Rigidbody rb;
    
    [SerializeField] private float maxSpeed = 0.5f;
    [SerializeField] private float maxHeave = 0.3f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float maxRotationSpeed = 1.2f;

    private int inputBits; // 16 bits to store the input values
    const int pitchBits = 0b0000000000000011;
    const int rollBits  = 0b0000000000001100;
    const int yawBits   = 0b0000000000110000;
    const int surgeBits = 0b0000000011000000;
    const int heaveBits = 0b0000001100000000;
    const int swayBits  = 0b0000110000000000;

    const int pitchSwitch = 0;
    const int rollSwitch  = 2;
    const int yawSwitch   = 4;
    const int surgeSwitch = 6;
    const int heaveSwitch = 8;
    const int swaySwitch  = 10;

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
        Debug.Log("Player spawned: "+OwnerClientId);
        if (IsLocalPlayer)
        {
            Debug.Log("Is local player");
            inputActions = new IAC_Default();
            inputActions.Gameplay.Enable();
        }
        else
            Debug.Log("Not local player");
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

            // IT WORKS!
            byte s = 0;
            if (surge.Value > 0)
                s = 0b01;
            else if (surge.Value < 0)
                s = 0b11;
            inputBits &= ~surgeBits; // Zero out the bits
            inputBits |= s << surgeSwitch; // Move value s to the correct position

            byte p = 0;
            if (pitch.Value > 0)
                p = 0b01;
            else if (pitch.Value < 0)
                p = 0b11;
            inputBits &= ~pitchBits;
            inputBits |= p << pitchSwitch;

            byte sw = 0;
            if (sway.Value > 0)
                sw = 0b01;
            else if (sway.Value < 0)
                sw = 0b11;
            inputBits &= ~swayBits;
            inputBits |= sw << swaySwitch;
            Debug.Log("inputBits: "+Convert.ToString(inputBits, 2));

            // Reverse the process
            // Is what is the sway value? 1 0 -1?
            int result = (inputBits & surgeBits) >> surgeSwitch;
            Debug.Log("final surge is: " + Convert.ToString(result, 2));
        }

        // TODO: Remove the use of NetworkTransform on client?
        if (IsServer)
        {
            // TODO: Clamp input value. It is currently client authorative

            currentSurge = UpdateSpeedValue(currentSurge, surge.Value, acceleration, maxSpeed);
            currentSway  = UpdateSpeedValue(currentSway,  sway.Value,  acceleration, maxSpeed);
            currentHeave = UpdateSpeedValue(currentHeave, heave.Value, acceleration, maxHeave);
            transform.position += transform.forward * currentSurge;
            transform.position += transform.right   * currentSway;
            transform.position += transform.up      * currentHeave;

            currentPitchSpeed = UpdateSpeedValue(currentPitchSpeed, pitch.Value, rotationSpeed, maxRotationSpeed);
            currentYawSpeed   = UpdateSpeedValue(currentYawSpeed,   yaw.Value,   rotationSpeed, maxRotationSpeed);
            currentRollSpeed  = UpdateSpeedValue(currentRollSpeed,  roll.Value,  rotationSpeed, maxRotationSpeed);
            transform.Rotate(currentPitchSpeed, currentYawSpeed, currentRollSpeed * -1);
        }
    }

    private float currentSurge, currentHeave, currentSway, currentYawSpeed, currentPitchSpeed, currentRollSpeed;
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
