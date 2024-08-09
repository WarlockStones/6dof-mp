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

    IAC_Default inputActions;
    
    [SerializeField] private float maxSpeed = 0.5f;
    [SerializeField] private float maxHeave = 0.3f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float maxRotationSpeed = 1.2f;

    private ushort inputBits; // 16 bits to store the input values
    const int positiveInput  = 0b01; // Key is pressed
    const int negativeInput  = 0b11; // Key reverse is pressed
    const int noInput = 0b00; // Key is not pressed

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

    struct InputBitValues
    {
        public float inputValue;
        public int bitPosition;
        public int bitSwitch;
    }
    InputBitValues[] inputVariables = new InputBitValues[6];

    enum Inputs : short
    { /*  x     y    z  |   x      y     z    = Unity convention */
        Pitch, Yaw, Roll,  Sway, Heave, Surge
    }



    PlayerMovement()
    {
        inputVariables[(int)Inputs.Pitch].bitPosition = 0b0000000000000011;
        inputVariables[(int)Inputs.Yaw]  .bitPosition = 0b0000000000001100;
        inputVariables[(int)Inputs.Roll] .bitPosition = 0b0000000000110000;
        inputVariables[(int)Inputs.Sway] .bitPosition = 0b0000000011000000;
        inputVariables[(int)Inputs.Heave].bitPosition = 0b0000001100000000;
        inputVariables[(int)Inputs.Surge].bitPosition = 0b0000110000000000;

        inputVariables[(int)Inputs.Pitch].bitSwitch = 0;
        inputVariables[(int)Inputs.Yaw]  .bitSwitch = 2;
        inputVariables[(int)Inputs.Roll] .bitSwitch = 4;
        inputVariables[(int)Inputs.Sway] .bitSwitch = 6;
        inputVariables[(int)Inputs.Heave].bitSwitch = 8;
        inputVariables[(int)Inputs.Surge].bitSwitch = 10;
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
            inputVariables[(int)Inputs.Pitch].inputValue = inputActions.Gameplay.Pitch.ReadValue<float>();
            inputVariables[(int)Inputs.Yaw]  .inputValue = inputActions.Gameplay.Yaw  .ReadValue<float>();
            inputVariables[(int)Inputs.Roll] .inputValue = inputActions.Gameplay.Roll .ReadValue<float>();
            inputVariables[(int)Inputs.Sway] .inputValue = inputActions.Gameplay.Sway .ReadValue<float>();
            inputVariables[(int)Inputs.Heave].inputValue = inputActions.Gameplay.Heave.ReadValue<float>();
            inputVariables[(int)Inputs.Surge].inputValue = inputActions.Gameplay.Surge.ReadValue<float>();

            inputBits = UpdateInputBits(inputBits, inputVariables);
            Debug.Log("inputBits: "+Convert.ToString(inputBits, 2));

            // result is inputBits of only the surgeBits moved to the rightmost position
            var s = inputVariables[(int)Inputs.Surge];
            Debug.Log("Final surgeBits: " + DecodeInputValue(inputBits, s.bitPosition, s.bitSwitch)); // 0b01 = 1, 0b11 = 3.
        }

        // TODO: Remove the use of NetworkTransform on client?
        if (IsServer)
        {
            /*
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
            */
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

    static ushort UpdateInputBits(ushort InInputBits, InputBitValues[] InputBitValues)
    {
        int bitSet = InInputBits;
        byte b;
        foreach (var v in InputBitValues)
        {
            b = noInput;
            if (v.inputValue > 0)
                b = positiveInput;
            else if (v.inputValue < 0)
                b = negativeInput;
            bitSet &= ~v.bitPosition;
            bitSet |= b << v.bitSwitch;
        }
        return (ushort)bitSet; 
    }
    static ushort UpdateInputBits(ushort inInputBits, float inputValue, int bitPosition, int bitShift)
    {
        int bitSet = inInputBits;
        byte b = noInput;
        if (inputValue > 0)
            b = positiveInput;
        else if (inputValue < 0)
            b = negativeInput;
        bitSet &= ~bitPosition;  // Zero out the selected bits
        bitSet |= b << bitShift; // Move the bits into the correct position

        return (ushort)bitSet;
    }

    static byte DecodeInputValue(int inputBits, int bitPosition, int bitShift)
    {
        int i = (inputBits & bitPosition) >> bitShift;
        return (byte)i;
    }
}
