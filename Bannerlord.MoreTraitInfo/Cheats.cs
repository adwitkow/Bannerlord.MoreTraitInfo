#if LOWER_THAN_1_3
using System.Linq;
#else
using TaleWorlds.ObjectSystem;
#endif
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;

namespace Bannerlord.MoreTraitInfo;

internal static class Cheats
{
    private static MethodInfo AddPlayerTraitXPAndLogEntryMethod = AccessTools.Method(
        typeof(TraitLevelingHelper), "AddPlayerTraitXPAndLogEntry");

    [CommandLineFunctionality.CommandLineArgumentFunction("add_trait_xp", "moretraitinfo")]
    public static string AddTraitXp(List<string> strings)
    {
        const string format = "Format is \"moretraitinfo.add_trait_xp [Trait] | [Value]\".";

        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
        {
            return CampaignCheats.ErrorType;
        }

        if (CampaignCheats.CheckHelp(strings))
        {
            return format;
        }
        var separatedNames = GetSeparatedNames(strings);
        if (separatedNames.Count < 2)
        {
            return format;
        }

        if (!int.TryParse(separatedNames[1], out var xp))
        {
            return $"Please enter a number\n{format}";
        }

        var traitResult = TryGetTrait(separatedNames[0]);
        if (!traitResult.IsSuccess)
        {
            return $"{traitResult.ErrorMessage}\n{format}";
        }

        var trait = traitResult.Value;
        AddPlayerTraitXPAndLogEntryMethod.Invoke(
            null,
            [trait, xp, ActionNotes.DefaultNote, null]);

        return $"Successfully added {xp} XP to {trait.Name}";
    }

    private static List<string> GetSeparatedNames(List<string> strings)
    {
#if LOWER_THAN_1_1
        return strings.Except(["|"]).ToList();
#elif LOWER_THAN_1_3
        return CampaignCheats.GetSeparatedNames(strings, "|");
#else
        return CampaignCheats.GetSeparatedNames(strings, true);
#endif
    }

    private static Result<TraitObject> TryGetTrait(string traitName)
    {
#if LOWER_THAN_1_3
        var trait = TraitObject.All.FirstOrDefault(item =>
        {
            var normalizedTraitName = item.Name.ToString().ToLowerInvariant().Replace(" ", "");
            var normalizedObjectName = traitName.ToLowerInvariant().Replace(" ", "");

            return string.Equals(
                normalizedTraitName,
                normalizedObjectName,
                System.StringComparison.InvariantCultureIgnoreCase)
                || item.StringId.ToLowerInvariant() == normalizedObjectName;
        });

        return trait is not null
            ? Result<TraitObject>.Ok(trait)
            : Result<TraitObject>.Error("Requested trait object could not be found");
#else
        return TryGetObject<TraitObject>(traitName);
    }

    private static Result<T> TryGetObject<T>(string objectName) where T : MBObjectBase
    {
        var success = CampaignCheats.TryGetObject<T>(objectName, out var result, out var error);

        return success
            ? Result<T>.Ok(result)
            : Result<T>.Error(error);
#endif
    }

    private readonly record struct Result<T>
    {
        [MemberNotNullWhen(false, nameof(ErrorMessage))]
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsSuccess => ErrorMessage is null;

        public string? ErrorMessage { get; init; }

        public T? Value { get; init; }

        public static Result<T> Ok(T value)
        {
            return new Result<T>() { Value = value };
        }

        public static Result<T> Error(string message)
        {
            return new Result<T>() { ErrorMessage = message };
        }
    }
}
