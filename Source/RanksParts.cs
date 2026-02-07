using RimWorld;
using Verse;
using System.Text;
using System.Linq;
using Core40k;
using System.Collections.Generic;


namespace Seg
{
    public class RankDef : Core40k.RankDef
    {
        public int ImplantsRequired = 0;

        public override bool RequirementMet(
            StringBuilder sb,
            Pawn pawn,
            CompRankInfo rankComp,
            RankCategoryDef currentCategory,
            out string reason)
        {
            bool valid = true;

            if (ImplantsRequired > 0)
            {
                int installed = pawn.health.hediffSet.hediffs
                    .Count(h => h is Hediff_AddedPart || h is Hediff_Implant);

                if (installed < ImplantsRequired)
                {
                    valid = false;
                    sb.AppendLine($"Requires at least {ImplantsRequired} artificial parts (has {installed}).");
                }
            }

            bool baseResult = base.RequirementMet(sb, pawn, rankComp, currentCategory, out reason);
            return valid && baseResult;
        }

        public override string BuildRankBonusString(StringBuilder sb)
        {
            return base.BuildRankBonusString(sb);
        }

        public override void UnlockRank(CompRankInfo rankComp)
        {
            base.UnlockRank(rankComp);
        }

        public override void RemoveRank(CompRankInfo rankComp)
        {
            base.RemoveRank(rankComp);
        }
    }

}





