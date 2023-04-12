using Bannerlord.MoreTraitInfo.Patches;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.MoreTraitInfo
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            var harmony = new Harmony(typeof(SubModule).Namespace);

            CampaignUIHelperPatch.Apply(harmony);
            HeroTraitDeveloperPatch.Apply(harmony);

            base.OnSubModuleLoad();
        }
    }
}