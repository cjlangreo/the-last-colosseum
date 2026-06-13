using System;
using Godot;
using TheLastColosseum.Fighters;

namespace TheLastColosseum.Utils;


public static class Debug
{
  private static readonly Random _random = new();
  public static void PrintDebug(string content, string ownerName)
  {
    string currentTime = Time.GetTimeStringFromSystem();
    GD.PrintRich($"[{currentTime}:{Engine.GetProcessFrames()}|{ownerName}] {content}");
  }

public static T GetRandomEnumValue<T>() where T : Enum 
{
    T[] enumValues = (T[])Enum.GetValues(typeof(T));
    int randomIndex = _random.Next(enumValues.Length);
    return enumValues[randomIndex];
}

}

