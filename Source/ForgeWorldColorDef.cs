using Core40k;
using UnityEngine;
using Verse;
using Genes40k;

namespace Seg
{
    public class ForgeWorldColorDef : ColourPresetDef
    {
        public GeneDef relatedChapterGene;
        public ShoulderIconDef relatedChapterIcon;
        public Color chapterIconColour = Color.white;
        public bool loyalist = true;
    }
}