using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace Seg.COTO
{
    public class CompProperties_PhosphorFire : CompProperties
    {
        public CompProperties_PhosphorFire()
        {
            this.compClass = typeof(CompPhosphorFire);
        }
    }

    public class CompPhosphorFire : ThingComp
    {
        public override string CompInspectStringExtra()
        {
            return "Burning (Phosphor Fire)";
        }
    }

    public class CompProperties_Targetable : CompProperties
    {
        public CompProperties_Targetable()
        {
            this.compClass = typeof(CompTargetable);
        }
    }

    public class CompTargetable : ThingComp
    {
    }

    public class PhosphorFire : Fire
    {
        private int ageTicks;
        private const int LifetimeTicks = 10000;

        protected override void Tick()
        {
            base.Tick();

            ageTicks++;
            if (ageTicks >= LifetimeTicks)
            {
                Destroy(DestroyMode.Vanish);
                return;
            }

            if (!Spawned || Map == null)
                return;

            if (this.IsHashIntervalTick(60))
                DoPhosphorDamageAndHeat();

            if (this.IsHashIntervalTick(120))
                IgniteNearbyPawns();

            if (this.IsHashIntervalTick(1200))
                TryPhosphorSpread();
        }

        private void DoPhosphorDamageAndHeat()
        {
            var list = Map.thingGrid.ThingsListAt(Position);
            for (int i = 0; i < list.Count; i++)
            {
                Thing t = list[i];
                if (t == this)
                    continue;

                if (t.def.category == ThingCategory.Mote)
                    continue;

                float dmg = 3f;
                t.TakeDamage(new DamageInfo(DamageDefOf.Flame, dmg, instigator: this));
            }

            float energy = this.fireSize * 160f;
            GenTemperature.PushHeat(Position, Map, energy);

            if (Rand.Value < 0.4f)
                WeatherBuildupUtility.AddSnowRadial(Position, Map, this.fireSize * 3f, -(this.fireSize * 0.1f));
        }

        private void IgniteNearbyPawns()
        {
            var pawns = Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!pawn.Position.InHorDistOf(Position, 1.5f))
                    continue;

                if (pawn.Dead || pawn.Downed)
                    continue;

                pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, 2f, instigator: this));

                if (Rand.Chance(0.15f))
                {
                    if (Map.thingGrid.ThingAt<PhosphorFire>(pawn.Position) == null)
                    {
                        var fire = (PhosphorFire)ThingMaker.MakeThing(ThingDef.Named("Seg_COTO_PhosphorFire"));
                        GenSpawn.Spawn(fire, pawn.Position, Map);
                    }
                }
            }
        }

        private void TryPhosphorSpread()
        {
            IntVec3 c = Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(1, 8)];
            if (!c.InBounds(Map))
                return;

            if (!c.Walkable(Map))
                return;

            if (Map.thingGrid.ThingAt<PhosphorFire>(c) != null)
                return;

            var fire = (PhosphorFire)ThingMaker.MakeThing(ThingDef.Named("Seg_COTO_PhosphorFire"));
            GenSpawn.Spawn(fire, c, Map);
        }
    }

    public class Projectile_Phosphor : Bullet
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 pos = Position;

            base.Impact(hitThing, blockedByShield);

            if (map == null)
                return;

            var fire = (PhosphorFire)ThingMaker.MakeThing(ThingDef.Named("Seg_COTO_PhosphorFire"));
            GenSpawn.Spawn(fire, pos, map);
        }
    }

    [HarmonyPatch(typeof(JobDriver_BeatFire), "MakeNewToils")]
    public static class Patch_ExtinguishFire_MakeNewToils
    {
        static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_BeatFire __instance)
        {
            Thing target = __instance.job.targetA.Thing;

            if (target != null && target.def.defName == "Seg_COTO_PhosphorFire")
            {
                foreach (var t in __result)
                    yield return t;

                Toil extinguish = new Toil();
                extinguish.initAction = () =>
                {
                    if (target.Spawned)
                        target.Destroy(DestroyMode.Vanish);
                };
                extinguish.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return extinguish;
                yield break;
            }

            foreach (var t in __result)
                yield return t;
        }
    }

    [HarmonyPatch(typeof(Fire), "DoComplexCalcs")]
    public static class Patch_Fire_DoComplexCalcs
    {
        static bool Prefix(Fire __instance)
        {
            return __instance.def.defName != "Seg_COTO_PhosphorFire";
        }
    }

    public class SEG_COTO_Radium_Verb_Shoot : Verb_Shoot
    {
        private static readonly HediffDef RadBuildupDef = HediffDef.Named("SEG_COTO_RadBuildup");

        protected override bool TryCastShot()
        {
            bool fired = base.TryCastShot();
            if (!fired)
                return false;

            if (!CasterIsPawn)
                return true;

            Pawn pawn = CasterPawn;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RadBuildupDef);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(RadBuildupDef, pawn);
                pawn.health.AddHediff(hediff);
            }

            hediff.Severity += 0.001f;

            return true;
        }
    }
    public class Verb_SEG_COTO_StunVerb : Verb_MeleeAttackDamage
    {
        protected override bool TryCastShot()
        {
            Pawn targetPawn = currentTarget.Pawn;
            if (targetPawn == null)
                return false;

            int stunDuration = 120; // ticks of stun (2 seconds)

            targetPawn.stances.stunner.StunFor(stunDuration, Caster);

            return true;
        }
    }

[HarmonyPatch(typeof(Fire), "TryAttachFire")]
public static class Patch_Fire_TryAttachFire
{
    static bool Prefix(Fire __instance, Thing t, ref bool __result)
    {
        if (__instance.def.defName != "Seg_COTO_PhosphorFire")
            return true;

        if (t == null)
            return true;

        if (t is Pawn)
        {
            __result = true;
            return false;
        }

        if (t.def.category == ThingCategory.Building)

        {
            __result = true;
            return false;
        }

        return true;
    }
}


[HarmonyPatch(typeof(Fire), "FireSpreadAndBurn")]
public static class Patch_Fire_FireSpreadAndBurn
{
    static bool Prefix(Fire __instance)
    {
        if (__instance.def.defName == "Seg_COTO_PhosphorFire")
            return false; // noooo dont kill my fire owo

        return true;
    }
}


}
