using Mono.Cecil;
using System;
using Unity.Netcode;
using UnityEngine;



public class PlayerMovement : NetworkBehaviour
{
    IAC_Default inputActions;
    
    [SerializeField] private float maxSpeed = 0.5f;
    [SerializeField] private float maxHeave = 0.3f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private float rotationSpeed = 0.1f;
    [SerializeField] private float maxRotationSpeed = 1.2f;

    // 16bits to store all input values
    NetworkVariable<ushort> inputBits = new NetworkVariable<ushort>(writePerm: NetworkVariableWritePermission.Owner); 


    struct InputValue
    {
        public float value;
        public InputTypes type;
    }
    InputValue[] inputValues = new InputValue[6];

    PlayerMovement()
    {
        inputValues[(int)InputTypes.Pitch].type = InputTypes.Pitch;
        inputValues[(int)InputTypes.Yaw]  .type = InputTypes.Yaw;
        inputValues[(int)InputTypes.Roll] .type = InputTypes.Roll;
        inputValues[(int)InputTypes.Sway] .type = InputTypes.Sway;
        inputValues[(int)InputTypes.Heave].type = InputTypes.Heave;
        inputValues[(int)InputTypes.Surge].type = InputTypes.Surge;
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
            inputValues[(int)InputTypes.Pitch].value = inputActions.Gameplay.Pitch.ReadValue<float>();
            inputValues[(int)InputTypes.Yaw]  .value = inputActions.Gameplay.Yaw  .ReadValue<float>();
            inputValues[(int)InputTypes.Roll] .value = inputActions.Gameplay.Roll .ReadValue<float>();
            inputValues[(int)InputTypes.Sway] .value = inputActions.Gameplay.Sway .ReadValue<float>();
            inputValues[(int)InputTypes.Heave].value = inputActions.Gameplay.Heave.ReadValue<float>();
            inputValues[(int)InputTypes.Surge].value = inputActions.Gameplay.Surge.ReadValue<float>();

            inputBits.Value = UpdateInputBits(inputBits.Value, inputValues);
            Debug.Log("inputBits: " + Convert.ToString(inputBits.Value, 2));

            // result is inputBits of only the surgeBits moved to the rightmost position
            var s = inputValues[(int)InputTypes.Surge];
            Debug.Log("Final surgeBits: " + GetInputBits(inputBits.Value, InputTypes.Surge)); // 0b01 = 1, 0b11 = 3.
        }

        // TODO: Remove the use of NetworkTransform on client?
        if (IsServer)
        {
            /*
            int surgeInput = DecodeInputBits(DecodeInputValue(inputBits.Value, InputBits.surgePos, InputBits.surgeSwitch));
            currentSurge = UpdateSpeedValue(currentSurge, surgeInput, acceleration, maxSpeed);
            transform.position += transform.forward * currentSurge;
            //.... I can not just continue do these 3 steps that is super messy
            */ 

            for (int i = 0; i < inputValues.Length; i++)
            {
                var v = inputValues[i];
                v.value = DecodeInputBits(GetInputBits(inputBits.Value, v.type));
            }

            // TODO: Make this cleaner
            currentSurge = UpdateSpeedValue(currentSurge, inputValues[(int)InputTypes.Surge].value, acceleration, maxSpeed);
            currentSway  = UpdateSpeedValue(currentSway,  inputValues[(int)InputTypes.Sway] .value, acceleration, maxSpeed);
            currentHeave = UpdateSpeedValue(currentHeave, inputValues[(int)InputTypes.Heave].value, acceleration, maxHeave);
            transform.position += transform.forward * currentSurge;
            transform.position += transform.right   * currentSway;
            transform.position += transform.up      * currentHeave;

            currentPitchSpeed = UpdateSpeedValue(currentPitchSpeed, inputValues[(int)InputTypes.Pitch].value, rotationSpeed, maxRotationSpeed);
            currentYawSpeed   = UpdateSpeedValue(currentYawSpeed,   inputValues[(int)InputTypes.Yaw]  .value, rotationSpeed, maxRotationSpeed);
            currentRollSpeed  = UpdateSpeedValue(currentRollSpeed,  inputValues[(int)InputTypes.Roll] .value, rotationSpeed, maxRotationSpeed);
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

    static ushort UpdateInputBits(ushort InInputBits, InputValue[] InputBitValues)
    {
        int bitSet = InInputBits;
        byte b;
        foreach (var v in InputBitValues)
        {
            b = InputBits.nill;
            if (v.value > 0)
                b = InputBits.positive;
            else if (v.value < 0)
                b = InputBits.negative;
            bitSet &= ~(InputBits.GetBitPosition(v.type));
            bitSet |= b << (InputBits.GetBitSwitch(v.type));
        }
        return (ushort)bitSet; 
    }

    static ushort UpdateInputBits(ushort inInputBits, float inputValue, int bitPosition, int bitShift)
    {
        int bitSet = inInputBits;
        byte b = InputBits.nill;
        if (inputValue > 0)
            b = InputBits.positive;
        else if (inputValue < 0)
            b = InputBits.negative;
        bitSet &= ~bitPosition;  // Zero out the selected bits
        bitSet |= b << bitShift; // Move the bits into the correct position

        return (ushort)bitSet;
    }

    // Returns the two bits for that specific input position
    static byte GetInputBits(int inputBits, InputTypes type)
    {
        int bitPosition = InputBits.GetBitPosition(type);
        int bitShift = InputBits.GetBitSwitch(type);
        int i = (inputBits & bitPosition) >> bitShift;
        return (byte)i;
    }

    // Read the two bits and decode if its is positive, negative, or nil
    static short DecodeInputBits(int bits)
    {
        if (bits == InputBits.positive)
            return 1;
        else if (bits == InputBits.negative)
            return -1;

        return 0;
    }
}
