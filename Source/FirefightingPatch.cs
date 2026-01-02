using Verse;
using Verse.AI;
using RimWorld;
using System.Reflection;
using HarmonyLib;
using System.Collections.Generic;

namespace Seg.COTO
{
    [HarmonyPatch]
    public static class Patch_FightFires_PotentialWorkThingRequest
    {
        static MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("RimWorld.WorkGiver_FightFires");
            return AccessTools.Property(t, "PotentialWorkThingRequest").GetGetMethod(true);
        }

        static void Postfix(ref ThingRequest __result)
        {
            __result = ThingRequest.ForGroup(ThingRequestGroup.Everything);
        }
    }

    [HarmonyPatch]
    public static class Patch_FightFires_HasJobOnThing
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("RimWorld.WorkGiver_FightFires"),
                "HasJobOnThing"
            );
        }

        static void Postfix(ref bool __result, Pawn pawn, Thing t, bool forced)
        {
            if (__result) return;
            if (t == null) return;
            if (t.def.defName != "Seg_COTO_PhosphorFire") return;
            if (!t.Spawned) return;
            if (pawn == null) return;
            if (pawn.WorkTagIsDisabled(WorkTags.Firefighting)) return;
            if (!pawn.CanReserve(t)) return;
            __result = true;
        }
    }

    [HarmonyPatch]
    public static class Patch_FightFires_JobOnThing
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("RimWorld.WorkGiver_FightFires"),
                "JobOnThing"
            );
        }

        static void Postfix(ref Job __result, Pawn pawn, Thing t, bool forced)
        {
            if (__result != null) return;
            if (t == null) return;
            if (t.def.defName != "Seg_COTO_PhosphorFire") return;
            if (!pawn.CanReserve(t)) return;
            __result = JobMaker.MakeJob(JobDefOf.BeatFire, t);
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
}
