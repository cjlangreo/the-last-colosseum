using Godot;
using TheLastColosseum.Abilities;
using TheLastColosseum.Fighters;
using TheLastColosseum.Weapons;

public partial class AFanaxeSwing : Ability
{
  [Export] private float AbilityDamage = 10f;
  private Hitbox _hitbox;
  private AnimationPlayer _animPlayer;
  private AudioStreamPlayer _audioStreamPlayer;

  private void GetNodeRefs()
  {
    _hitbox = GetNode<Hitbox>("%Hitbox");
    _animPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
    _audioStreamPlayer = GetNode<AudioStreamPlayer>("%AudioStreamPlayer");
  }

  public override void InitAbility()
  {
    base.InitAbility();
    GetNodeRefs();
    _animPlayer.AnimationFinished += OnSwingEnd;
    _hitbox.Hit += OnHit;
  }
  public override void UseAbility()
  {
    base.UseAbility();
    _animPlayer.Play("swing");
  }

  private void OnSwingEnd(StringName _)
  {
    EndAbility();
  }

  private void OnHit(Fighter fighter)
  {
    HitStatus hitStatus = fighter.HitRequest(AbilityDamage, false, 100f);
    if( hitStatus == HitStatus.Hit)
    {
    _audioStreamPlayer.Play();
      GD.Print("Hit");
    }
  }

  // Explicitly call
  public override void EndAbility()
  {
    base.EndAbility();
  }
}
