using Godot;
using TheLastColosseumFighters;
using System;

public static class EventBus
{
    public static Fighter FighterA;
    public static Fighter FighterB;

    public static Action<string> Hit;
}
