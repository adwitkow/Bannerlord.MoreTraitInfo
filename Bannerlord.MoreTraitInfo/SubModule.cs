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

            HeroTraitDeveloperPatch.Apply(harmony);
            harmony.PatchAll();

            var uiExtender = new UIExtender(Namespace);
            uiExtender.Register(typeof(SubModule).Assembly);
            uiExtender.Enable();

            base.OnSubModuleLoad();
        }
    }
}