using Godot;
using System;
using TheLastColosseum.Fighters;

[GlobalClass]
public partial class FighterStats : Resource
{
    [Export] public string FighterName;
    [Export] public Texture2D FighterIcon;
    [Export] public Texture2D FighterHands;
    [Export] public int Strength;
    [Export] public int Agility;
    [Export] public int Intelligence;
}
