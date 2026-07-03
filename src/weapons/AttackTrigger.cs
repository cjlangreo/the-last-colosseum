using Godot;
using System;
using System.Linq;
using TheLastColosseum.Fighters;

namespace TheLastColosseum.Weapons;

[GlobalClass]
public partial class AttackTrigger : Area2D
{
	public bool EnemyDetected = false;
	public Team team;

    public override void _EnterTree()
	{
		UniqueNameInOwner = true;
	}

	public void InitCollissions()
	{
		SetCollisionLayerValue(1, false);
		SetCollisionMaskValue(1, false);
		if( team == Team.A)
		{
			SetCollisionMaskValue((int)ColLayer.B, true);
		} else
		{
			SetCollisionMaskValue((int)ColLayer.A, true);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		EnemyDetected = false;
		if(!Monitoring || !HasOverlappingBodies()) return;
		foreach(Fighter fighter in GetOverlappingBodies().OfType<Fighter>())
		{
			if(fighter.team != team)
			{
				EnemyDetected = true;
				return;
			}
		}
	}
}
