public enum InputType : byte
{ /*  x     y    z  |   x      y     z    = Unity convention */
    Pitch, Yaw, Roll,  Sway, Heave, Surge
}

public class Input
{
    private enum InputBit : ushort
    {
        PitchPositive = 0b0000000000000001,
        PitchNegative = 0b0000000000000010,
        YawPositive   = 0b0000000000000100,
        YawNegative   = 0b0000000000001000,
        RollPositive  = 0b0000000000010000,
        RollNegative  = 0b0000000000100000,
        SwayPositive  = 0b0000000001000000,
        SwayNegative  = 0b0000000010000000,
        HeavePositive = 0b0000000100000000,
        HeaveNegative = 0b0000001000000000,
        SurgePositive = 0b0000010000000000,
        SurgeNegative = 0b0000100000000000
    }

    private struct InputEntry
    {
        public InputType type;
        public InputBit positiveBitPosition;
        public InputBit negativeBitPosition;
    }

    private static InputEntry[] inputs = {
        new InputEntry { type = InputType.Pitch, positiveBitPosition = InputBit.PitchPositive, negativeBitPosition = InputBit.PitchNegative },
        new InputEntry { type = InputType.Yaw,   positiveBitPosition = InputBit.YawPositive,   negativeBitPosition = InputBit.YawNegative   },
        new InputEntry { type = InputType.Roll,  positiveBitPosition = InputBit.RollPositive,  negativeBitPosition = InputBit.RollNegative  },
        new InputEntry { type = InputType.Sway,  positiveBitPosition = InputBit.SwayPositive,  negativeBitPosition = InputBit.SwayNegative  },
        new InputEntry { type = InputType.Heave, positiveBitPosition = InputBit.HeavePositive, negativeBitPosition = InputBit.HeaveNegative },
        new InputEntry { type = InputType.Surge, positiveBitPosition = InputBit.SurgePositive, negativeBitPosition = InputBit.SurgeNegative },
    };

    public static ushort GetPositiveBitPosition(InputType type)
    {
        return (ushort)inputs[(int)type].positiveBitPosition;
    }

    public static ushort GetNegativeBitPosition(InputType type)
    {
        return (ushort)inputs[(int)type].negativeBitPosition;
    }
}
