using Godot;
using TheLastColosseum;
using TheLastColosseum.Abilities;
using TheLastColosseum.Fighters;
using TheLastColosseum.Weapons;
using static TheLastColosseum.Utils.Debug;

[GlobalClass]
public partial class TeamSpawner : Marker2D
{
  [Export] public Team Team;
  [Export] public FighterEnum fighter = FighterEnum.Knight;
  [Export] public WeaponEnum weapon = WeaponEnum.Greataxe;
  [Export] public AbilityEnum ability = AbilityEnum.MarkOfTheBear;
  private Fighter _fighter;


  public override void _EnterTree()
  {

    Main main = GetNode<Main>("/root/Main");
    if (main.Random)
    {
      fighter = GetRandomEnumValue<FighterEnum>();
      weapon = GetRandomEnumValue<WeaponEnum>();
      ability = GetRandomEnumValue<AbilityEnum>();
    }
    
    AddFighter(fighter);
    AddWeapon(weapon);
    AddAbility(ability);
  }


  private void AddFighter(FighterEnum fighter)
  {
    FighterStats fighterStats = RefServer.fighters[fighter];
    _fighter = new()
    {
      FighterStats = fighterStats
    };
    _fighter.InitTeam(Team);
    
    AddChild(_fighter);

    if(Team == Team.A)
    {
      RefServer.FighterA = _fighter;
    } else
    {
      RefServer.FighterB = _fighter;
    }
  }


  private void AddWeapon(WeaponEnum weapon)
  {
    Weapon _weapon = (Weapon)RefServer.WeaponScenes[weapon].Instantiate();
    _fighter.AddChild(_weapon);
  }

  private void AddAbility(AbilityEnum ability)
  {
    Ability _ability = (Ability)RefServer.AbilityScenes[ability].Instantiate();
    _fighter.AddChild(_ability);
  }
  

}
