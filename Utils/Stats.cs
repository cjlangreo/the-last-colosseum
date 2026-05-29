using System;

namespace RandomBattles.Utils;

public static class Stats
{
  public const float IntCooldownMult = 0.1011f;
  
  public static double GetAbilityCooldown(double cooldownBase, int intelligence)
  {
    return cooldownBase - (cooldownBase * intelligence * IntCooldownMult);
  }

}
