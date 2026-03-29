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
            HediffDef waveburstHediff = DefDatabase<HediffDef>.GetNamed("Seg_COTO_WaveburstEffect");
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

                    if (waveburstHediff != null)
                    {
                        Hediff h = pawn.health.AddHediff(waveburstHediff);
                        if (h != null && h.Severity <= 0f)
                            h.Severity = 1f;
                    }

                    if (pawn.stances?.stunner != null)
                        pawn.stances.stunner.StunFor(ticks, (Thing)this.CasterPawn);
                }
            }
        }
    }

     public class Ability_ExtractGene : VEF.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (targets == null || targets.Length == 0) return;
            Pawn targetPawn = targets[0].Thing as Pawn;
            if (targetPawn == null || targetPawn.genes == null) return;

            var availableGenes = targetPawn.genes.GenesListForReading
                .Where(g => g.def.biostatArc == 0)
                .Select(g => g.def)
                .Distinct()
                .ToList();

            if (!availableGenes.Any())
            {
                Messages.Message("PawnHasNoNonArchiteGenes".Translate(targetPawn.Named("PAWN")), MessageTypeDefOf.RejectInput);
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (var geneDef in availableGenes)
            {
                var label = geneDef.label.CapitalizeFirst();
                options.Add(new FloatMenuOption(label, () =>
                {
                    Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                    genepack.Initialize(new List<GeneDef> { geneDef });
                    IntVec3 dropCell = this.CasterPawn.Position;
                    GenPlace.TryPlaceThing((Thing)genepack, dropCell, this.CasterPawn.Map, ThingPlaceMode.Near);
                    GeneUtility.ExtractXenogerm(targetPawn, Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange));
                    Messages.Message("GeneExtractionComplete".Translate(targetPawn.Named("PAWN")) + ": " + geneDef.label, MessageTypeDefOf.PositiveEvent);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

}
