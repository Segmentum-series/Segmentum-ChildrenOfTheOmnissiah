using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld.Planet;
using VEF;

namespace Seg
{
public class Ability_StripWeapon : VEF.Abilities.Ability
{
  public override void Cast(params GlobalTargetInfo[] targets)
  {
    base.Cast(targets);
    if (((IList<GlobalTargetInfo>) targets).NullOrEmpty<GlobalTargetInfo>())
      return;
    GlobalTargetInfo target = targets[0];
    Map map = this.pawn.Map;
    IntVec3 cell = target.Cell;
    if (!cell.InBounds(map) || !(target.Thing is Pawn thing))
      return;
    if (thing.equipment != null && thing.equipment.Primary != null)
      thing.equipment.TryDropEquipment(thing.equipment.Primary, out ThingWithComps _, this.CasterPawn.Position);
  }
}
}

