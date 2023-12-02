using HarmonyLib.BUTR.Extensions;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;
using Humanizer;
using System.Reflection;
using System.Linq;

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
            Context = __originalMethod.Name
                .Substring(2)
                .Humanize(LetterCasing.Title);
        }

        public static void AddPlayerTraitXPAndLogEntryPostfix(
            TraitObject trait,
            int xpValue)
        {
            var @operator = xpValue < 0 ? '-' : '+';
            var rawValue = MathF.Abs(xpValue);
            var color = xpValue > 0 ? Colors.Green : Colors.Red;
            var message = new InformationMessage($"{Context}: {@operator}{rawValue} {trait.Name}", color);

            InformationManager.DisplayMessage(message);
        }
    }
}
