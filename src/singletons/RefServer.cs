using System.Collections.Generic;
using Godot;
using TheLastColosseum.Fighters;
using TheLastColosseum.Abilities;
using TheLastColosseum.Weapons;

public static partial class RefServer
{
	public static Fighter FighterA;
	public static Fighter FighterB;


	public static Dictionary<FighterEnum, FighterStats> fighters = new()
	{
		{FighterEnum.Knight, GD.Load<FighterStats>("uid://bestvydmmqvt8")},
		{FighterEnum.Orc, GD.Load<FighterStats>("uid://drleqmgfq1f5y")},
		{FighterEnum.Ninja, GD.Load<FighterStats>("uid://c823xunr1p5nm")},
		{FighterEnum.Samurai, GD.Load<FighterStats>("uid://csa0caxug1vds")}
	};
	public static Dictionary<FighterEnum, Texture2D> FighterIcons = new(){};

	public static Dictionary<WeaponEnum, Texture2D> WeaponIcons = new() { };
	public static Dictionary<AbilityEnum, Texture2D> AbilityIcons = new() {};

	public static Dictionary<WeaponEnum, PackedScene> WeaponScenes = new()
	{
		{WeaponEnum.DoubleSai, GD.Load<PackedScene>("uid://dladnw68amri6")},
		{WeaponEnum.Greataxe, GD.Load<PackedScene>("uid://bfrclebkpbjo2")},
		{WeaponEnum.IronSword, GD.Load<PackedScene>("uid://dp5ycbv4t5w65")},
		{WeaponEnum.Katana, GD.Load<PackedScene>("uid://cao3vxouldjfw")}
	};

	public static Dictionary<AbilityEnum, PackedScene> AbilityScenes = new()
	{
		{AbilityEnum.DisciplinedStrike, GD.Load<PackedScene>("uid://b3hxav4hll64s")},
		{AbilityEnum.MarkOfTheBear, GD.Load<PackedScene>("uid://hbrcckwyp775")},
		{AbilityEnum.MarkOfTheOwl, GD.Load<PackedScene>("uid://dwrtxb2jov3dq")},
		{AbilityEnum.MarkofTheSnake, GD.Load<PackedScene>("uid://b7vdewbdwy1up")},
		{AbilityEnum.RaiseShield, GD.Load<PackedScene>("uid://cmmqc1o0t6nd6")},
	};

	static RefServer()
	{
		foreach (PackedScene weaponScene in WeaponScenes.Values)
		{
			Weapon weapon = weaponScene.Instantiate<Weapon>();
			WeaponIcons.Add(weapon.WeaponEnum, (Texture2D)weapon.WeaponSprites[0].Texture.Duplicate());
			weapon.QueueFree();
		}

		foreach (PackedScene abilityScene in AbilityScenes.Values)
		{
			Ability ability = abilityScene.Instantiate<Ability>();
			AbilityIcons.Add(ability.AbilityEnum, (Texture2D)ability.AbilityIcon.Duplicate());
			ability.QueueFree();
		}

		foreach (FighterStats fighterStats in fighters.Values)
		{
			FighterIcons.Add(fighterStats.Fighter, fighterStats.FighterIcon);
		}
	}

}
