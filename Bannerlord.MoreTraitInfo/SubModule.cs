using Bannerlord.MoreTraitInfo.Patches;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.MoreTraitInfo
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly string Namespace = typeof(SubModule).Namespace;

        protected override void OnSubModuleLoad()
        {
            var harmony = new Harmony(Namespace);

            EncyclopediaHeroTraitVisualWidgetPatch.Apply(harmony);
#if LOWER_THAN_1_5
            EncyclopediaTraitItemVMPatch.Apply(harmony);
#else
            TraitEffectTooltipPatch.Apply(harmony);
#endif
            TraitLevelingHelperPatch.Apply(harmony);

            var uiExtender = UIExtender.Create(Namespace);
            uiExtender.Register(typeof(SubModule).Assembly);
            uiExtender.Enable();

            base.OnSubModuleLoad();
        }
    }
}