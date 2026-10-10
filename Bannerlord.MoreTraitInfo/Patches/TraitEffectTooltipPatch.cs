#if !LOWER_THAN_1_5
using HarmonyLib;
using HarmonyLib.PatchBuilder;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core.ViewModelCollection.Information;

namespace Bannerlord.MoreTraitInfo.Patches;

public class TraitEffectTooltipPatch
{
    private static MethodInfo GetTraitLevelNameMethod = null!;

    private static bool _buildingMainHeroTooltip;

    public static void Apply(Harmony harmony)
    {
        GetTraitLevelNameMethod = AccessTools.Method(
            typeof(CampaignUIHelper),
            "GetTraitLevelName");

        harmony.Patch()
            .Method(() => CampaignUIHelper.GetTraitEffectTooltip(default, default))
                .Transpiler(CampaignUIHelper_GetTraitEffectTooltip_Transpiler);
        harmony.Patch<EncyclopediaTraitItemVM>()
            .Constructor([typeof(TraitObject), typeof(Hero)])
                .Postfix(EncyclopediaTraitItemVM_Constructor_Postfix);
    }

    private static void EncyclopediaTraitItemVM_Constructor_Postfix(EncyclopediaTraitItemVM __instance, TraitObject traitObj, Hero hero)
    {
        if (hero != Hero.MainHero)
        {
            return;
        }

        __instance.Hint = new BasicTooltipViewModel(() =>
        {
            _buildingMainHeroTooltip = true;
            try
            {
                return CampaignUIHelper.GetTraitEffectTooltip(traitObj, __instance.Value);
            }
            finally
            {
                _buildingMainHeroTooltip = false;
            }
        });
    }

    private static IEnumerable<CodeInstruction> CampaignUIHelper_GetTraitEffectTooltip_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var maxValue = AccessTools.PropertyGetter(typeof(TraitObject), nameof(TraitObject.MaxValue));

        var matcher = new CodeMatcher(instructions);

        // callvirt get_MaxValue, ble.s  -> end of the vanilla list2 loop
        matcher.MatchStartForward(new CodeMatch(OpCodes.Callvirt, maxValue));

        if (matcher.IsInvalid)
        {
            return matcher.InstructionEnumeration();
        }

        return matcher
            .Advance(2) // first instruction after the loop
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_1),   // list2 (vanilla)
                new CodeInstruction(OpCodes.Ldarg_0),   // traitObject
                new CodeInstruction(OpCodes.Ldarg_1),   // traitValue
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(
                        typeof(TraitEffectTooltipPatch),
                        nameof(ReplaceLevelsForMainHero))),
                new CodeInstruction(OpCodes.Stloc_1))
            .InstructionEnumeration();
    }

    private static List<ValueTuple<string, bool>> ReplaceLevelsForMainHero(
        List<ValueTuple<string, bool>> vanillaList, TraitObject trait, int traitValue)
    {
        return _buildingMainHeroTooltip
            ? BuildTraitLevelRoadmap(trait, traitValue)
            : vanillaList;
    }

    private static List<ValueTuple<string, bool>> BuildTraitLevelRoadmap(TraitObject trait, int traitValue)
    {
        var currentXp = Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(trait);

        var list = new List<ValueTuple<string, bool>>();

        for (int traitLevel = trait.MinValue; traitLevel <= trait.MaxValue; traitLevel++)
        {
            var traitName = (string)GetTraitLevelNameMethod.Invoke(null, [trait, traitLevel]);
            var traitLevelExp = GetTraitXpRoadmapBreakpoint(trait, traitLevel, currentXp);
            var roadMapElement = $"{traitLevelExp}\n{traitName}";

            var isCurrentLevel = traitLevel == traitValue;
            list.Add(new ValueTuple<string, bool>(roadMapElement, isCurrentLevel));
        }

        return list;
    }

    private static int GetTraitXpRoadmapBreakpoint(TraitObject trait, int targetLevel, int currentXp)
    {
        if (targetLevel < trait.MinValue || targetLevel > trait.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(targetLevel));
        }

        int currentLevel = GetRoadmapLevelFromXp(trait, currentXp);

        // Target is the current level: show the real XP.
        if (targetLevel == currentLevel)
        {
            return currentXp;
        }

        // Target is below the current level: highest XP inside the target's range.
        if (targetLevel < currentLevel)
        {
            return targetLevel >= 0
                ? GetTraitXpRequiredForTraitLevel(trait, targetLevel + 1) - 1  // next threshold is exclusive
                : GetTraitXpRequiredForTraitLevel(trait, targetLevel);         // negative thresholds are inclusive
        }

        // Target is above the current level: lowest XP inside the target's range.
        return targetLevel >= 1
            ? GetTraitXpRequiredForTraitLevel(trait, targetLevel)              // positive thresholds are inclusive
            : GetTraitXpRequiredForTraitLevel(trait, targetLevel - 1) + 1;     // negative thresholds are exclusive from above
    }

    private static int GetRoadmapLevelFromXp(TraitObject trait, int xp)
    {
        int level;

        if (xp >= GetTraitXpRequiredForTraitLevel(trait, 1))
        {
            // Positive side: a level covers T(level) <= xp < T(level + 1).
            level = 1;
            while (level < trait.MaxValue && xp >= GetTraitXpRequiredForTraitLevel(trait, level + 1))
            {
                level++;
            }
        }
        else if (xp > GetTraitXpRequiredForTraitLevel(trait, -1))
        {
            // Neutral level: T(-1) < xp < T(1).
            level = 0;
        }
        else
        {
            // Negative side: a level covers T(level - 1) < xp <= T(level).
            level = -1;
            while (level > trait.MinValue && xp <= GetTraitXpRequiredForTraitLevel(trait, level - 1))
            {
                level--;
            }
        }

        return Math.Max(trait.MinValue, Math.Min(trait.MaxValue, level));
    }

    private static int GetTraitXpRequiredForTraitLevel(TraitObject trait, int traitLevel)
    {
        return Campaign.Current.Models.CharacterDevelopmentModel.GetTraitXpRequiredForTraitLevel(trait, traitLevel);
    }
}
#endif