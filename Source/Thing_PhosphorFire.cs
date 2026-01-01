using RimWorld;
using Verse;
using UnityEngine;

namespace Seg.COTO
{
    public class PhosphorFire : ThingWithComps
    {
        private int ageTicks;
        private const int LifetimeTicks = 10000;
        private float fireSize = 1.0f;

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
                DoDamageAndHeat();

            if (this.IsHashIntervalTick(120))
                TrySpread();
        }

        private void DoDamageAndHeat()
        {
            var list = Map.thingGrid.ThingsListAt(Position);
            for (int i = 0; i < list.Count; i++)
            {
                Thing t = list[i];
                if (t == this)
                    continue;

                if (t.def.category == ThingCategory.Mote)
                    continue;

                float dmg = 5f;
                t.TakeDamage(new DamageInfo(DamageDefOf.Flame, dmg, instigator: this));
            }

            float energy = fireSize * 160f;
            GenTemperature.PushHeat(Position, Map, energy);

            if (Rand.Value < 0.4f)
                WeatherBuildupUtility.AddSnowRadial(Position, Map, fireSize * 3f, -(fireSize * 0.1f));
        }

        private void TrySpread()
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
}
