
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using taranchuk_lasers;



namespace seg
{
    public class ConversionBeamerBeam : LaserProjectile
    {
        private int DistanceDamage(Thing target)
        {
            if (launcher == null || target == null)
                return 0;

            float dist = Vector3.Distance(launcher.DrawPos, target.DrawPos);
            return Mathf.RoundToInt(dist);
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (hitThing != null && !blockedByShield)
            {
                int dmg = DistanceDamage(hitThing);

                DamageInfo dinfo = new DamageInfo(
                    this.def.projectile.damageDef,
                    dmg,
                    this.ArmorPenetration,
                    this.ExactRotation.eulerAngles.y,
                    this.launcher,
                    weapon: this.equipmentDef,
                    intendedTarget: this.intendedTarget.Thing
                );

                hitThing.TakeDamage(dinfo);
            }
            base.Impact(hitThing, blockedByShield);
        }
    }
}
