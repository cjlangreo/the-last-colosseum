using Godot;
using System.Linq;
using TheLastColosseum.Fighters;

[GlobalClass]
public partial class TeamSpawner : Marker2D
{
  [Export] public Team Team;

  public override void _EnterTree()
  {
    foreach (Fighter fighter in GetChildren().OfType<Fighter>())
    {
      fighter.InitTeam(Team);
    }
  }

}
