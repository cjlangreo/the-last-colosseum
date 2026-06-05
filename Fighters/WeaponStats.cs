using Godot;
using System;

[GlobalClass]
public partial class WeaponStats : Resource
{
    [Export(PropertyHint.Range, "0.3,1,0.05")] public float Weight = 0.5f;
    [Export] public float BaseDmg = 15f;
    [Export] public float CritMult = 1.5f;
}
