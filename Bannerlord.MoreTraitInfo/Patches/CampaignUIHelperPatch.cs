using System.Text;
using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.MoreTraitInfo.Patches
{
    public static class CampaignUIHelperPatch
    {
        private static readonly TextObject NeutralTextObject = new TextObject("{=3PzgpFGq}Neutral");

        public static void Apply(Harmony harmony)
        {
            harmony.TryPatch(AccessTools2.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTraitTooltipText)),
                postfix: AccessTools2.Method(typeof(CampaignUIHelperPatch), nameof(GetTraitTooltipTextPostfix)));
        }

        private static void GetTraitTooltipTextPostfix(ref string __result, TraitObject traitObject, int traitValue)
        {
            var characterDevelopmentModel = Campaign.Current.Models.CharacterDevelopmentModel;

            var currentXp = Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(traitObject);
            var traitName = GameTexts.FindText("str_trait", traitObject.StringId.ToLower());

            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine();

            var currentTier = traitValue + MathF.Abs(traitObject.MinValue);
            if (traitValue > -2)
            {
                var lowerLevel = GetTraitLevelName(traitObject, currentTier - 1);
                var requiredXp = characterDevelopmentModel.GetTraitXpRequiredForTraitLevel(traitObject, traitValue - 1);

                var requiredXpLine = GetRequiredXpLine(traitName, lowerLevel, traitValue - 1, currentXp, requiredXp);
                builder.AppendLine(requiredXpLine);
            }

            if (traitValue < 2)
            {
                var higherLevel = GetTraitLevelName(traitObject, currentTier + 1);
                var requiredXp = characterDevelopmentModel.GetTraitXpRequiredForTraitLevel(traitObject, traitValue + 1);

                var requiredXpLine = GetRequiredXpLine(traitName, higherLevel, traitValue + 1, currentXp, requiredXp);
                builder.AppendLine(requiredXpLine);
            }

            __result += builder.ToString();
        }

        private static TextObject GetTraitLevelName(TraitObject trait, int tier)
        {
            if (tier == 2)
            {
                return NeutralTextObject;
            }

            return GameTexts.FindText($"str_trait_name_{trait.StringId.ToLower()}", tier.ToString());
        }

        private static string GetRequiredXpLine(TextObject traitName, TextObject targetLevel, int targetValue, int currentXp, int requiredXp)
        {
            return $"{targetLevel} ({traitName} {targetValue}): {currentXp}/{requiredXp}";
        }
    }
}
