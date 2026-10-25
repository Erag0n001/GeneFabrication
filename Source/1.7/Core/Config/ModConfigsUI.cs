using System.Globalization;
using GeneFabrication.UI;
using UnityEngine;
using Verse;

namespace GeneFabrication.Core.Config;

public class ModConfigsUI : Mod
{
	public ModConfigsUI(ModContentPack content) : base(content)
	{
		_ = GetSettings<ModConfigs>();
	}
		
	public override string SettingsCategory() => "Gene Fabrication";

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Listing_Standard listingStandard = new Listing_Standard();
		listingStandard.Begin(inRect);
		listingStandard.Label("GeneFab_NeutroamineMultiplier".Translate());
		var sliderText = (ModConfigs.NeutroamineMultiplier * 100).ToString(CultureInfo.InvariantCulture) + "%";
		ModConfigs.NeutroamineMultiplier = listingStandard.SliderLabeled(sliderText,ModConfigs.NeutroamineMultiplier, 0.1f, 3f);
		listingStandard.GapLine();
		listingStandard.Label("GeneFab_ArchiteMultiplier".Translate());
		var architeSliderText = (ModConfigs.ArchiteMultiplier * 100).ToString(CultureInfo.InvariantCulture) + "%";
		ModConfigs.ArchiteMultiplier = listingStandard.SliderLabeled(architeSliderText, ModConfigs.ArchiteMultiplier, 0.1f, 3f);
		listingStandard.GapLine();
		listingStandard.Label("GeneFed_WorkMultiplier".Translate());
		var workMultiplierText = (ModConfigs.WorkMultiplier * 100).ToString(CultureInfo.InvariantCulture) + "%";
		ModConfigs.WorkMultiplier = listingStandard.SliderLabeled(workMultiplierText, ModConfigs.WorkMultiplier, 0.1f, 3f);
		listingStandard.GapLine();
		if (listingStandard.ButtonTextLabeled("GeneFab_BlacklistedButton".Translate(), "GeneFab_OpenBlacklistMenu".Translate())) {
			Window window = ModConfigs.LegacyUI ? new GeneUI(null, GeneUIMode.Blacklist) : new GenePicker(null, GeneUIMode.Blacklist);
			Find.WindowStack.Add(window);
		}
		listingStandard.CheckboxLabeled("GeneFab_LegacyUI".Translate(), ref ModConfigs.LegacyUI, "GeneFab_LegacyUI_Tooltip".Translate());
		listingStandard.CheckboxLabeled("GeneFab_DisableUI".Translate(), ref ModConfigs.DisableUI, "GeneFab_DisableUI_Tooltip".Translate());
		if(GeneFarbication.IsVEStarJackEnabled) {
			listingStandard.CheckboxLabeled("GeneFab_BanAstrogenes".Translate(),
				ref ModConfigs.IgnoreAstrogenes, 
				"GeneFab_BanAstrogenes_Tooltip".Translate());
		}
		listingStandard.End();
	}

	public override void WriteSettings()
	{
		base.WriteSettings();
		foreach(RecipeDef def in DefDatabase<RecipeDef>.AllDefs)
		{
			if (def.defName.Contains("Make_Genepack_"))
			{
				string temp = def.defName.Substring(def.defName.IndexOf("_") + 1);
				GeneDef gene = DefDatabase<GeneDef>.GetNamed(temp.Substring(temp.IndexOf("_") + 1));
				def.ingredients = RecipeDefGenerator.CalcIngredients(gene);
				def.workAmount = RecipeDefGenerator.CalcWorkAmount(gene);
			}
		}
	}
}