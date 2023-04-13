using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.MoreTraitInfo.Mixins
{
    [ViewModelMixin(nameof(EncyclopediaHeroPageVM.RefreshValues))]
    internal class EncyclopediaHeroPageVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private static readonly TextObject TraitsTextObject = new TextObject("{=7NnPxNXZ}Personality Traits");

        private readonly Hero? _hero;

        public EncyclopediaHeroPageVMMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            _hero = vm.Obj as Hero;

            TraitsText = TraitsTextObject.ToString();
            TraitXpInfo = new MBBindingList<StringPairItemVM>();
        }

        [DataSourceProperty]
        public string TraitsText { get; set; }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> TraitXpInfo { get; set; }

        public override void OnRefresh()
        {
            TraitXpInfo.Clear();
            TraitsText = TraitsTextObject.ToString();

            if (ViewModel is null || _hero is null || _hero != Hero.MainHero)
            {
                return;
            }

            foreach (var trait in CampaignUIHelper.GetHeroTraits())
            {
                var xp = Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(trait);
                var pair = new StringPairItemVM($"{trait.Name}:", xp.ToString());

                TraitXpInfo.Add(pair);
            }

            base.OnRefresh();
        }
    }
}
