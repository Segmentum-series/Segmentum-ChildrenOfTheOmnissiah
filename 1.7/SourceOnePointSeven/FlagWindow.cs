using ColourPicker;
using Core40k;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Genes40k;

namespace Seg
{
    public class Dialog_ChangeForgeWorldFlagColour : Window
    {
        private readonly List<ForgeWorldColorDef> forgeWorldColours;
        private ForgeWorldColorDef currentlySelectedPreset;
        private Color currentlySelectedPrimaryColour;
        private Color currentlySelectedSecondaryColour;
        private string currentlySelectedIcon;
        private readonly Building_DecorativeForgeWorldFlag decoFlag;
        private List<FlagIconDef> flagIcons;
        private Vector2 scrollPos;
        private float scrollViewHeight;
        private const int RowAmount = 6;
        private const float gap = 5f;

        private Texture2D CurrentlySelectedIconTexture
        {
            get => ContentFinder<Texture2D>.Get(this.currentlySelectedIcon);
        }

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public Dialog_ChangeForgeWorldFlagColour(Building_DecorativeForgeWorldFlag decoFlag)
        {
            this.decoFlag = decoFlag;
            this.closeOnClickedOutside = true;
            this.currentlySelectedPreset = decoFlag.currentlySelectedPreset;
            this.currentlySelectedIcon = decoFlag.FlagInsigniaFilePath;
            this.currentlySelectedPrimaryColour = decoFlag.DrawColor;
            this.currentlySelectedSecondaryColour = decoFlag.DrawColorTwo;
            this.forgeWorldColours = DefDatabase<ForgeWorldColorDef>.AllDefsListForReading;
            this.flagIcons = DefDatabase<FlagIconDef>.AllDefs.OrderBy(f => f.sortOrder).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            inRect.xMin += 50f;
            inRect.xMax -= 50f;

            Rect rect1 = new Rect(inRect);
            rect1.height = 40f;
            rect1.width /= 2f;
            rect1.x += rect1.width / 2f;

            if (Widgets.ButtonText(rect1, "BEWH.MankindsFinest.ModSettings.ColourPreset".Translate(this.currentlySelectedPreset?.label ?? "Custom")))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                options.Add(new FloatMenuOption("BEWH.MankindsFinest.ModSettings.CustomColour".Translate(), () =>
                {
                    this.currentlySelectedPreset = null;
                    this.decoFlag.currentlySelectedPreset = null;
                }));

                foreach (ForgeWorldColorDef fw in this.forgeWorldColours)
                {
                    ForgeWorldColorDef colour = fw;
                    options.Add(new FloatMenuOption(colour.label.CapitalizeFirst(), () =>
                    {
                        this.currentlySelectedPreset = colour;
                        this.currentlySelectedPrimaryColour = colour.primaryColour;
                        this.currentlySelectedSecondaryColour = colour.secondaryColour;
                        this.currentlySelectedIcon = colour.relatedChapterIcon.iconPath;
                        this.decoFlag.currentlySelectedPreset = colour;
                    },
                    Core40kUtils.ThreeColourPreview(colour.primaryColour, colour.secondaryColour, colour.tertiaryColour, colour.colorAmount),
                    Color.white));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            Rect source = new Rect(inRect);
            source.y = rect1.yMax + 5f;
            source.height /= 3f;

            Rect rect2 = new Rect(source);
            rect2.width /= 2f;

            Rect rect3 = rect2.ContractedBy(5f);
            rect3.x = inRect.xMin + 1f;

            Widgets.DrawMenuSection(rect3.ContractedBy(-1f));
            Widgets.DrawRectFast(rect3, this.currentlySelectedPrimaryColour);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect3, "BEWH.Framework.Customization.PrimaryColor".Translate());
            TooltipHandler.TipRegion(rect3, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(rect3))
                Find.WindowStack.Add(new Dialog_ColourPicker(this.currentlySelectedPrimaryColour, newColour =>
                {
                    this.currentlySelectedPrimaryColour = newColour;
                    this.decoFlag.currentlySelectedPreset = null;
                }));

            Rect rect4 = new Rect(source);
            rect4.width /= 2f;
            rect4.x = rect3.xMax;

            Rect rect5 = rect4.ContractedBy(5f);
            rect5.x = inRect.xMax - rect5.width - 1f;

            Widgets.DrawMenuSection(rect5.ContractedBy(-1f));
            Widgets.DrawRectFast(rect5, this.currentlySelectedSecondaryColour);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect5, "BEWH.Framework.Customization.SecondaryColor".Translate());
            TooltipHandler.TipRegion(rect5, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(rect5))
                Find.WindowStack.Add(new Dialog_ColourPicker(this.currentlySelectedSecondaryColour, newColour =>
                {
                    this.currentlySelectedSecondaryColour = newColour;
                    this.decoFlag.currentlySelectedPreset = null;
                }));

            if (this.decoFlag.currentlySelectedPreset == null)
            {
                Rect rect6 = new Rect(source);
                rect6.height = rect1.height;
                rect6.width = rect1.width;
                rect6.x = rect1.x;
                rect6.y = source.yMax + 5f;

                Widgets.DrawMenuSection(rect6);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect6, "Custom Icon");
                Text.Anchor = TextAnchor.UpperLeft;

                float y = rect6.yMax + 5f;
                float num1 = inRect.yMax - rect6.yMax - Window.CloseButSize.y;

                Rect outRect = new Rect(inRect.x, y, inRect.width, num1);
                Rect rect7 = new Rect(inRect.x, y, inRect.width - 16f, Mathf.Max(this.scrollViewHeight, num1));

                this.scrollViewHeight = num1;

                Widgets.BeginScrollView(outRect, ref this.scrollPos, rect7);

                float num2 = rect7.width / 6f;
                Vector2 size = new Vector2(num2, num2);
                Vector2 position = new Vector2(rect7.x, y);
                float x = position.x;
                int num3 = 1;

                for (int index = 0; index < this.flagIcons.Count; index++)
                {
                    position = new Vector2(x, y);
                    Rect rect8 = new Rect(position, size);
                    x += rect8.width;

                    if (index != 0 && (index + 1) % 6 == 0)
                    {
                        y += rect8.height;
                        x = rect7.position.x;
                        num3++;
                    }

                    rect8 = rect8.ContractedBy(5f);

                    if (this.currentlySelectedIcon == this.flagIcons[index].iconPath)
                        Widgets.DrawStrongHighlight(rect8.ExpandedBy(3f));

                    GUI.color = Mouse.IsOver(rect8) ? GenUI.MouseoverColor : Color.white;
                    GUI.DrawTexture(rect8, Command.BGTexShrunk);
                    GUI.color = Color.white;
                    GUI.DrawTexture(rect8, this.flagIcons[index].Icon);

                    TooltipHandler.TipRegion(rect8, this.flagIcons[index].label);

                    if (Widgets.ButtonInvisible(rect8))
                        this.currentlySelectedIcon = this.flagIcons[index].iconPath;
                }

                this.scrollViewHeight = num3 * num2 + 5f;
                Widgets.EndScrollView();
            }
            else
            {
                float num = inRect.width / 3f;
                Rect position = new Rect(source);
                position.height = num;
                position.width = num;
                position.x = inRect.x + num;
                position.y = source.yMax + 20f;

                GUI.DrawTexture(position, Command.BGTexShrunk);
                GUI.DrawTexture(position, this.CurrentlySelectedIconTexture);
            }

            if (Widgets.ButtonText(new Rect(inRect.xMax - Window.CloseButSize.x, inRect.yMax - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "Accept".Translate()))
            {
                this.decoFlag.SetPrimaryColor(this.currentlySelectedPrimaryColour);
                this.decoFlag.SetSecondaryColor(this.currentlySelectedSecondaryColour);
                this.decoFlag.SetFlagInsignia(this.currentlySelectedIcon);
                this.decoFlag.SetOriginals();
                this.decoFlag.Notify_ColorChanged();
                Graphic graphic = this.decoFlag.Graphic;
                this.Close();
            }

            if (Widgets.ButtonText(new Rect(inRect.xMin, inRect.yMax - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "Close".Translate()))
            {
                this.decoFlag.Reset();
                this.Close();
            }
        }

        public override void Notify_ClickOutsideWindow()
        {
            this.decoFlag.Reset();
            base.Notify_ClickOutsideWindow();
        }
    }
}