using Verse;
using RimWorld;
using UnityEngine;

namespace SegCOTO
{
    public class Thing_PhosphorFire : Fire
    {
        static Thing_PhosphorFire()
        {
            Log.Message("SegCOTO: Thing_PhosphorFire class loaded successfully. Thank the Omnissiah!");
        }
    }

    public class CompProperties_PhosphorFire : CompProperties
    {
        public CompProperties_PhosphorFire()
        {
            this.compClass = typeof(CompPhosphorFire);
        }
    }

    public class CompPhosphorFire : ThingComp
    {
        private int phosphorTickCounter;

        public CompProperties_PhosphorFire Props
        {
            get { return (CompProperties_PhosphorFire)this.props; }
        }

        public override void CompTick()
        {
            base.CompTick();
            phosphorTickCounter++;
        }
    }
}

