using HarmonyLib;
using HarmonyLib.PatchBuilder;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Encyclopedia;

namespace Bannerlord.MoreTraitInfo.Patches;

public class EncyclopediaHeroTraitVisualWidgetPatch
{
    public static void Apply(Harmony harmony)
    {
        harmony.Patch<EncyclopediaHeroTraitVisualWidget>()
            .Method("SetVisual")
                .Postfix(EncyclopediaHeroTraitVisualWidget_SetVisual_Postfix);
    }

    private static void EncyclopediaHeroTraitVisualWidget_SetVisual_Postfix(
        ref EncyclopediaHeroTraitVisualWidget __instance, string traitCode, int value)
    {
        if (value == 0)
        {
            var sprite = $@"SPGeneral\SPTraits\{traitCode.ToLower()}_1";
            __instance.Sprite = __instance.Context.SpriteData.GetSprite(sprite);
            __instance.Color = new Color(0.33f, 0.33f, 0.33f, 0.8f);
        }
    }
}
