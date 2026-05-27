using Godot;
using System;
using System.Diagnostics;

public partial class Powerup : Area2D
{
  public enum PowerupType
  {
    Speed,
    Health,
    Damage
  }

  public bool Random = false;
  [Export] public Sprite2D SpriteNode;
  [Export] public PowerupType Type {set;get;} = PowerupType.Speed;
  [ExportGroup("Sprites")]
  [Export] public CompressedTexture2D SpeedTexture;
  [Export] public CompressedTexture2D HealthTexture;
  [Export] public CompressedTexture2D DamageTexture;

  public Action PickedUp;

  public override void _Ready()
  {
    if (Random)
    {
      GD.Print($"Old type: {Type}");
      Random random = new();
      Array enumValues = Enum.GetValues(typeof(PowerupType));
      Type = (PowerupType)enumValues.GetValue(random.Next(enumValues.Length));
      GD.Print($"New type: {Type}");
    }

    switch (Type)
    {
      case PowerupType.Speed:
        SpriteNode.Texture = SpeedTexture;
        break;
      case PowerupType.Health:
        SpriteNode.Texture = HealthTexture;
        break;
      case PowerupType.Damage:
        SpriteNode.Texture = DamageTexture;
        break;
    }
    GD.Print($"{Type} powerup spawned");
  }

  public void OnBodyEntered(PhysicsBody2D body)
  {
    if (body is not Fighter) return;
    Fighter fighter = (Fighter)body;
    switch (Type)
    {
      case PowerupType.Speed:
        fighter.Speed += 100;
        break;
      case PowerupType.Health:
        fighter.Heal(50);
        break;
      case PowerupType.Damage:
        fighter.Damage += 10;
        break;
    }
    PickedUp.Invoke();
    QueueFree();
  }
}
