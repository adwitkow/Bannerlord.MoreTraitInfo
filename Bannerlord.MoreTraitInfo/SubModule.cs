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

            HeroTraitDeveloperPatch.Apply(harmony);
            harmony.PatchAll();

            base.OnSubModuleLoad();
        }
    }
}