using Godot;
using System;

public static class EventManager
{
    public static Action<Fighter.Team> RoundEnd;
    public static int Rounds = 0;
    public static int TeamAPoints = 0;
    public static int TeamBPoints = 0;

}
