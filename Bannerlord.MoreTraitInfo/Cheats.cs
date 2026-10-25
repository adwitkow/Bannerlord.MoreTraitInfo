using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

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

        List<string> separatedNames = CampaignCheats.GetSeparatedNames(strings, true);
        if (separatedNames.Count < 2)
        {
            return format;
        }

        if (!int.TryParse(separatedNames[1], out var xp))
        {
            return $"Please enter a number\n{format}";
        }

        var traitResult = TryGetObject<TraitObject>(separatedNames[0]);
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

    private static Result<T> TryGetObject<T>(string objectName) where T : MBObjectBase
    {
        var success = CampaignCheats.TryGetObject<T>(objectName, out var result, out var error);

        return success
            ? Result<T>.Ok(result)
            : Result<T>.Error(error);
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
