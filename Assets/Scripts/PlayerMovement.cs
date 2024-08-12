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
            inputBits.Value = UpdateInputBits(inputBits.Value, InputType.Pitch, inputActions.Gameplay.Pitch.ReadValue<float>());
            inputBits.Value = UpdateInputBits(inputBits.Value, InputType.Yaw,   inputActions.Gameplay.Yaw  .ReadValue<float>());
            inputBits.Value = UpdateInputBits(inputBits.Value, InputType.Roll,  inputActions.Gameplay.Roll .ReadValue<float>());
            inputBits.Value = UpdateInputBits(inputBits.Value, InputType.Sway,  inputActions.Gameplay.Sway .ReadValue<float>());
            inputBits.Value = UpdateInputBits(inputBits.Value, InputType.Heave, inputActions.Gameplay.Heave.ReadValue<float>());
            inputBits.Value = UpdateInputBits(inputBits.Value, InputType.Surge, inputActions.Gameplay.Surge.ReadValue<float>());

            // Debug.Log("inputBits: " + Convert.ToString(inputBits.Value, 2));
        }

        // TODO: Remove the use of NetworkTransform on client?
        if (IsServer)
        {
            currentSurge = UpdateSpeedValue(currentSurge, GetMovementFromInput(inputBits.Value, InputType.Surge), acceleration, maxSpeed);
            currentSway  = UpdateSpeedValue(currentSway,  GetMovementFromInput(inputBits.Value, InputType.Sway),  acceleration, maxSpeed);
            currentHeave = UpdateSpeedValue(currentHeave, GetMovementFromInput(inputBits.Value, InputType.Heave), acceleration, maxHeave);
            transform.position += transform.forward * currentSurge;
            transform.position += transform.right   * currentSway;
            transform.position += transform.up      * currentHeave;

            currentPitchSpeed = UpdateSpeedValue(currentPitchSpeed, GetMovementFromInput(inputBits.Value, InputType.Pitch), rotationSpeed, maxRotationSpeed);
            currentYawSpeed   = UpdateSpeedValue(currentYawSpeed,   GetMovementFromInput(inputBits.Value, InputType.Yaw),   rotationSpeed, maxRotationSpeed);
            currentRollSpeed  = UpdateSpeedValue(currentRollSpeed,  GetMovementFromInput(inputBits.Value, InputType.Roll),  rotationSpeed, maxRotationSpeed);
            transform.Rotate(currentPitchSpeed, currentYawSpeed, currentRollSpeed * -1);
        }
    }

    static sbyte GetMovementFromInput(ushort bits, InputType inputType)
    {
        if ((bits & Input.GetPositiveBitPosition(inputType)) != 0)
        {
            return 1;
        }
        else if ((bits & Input.GetNegativeBitPosition(inputType)) != 0)
        {
            return -1;
        }
        else
        {
            return 0;
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
            if (speed < 0.01f && speed > -0.01f) // Do not move if float is almost zero
            {
                speed = 0;
            }
        }
        
        return speed;
    }

    static ushort UpdateInputBits(ushort inInputBits, InputType inputType, float movementValue)
    {
        int bitSet = inInputBits; // Must be int because ~ operator only returns int

        if (movementValue > 0)
        {
            bitSet |=  Input.GetPositiveBitPosition(inputType);
            bitSet &= ~Input.GetNegativeBitPosition(inputType);
        }
        else if (movementValue < 0)
        {
            bitSet &= ~Input.GetPositiveBitPosition(inputType);
            bitSet |=  Input.GetNegativeBitPosition(inputType);
        }
        else
        {
            bitSet &= ~Input.GetPositiveBitPosition(inputType);
            bitSet &= ~Input.GetNegativeBitPosition(inputType);
        }

        return (ushort)bitSet; 
    }
}
