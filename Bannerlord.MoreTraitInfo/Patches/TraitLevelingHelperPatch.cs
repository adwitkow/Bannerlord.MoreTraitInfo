using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
                postfix: AccessTools2.Method(typeof(TraitLevelingHelperPatch), nameof(AddPlayerTraitXPAndLogEntryPostfix)));
        }

        public static void ContextPrefix(MethodBase __originalMethod)
        {
            Context = SpaceBeforeUppercase(__originalMethod.Name
                .Substring(2));
        }

        public static void AddPlayerTraitXPAndLogEntryPostfix(
            TraitObject trait,
            int xpValue)
        {
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
