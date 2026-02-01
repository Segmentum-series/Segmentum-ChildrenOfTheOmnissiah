using RimWorld;
using Verse;
using System.Text;
using System.Linq;
using Core40k;

namespace Seg
{
    public class RankDef : Core40k.RankDef
    {
        public int ImplantsRequired = 0;

        public override bool  RequirementMet(
            StringBuilder sb,
            Pawn pawn,
            CompRankInfo rankComp,
            RankCategoryDef currentCategory,
            out string reason)
        {
            if (ImplantsRequired > 0)
            {
                int installed = pawn.health.hediffSet.hediffs
                    .OfType<Hediff_AddedPart>()
                    .Count();

                if (installed < ImplantsRequired)
                {
                    sb.AppendLine($"Requires at least {ImplantsRequired} artificial parts (has {installed}).");
                }
            }

            return base.RequirementMet(sb, pawn, rankComp, currentCategory, out reason);
        }

        public override string BuildRankBonusString(StringBuilder sb)
        {

            return base.BuildRankBonusString(sb);
        }

        public override void UnlockRank(Pawn pawn, CompRankInfo rankComp)
        {
        }

        public override void RemoveRank(Pawn pawn, CompRankInfo rankComp)
        {
        }
    }
}
