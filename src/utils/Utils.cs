using System;
using Godot;

namespace TheLastColosseum.Utils;


public static class Debug
{
  public static void PrintDebug(string content, string ownerName)
  {
    string currentTime = Time.GetTimeStringFromSystem();
    GD.PrintRich($"[{currentTime}:{ownerName}] {content}");
  }
}

