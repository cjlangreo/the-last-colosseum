using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheLastColosseum.Fighters;

namespace TheLastColosseum.Weapons;

[GlobalClass]
public partial class Hitbox : Area2D
{
	[Export] private Color HitboxColor = Colors.Red;
	public event Action<Fighter> Hit;
	private List<Fighter> _fightersHit = [];

	public Team Team;
	private CollisionShape2D _colObject;

    public override void _EnterTree()
	{
		UniqueNameInOwner = true;
		Monitorable = false;
		ToggleListen(false);
		BodyEntered += FighterDetected;
	}


    public override void _Ready()
	{
		_colObject = GetChildren().OfType<CollisionShape2D>().First();
		SetHitboxColor(HitboxColor);
	}


    public override void _ExitTree()
	{
		BodyEntered -= FighterDetected;
	}
	
	public void InitCollissions()
	{
		SetCollisionLayerValue(1, false);
		SetCollisionMaskValue(1, false);

		if( Team == Team.A)
		{
			SetCollisionMaskValue((int)ColLayer.B, true);
		} else
		{
			SetCollisionMaskValue((int)ColLayer.A, true);
		}
	}

	private void SetHitboxColor(Color color)
	{
		_colObject.DebugColor = color;
	}
	

	private void FighterDetected(Node2D body)
	{
		if(body is Fighter fighter && fighter.team != Team && !_fightersHit.Contains(fighter))
		{
			Hit.Invoke(fighter);
			_fightersHit.Add(fighter);
		}
	}


	public void ToggleListen(bool listen)
	{
		_fightersHit.Clear();
		SetDeferred("Monitoring", listen);
		Visible = listen;
	}
}
