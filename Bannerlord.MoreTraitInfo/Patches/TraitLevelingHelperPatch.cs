using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;

namespace Bannerlord.MoreTraitInfo.Patches
{
    public class TraitLevelingHelperPatch
    {
        private static string Context = string.Empty;

        public static void Apply(Harmony harmony)
        {
            var traitLevelingHelperType = typeof(TraitLevelingHelper);
            var methods = traitLevelingHelperType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => !m.IsSpecialName);

            var contextMethod = AccessTools2.Method(typeof(TraitLevelingHelperPatch), nameof(ContextPrefix));
            foreach (var method in methods)
            {
                // This is a ghetto solution but it gets the job done
                harmony.TryPatch(method, prefix: contextMethod);
            }

            harmony.TryPatch(AccessTools2.Method(typeof(TraitLevelingHelper), "AddPlayerTraitXPAndLogEntry"),
                prefix: AccessTools2.Method(typeof(TraitLevelingHelperPatch), nameof(AddPlayerTraitXPAndLogEntryPrefix)),
                postfix: AccessTools2.Method(typeof(TraitLevelingHelperPatch), nameof(AddPlayerTraitXPAndLogEntryPostfix)));
        }

        public static void ContextPrefix(MethodBase __originalMethod)
        {
            Context = SpaceBeforeUppercase(__originalMethod.Name
                .Substring(2));
        }

        public static void AddPlayerTraitXPAndLogEntryPrefix(
            TraitObject trait,
            out int __state)
        {
            __state = Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(trait);
        }

        public static void AddPlayerTraitXPAndLogEntryPostfix(
            TraitObject trait,
            int __state)
        {
            // Report the XP the player actually received. The requested amount can differ:
            // the XP is clamped at the trait's limits, and other mods may scale, redirect or block it.
            var xpValue = Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(trait) - __state;
            if (xpValue == 0)
            {
                return;
            }

            char sign;
            Color color;
            if (xpValue < 0)
            {
                sign = '-';
                color = Colors.Red;
            }
            else
            {
                sign = '+';
                color = Colors.Green;
            }

            var rawValue = MathF.Abs(xpValue);
            var message = new InformationMessage($"{Context}: {sign}{rawValue} {trait.Name}", color);

            InformationManager.DisplayMessage(message);
        }

        private static string SpaceBeforeUppercase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return Regex.Replace(value, "(?<!^)([A-Z])", " $1");
        }
    }
}
