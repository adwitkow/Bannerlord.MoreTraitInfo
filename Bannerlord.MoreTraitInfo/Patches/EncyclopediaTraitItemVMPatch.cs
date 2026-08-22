using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.MoreTraitInfo.Patches
{
    [HarmonyPatch(typeof(EncyclopediaTraitItemVM), MethodType.Constructor, typeof(TraitObject), typeof(Hero))]
    public static class EncyclopediaTraitItemVMPatch
    {
        private static readonly TextObject NeutralTextObject = new TextObject("{=3PzgpFGq}Neutral");

        private static void Postfix(ref EncyclopediaTraitItemVM __instance, TraitObject traitObj, Hero hero)
        {
            if (hero != Hero.MainHero)
            {
                return;
            }

            var characterDevelopmentModel = Campaign.Current.Models.CharacterDevelopmentModel;

            var traitValue = hero.GetTraitLevel(traitObj);
            var currentXp = Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(traitObj);
            var traitName = GameTexts.FindText("str_trait", traitObj.StringId.ToLower());

            var currentTier = traitValue + MathF.Abs(traitObj.MinValue);

#if LOWER_THAN_1_5
            var builder = new StringBuilder();
            builder.AppendLine();

            if (traitValue > -2)
            {
                builder.AppendLine();

                var lowerLevel = GetTraitLevelName(traitObj, currentTier - 1);
                var requiredXp = characterDevelopmentModel.GetTraitXpRequiredForTraitLevel(traitObj, traitValue - 1);

                var requiredXpLine = GetRequiredXpLine(traitName, lowerLevel, traitValue - 1, currentXp, requiredXp);
                builder.Append(requiredXpLine);
            }

            if (traitValue < 2)
            {
                builder.AppendLine();
                var higherLevel = GetTraitLevelName(traitObj, currentTier + 1);
                var requiredXp = characterDevelopmentModel.GetTraitXpRequiredForTraitLevel(traitObj, traitValue + 1);

                var requiredXpLine = GetRequiredXpLine(traitName, higherLevel, traitValue + 1, currentXp, requiredXp);
                builder.Append(requiredXpLine);
            }

            var traitTooltipText = CampaignUIHelper.GetTraitTooltipText(traitObj, traitValue);
            __instance.Hint = new HintViewModel(new TextObject("{=!}" + traitTooltipText + builder.ToString()));
#else
            var traitXpProperties = new List<TooltipProperty>();
            if (traitValue > -2)
            {
                var lowerLevel = GetTraitLevelName(traitObj, currentTier - 1);
                var requiredXp = characterDevelopmentModel.GetTraitXpRequiredForTraitLevel(traitObj, traitValue - 1);

                traitXpProperties.Add(new(
                    $"{lowerLevel} ({traitName} {traitValue - 1})",
                    $"{currentXp}/{requiredXp}",
                    0));
            }
            
            if (traitValue < 2)
            {
                var higherLevel = GetTraitLevelName(traitObj, currentTier + 1);
                var requiredXp = characterDevelopmentModel.GetTraitXpRequiredForTraitLevel(traitObj, traitValue + 1);

                var requiredXpLine = GetRequiredXpLine(traitName, higherLevel, traitValue + 1, currentXp, requiredXp);
                traitXpProperties.Add(new(
                    $"{higherLevel} ({traitName} {traitValue + 1})",
                    $"{currentXp}/{requiredXp}",
                    0));
            }

            if (traitXpProperties.Count > 0)
            {
                traitXpProperties.InsertRange(0,
                    [
                        new(
                            string.Empty,
                            string.Empty,
                            0,
                            false,
                            TooltipProperty.TooltipPropertyFlags.None),
                        new(
                            string.Empty,
                            string.Empty,
                            0,
                            false,
                            TooltipProperty.TooltipPropertyFlags.RundownSeperator)
                    ]);
            }

            var allTooltipProperties = CampaignUIHelper.GetTraitEffectTooltip(traitObj, traitValue);
            allTooltipProperties.AddRange(traitXpProperties);

            __instance.Hint = new BasicTooltipViewModel(() => allTooltipProperties);
#endif
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
