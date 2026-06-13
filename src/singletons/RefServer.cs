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

	public static Dictionary<WeaponEnum, Weapon> weapons = new() { };
	public static Dictionary<AbilityEnum, Ability> abilities = new() { };

	private static Dictionary<WeaponEnum, PackedScene> _weaponScenes = new()
	{
		{WeaponEnum.DoubleSai, GD.Load<PackedScene>("uid://dladnw68amri6")},
		{WeaponEnum.Greataxe, GD.Load<PackedScene>("uid://bfrclebkpbjo2")},
		{WeaponEnum.IronSword, GD.Load<PackedScene>("uid://dp5ycbv4t5w65")},
		{WeaponEnum.Katana, GD.Load<PackedScene>("uid://cao3vxouldjfw")}
	};

	private static Dictionary<AbilityEnum, PackedScene> _abilityScenes = new()
	{
		{AbilityEnum.DisciplinedStrike, GD.Load<PackedScene>("uid://b3hxav4hll64s")},
		{AbilityEnum.MarkOfTheBear, GD.Load<PackedScene>("uid://hbrcckwyp775")},
		{AbilityEnum.MarkOfTheOwl, GD.Load<PackedScene>("uid://dwrtxb2jov3dq")},
		{AbilityEnum.MarkofTheSnake, GD.Load<PackedScene>("uid://b7vdewbdwy1up")},
		{AbilityEnum.RaiseShield, GD.Load<PackedScene>("uid://cmmqc1o0t6nd6")},
	};

	static RefServer()
	{
		foreach (PackedScene weaponScene in _weaponScenes.Values)
		{
			Weapon weapon = weaponScene.Instantiate<Weapon>();
			weapons.Add(weapon.WeaponEnum, weapon);
		}

		foreach (PackedScene abilityScene in _abilityScenes.Values)
		{
			Ability ability = abilityScene.Instantiate<Ability>();
			abilities.Add(ability.AbilityEnum, ability);
		}
	}

}
