using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
namespace Seg
{
    public class Ability_Waveburst : VEF.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (((IList<GlobalTargetInfo>)targets).NullOrEmpty<GlobalTargetInfo>())
                return;
            Map map = this.CasterPawn.Map;
            if (map == null)
                return;
            IntVec3 cell = targets[0].Cell;
            SoundDef.Named("Seg_COTO_ArcSound").PlayOneShot(new TargetInfo(cell, map));
            int ticks = 240;
            float radius = this.def.radius;
            foreach (IntVec3 c in GenRadial.RadialCellsAround(cell, radius, true))
            {
                if (!c.InBounds(map))
                    continue;
                foreach (Pawn pawn in map.thingGrid.ThingsListAt(c).OfType<Pawn>())
                {
                    if (pawn == this.CasterPawn)
                        continue;
                    if (pawn.stances?.stunner != null)
                        pawn.stances.stunner.StunFor(ticks, (Thing)this.CasterPawn);
                }

            }
        }
    }
}
