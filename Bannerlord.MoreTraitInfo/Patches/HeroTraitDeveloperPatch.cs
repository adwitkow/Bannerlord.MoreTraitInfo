using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Library;

namespace Bannerlord.MoreTraitInfo.Patches
{
    public static class HeroTraitDeveloperPatch
    {
        public static void Apply(Harmony harmony)
        {
            harmony.TryPatch(AccessTools2.Method(typeof(HeroTraitDeveloper), nameof(HeroTraitDeveloper.AddTraitXp)),
                prefix: AccessTools2.Method(typeof(HeroTraitDeveloperPatch), nameof(AddTraitXpPrefix)));
        }

        public static void AddTraitXpPrefix(TraitObject trait, int xpAmount)
        {
            if (xpAmount == 0)
            {
                return;
            }

            var @operator = xpAmount > 0 ? '+' : '-';
            var rawValue = MathF.Abs(xpAmount);
            var color = xpAmount > 0 ? Colors.Green : Colors.Red;
            var message = new InformationMessage($"{@operator}{rawValue} {trait.Name}", color);

            InformationManager.DisplayMessage(message);
        }
    }
}
