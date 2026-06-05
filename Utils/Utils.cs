using System;
using Godot;

namespace RandomBattles.Utils;


public static class Debug
{
  public static void PrintDebug(string content, string ownerName)
  {
    string currentTime = Time.GetTimeStringFromSystem();
    GD.PrintRich($"[{currentTime}:{ownerName}] {content}");
  }
}

