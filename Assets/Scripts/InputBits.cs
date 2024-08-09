public enum InputTypes : byte
{ /*  x     y    z  |   x      y     z    = Unity convention */
    Pitch, Yaw, Roll,  Sway, Heave, Surge
}

public class InputBits 
{
    public const int positive = 0b01; // Key is pressed
    public const int negative = 0b11; // Key reverse is pressed
    public const int nill     = 0b00; // Key is not pressed

    public const int pitchPos = 0b0000000000000011; 
    public const int yawPos   = 0b0000000000001100; 
    public const int rollPos  = 0b0000000000110000; 
    public const int swayPos  = 0b0000000011000000; 
    public const int heavePos = 0b0000001100000000; 
    public const int surgePos = 0b0000110000000000;

    public const byte pitchSwitch = 0;
    public const byte yawSwitch   = 2;
    public const byte rollSwitch  = 4;
    public const byte swaySwitch  = 6;
    public const byte heaveSwitch = 8;
    public const byte surgeSwitch = 10;

    private struct Input
    {
        public int bitPosition;
        public byte bitSwitch;
    }

    private static Input[] inputs = {
           new Input { bitPosition = pitchPos, bitSwitch = pitchSwitch },
           new Input { bitPosition = yawPos,   bitSwitch = yawSwitch   },
           new Input { bitPosition = rollPos,  bitSwitch = rollSwitch  },
           new Input { bitPosition = swayPos,  bitSwitch = swaySwitch  },
           new Input { bitPosition = heavePos, bitSwitch = heaveSwitch },
           new Input { bitPosition = surgePos, bitSwitch = surgeSwitch }
    };

    // Returns the position the desired InputType
    public static int GetBitPosition(InputTypes input)
    {
        return inputs[(int)input].bitPosition;
    }

    // Returns the offset of the desired InputType for bit switching
    public static byte GetBitSwitch(InputTypes input)
    {
        return inputs[(int)input].bitSwitch;
    }

}
