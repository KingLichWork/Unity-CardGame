using System.Collections.Generic;

public class PlayerState
{
    public HandZone Hand { get; }
    public FieldZone Field { get; }

    public PlayerState(int maxHand, int maxField)
    {
        Hand = new HandZone(maxHand);
        Field = new FieldZone(maxField);
    }
}
