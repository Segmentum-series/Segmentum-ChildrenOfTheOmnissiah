using Core40k;
using UnityEngine;
using Verse;
using Genes40k;
using System.Collections.Generic;
using Seg;

namespace Seg
{
      [StaticConstructorOnStartup]
    public class Building_DecorativeForgeWorldFlag : Building
    {
        private Genes40kModSettings modSettings;
        private Color drawColorOne;
        private Color originalColorOne;
        private Color drawColorTwo;
        private Color originalColorTwo;

        public ForgeWorldColorDef currentlySelectedPreset;

        private static readonly CachedTexture EditFlagIcon = new CachedTexture("UI/Gizmos/BEWH_CogIcon");

        [Unsaved(false)]
        private Graphic flagInsigniaGraphic;

        private string originalFlagInsigniaFilePath = "UI/Decoration/LegionBadges/BEWH_iconUI_Aquila";
        private string flagInsigniaFilePath;

        private const string NoIcon = "UI/Decoration/LegionBadges/BEWH_NoneSingle";

        private Genes40kModSettings ModSettings =>
            modSettings ?? (modSettings = LoadedModManager.GetMod<Genes40kMod>().GetSettings<Genes40kModSettings>());

       public Building_DecorativeForgeWorldFlag()
                    {
                        var defaultPreset = DefDatabase<ForgeWorldColorDef>.GetNamedSilentFail("Seg_COTO_ForgeColorMarsFlag");

                        drawColorOne = defaultPreset?.primaryColour ?? Color.white;
                        originalColorOne = drawColorOne;

                        drawColorTwo = defaultPreset?.secondaryColour ?? Color.white;
                        originalColorTwo = drawColorTwo;

                        flagInsigniaFilePath =
                            defaultPreset?.relatedChapterIcon?.iconPath
                            ?? originalFlagInsigniaFilePath;

                        currentlySelectedPreset = defaultPreset;
                    }
        public override Color DrawColor => drawColorOne;
        public override Color DrawColorTwo => drawColorTwo;

        public override Graphic Graphic => GetGraphic();

        private Graphic GetGraphic()
        {
            string maskPath = def.graphicData.maskPath;
            Shader shader = def.graphicData.shaderType?.Shader ?? ShaderDatabase.CutoutComplex;

            return GraphicDatabase.Get<Graphic_Single>(
                def.graphicData.texPath,
                shader,
                def.graphicData.drawSize,
                DrawColor,
                DrawColorTwo,
                def.graphicData,
                maskPath
            );
        }

        public string FlagInsigniaFilePath => flagInsigniaFilePath;

        private Graphic FlagInsigniaGraphic =>
            flagInsigniaGraphic ??
            (flagInsigniaGraphic = GraphicDatabase.Get<Graphic_Single>(
                flagInsigniaFilePath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white
            ));

        public void SetFlagInsignia(string path, bool noIcon = false)
        {
            flagInsigniaFilePath = noIcon ? NoIcon : path;
            flagInsigniaGraphic = GraphicDatabase.Get<Graphic_Single>(
                flagInsigniaFilePath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white
            );
            Notify_ColorChanged();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            Vector3 loc = drawLoc;
            loc.y += 0.1f;
            loc.z += 0.8f;

            FlagInsigniaGraphic.DrawFromDef(loc, Rot4.North, null);
        }
    public override IEnumerable<Gizmo> GetGizmos()
            {
                foreach (var g in base.GetGizmos())
                    yield return g;

                yield return new Command_Action
                {
                    defaultLabel = "BEWH.MankindsFinest.Decorations.EditFlag".Translate() + "...",
                    defaultDesc = "BEWH.MankindsFinest.Decorations.EditFlagDesc".Translate(),
                    icon = Building_DecorativeForgeWorldFlag.EditFlagIcon.Texture,
                    action = () => Find.WindowStack.Add(new Dialog_ChangeForgeWorldFlagColour(this))
                };
            }
     private void OpenForgeWorldFlagEditor()
            {
                // Primary color picker
                Find.WindowStack.Add(new ColourPicker.Dialog_ColourPicker(
                    drawColorOne,
                    c => SetPrimaryColor(c)
                ));

                // Secondary color picker
                Find.WindowStack.Add(new ColourPicker.Dialog_ColourPicker(
                    drawColorTwo,
                    c => SetSecondaryColor(c)
                ));
            }
        public void SetOriginals()
        {
            originalFlagInsigniaFilePath = flagInsigniaFilePath;
            originalColorOne = drawColorOne;
            originalColorTwo = drawColorTwo;
        }

        public void Reset()
        {
            flagInsigniaFilePath = originalFlagInsigniaFilePath;
            drawColorOne = originalColorOne;
            drawColorTwo = originalColorTwo;
            Notify_ColorChanged();
        }

        public void SetPrimaryColor(Color color)
        {
            drawColorOne = color;
            Notify_ColorChanged();
        }

        public void SetSecondaryColor(Color color)
        {
            drawColorTwo = color;
            Notify_ColorChanged();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref originalColorOne, "originalColorOne", Color.white);
            Scribe_Values.Look(ref originalColorTwo, "originalColorTwo", Color.white);
            Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.white);
            Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);

            Scribe_Values.Look(ref flagInsigniaFilePath, "flagInsigniaFilePath");
            Scribe_Values.Look(ref originalFlagInsigniaFilePath, "originalFlagInsigniaFilePath");

            Scribe_Defs.Look(ref currentlySelectedPreset, "currentlySelectedPreset");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                Notify_ColorChanged();
        }
    }
}
