using Godot;
using System;
using System.Collections.Generic;
using TheLastColosseum.Fighters;

[GlobalClass]
public partial class FighterStats : Resource
{
    [Export] public FighterEnum Fighter;
    [Export] public Texture2D FighterIcon;
    [Export] public Texture2D FighterHands;
    [Export] public int Strength;
    [Export] public int Agility;
    [Export] public int Intelligence;

    public Dictionary<FighterEnum, string> FighterNames = new()
    {
      {FighterEnum.Knight, "Knight"},
      {FighterEnum.Samurai, "Samurai"},
      {FighterEnum.Orc, "Orc"},
      {FighterEnum.Ninja, "Ninja"},
    };
}
