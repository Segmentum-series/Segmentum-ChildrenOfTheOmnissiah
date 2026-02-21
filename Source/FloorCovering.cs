using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Seg
{
 public class Building_Seg_COTOFloorCover : Building
    {
        private float rotation;
        private Graphic cachedGraphic;
        private Gizmo cachedGraphicGizmo;

        public override void PostMake()
        {
            base.PostMake();
            overrideGraphicIndex = thingIDNumber;
            if (Graphic is Graphic_Random graphic)
                overrideGraphicIndex %= graphic.SubGraphicsCount;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rotation, "Seg_COTO_rotation");
            Scribe_Values.Look(ref overrideGraphicIndex, "Seg_COTO_overrideGraphicIndex");
        }

        private void ChangeGraphic()
        {
            if (overrideGraphicIndex == null) overrideGraphicIndex = 0;
            overrideGraphicIndex++;
            if (Graphic is Graphic_Random graphic) overrideGraphicIndex %= graphic.SubGraphicsCount;
            ClearCache();
        }

        private void ClearCache()
        {
            cachedGraphic = null;
            if (Map != null) DirtyMapMesh(Map);
        }

        public override void Notify_ColorChanged()
        {
            base.Notify_ColorChanged();
            ClearCache();
        }

        public override Graphic Graphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    Graphic g = def.graphicData.GraphicColoredFor(this);
                    cachedGraphic = g is Graphic_Random gr ? gr.SubGraphicFor(this) : g;
                }
                return cachedGraphic;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos()) yield return g;

            if (cachedGraphicGizmo == null)
            {
                Texture2D icon = ContentFinder<Texture2D>.Get("UI/Abilities/Seg_COTO_Ability_EmergencyAid", true);
                cachedGraphicGizmo = new Command_Action
                {
                    defaultLabel = "Change appearance",
                    defaultDesc = "Cycle through the available emblem variants.",
                    icon = icon,
                    action = ChangeGraphic
                };
            }

            yield return cachedGraphicGizmo;
        }
    }
}