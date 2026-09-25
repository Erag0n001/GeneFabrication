using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GeneFarbication
{
	public class ModConfigsUI : Mod
	{
		static public ModConfigs config;

		public ModConfigsUI(ModContentPack content) : base(content)
		{
			config = GetSettings<ModConfigs>();
		}
		public override string SettingsCategory() => "Gene Fabrication";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.Label("GeneFab_NeutroamineMultiplier".Translate());
			config.neutroamineMultiplier = listingStandard.Slider(config.neutroamineMultiplier, 0.1f, 3f);
			listingStandard.GapLine();
			listingStandard.Label("GeneFab_ArchiteMultiplier".Translate());
			config.architeMultiplier = listingStandard.Slider( config.architeMultiplier, 0.1f, 3f);
			listingStandard.GapLine();
			listingStandard.Label("GeneFed_WorkMultiplier".Translate());
			config.workMultiplier = listingStandard.Slider(config.workMultiplier, 0.1f, 3f);
			listingStandard.GapLine();
			if (listingStandard.ButtonTextLabeled("GeneFab_BlacklistedButton".Translate(), "GeneFab_BlackListSimple".Translate()))
				Find.WindowStack.Add(new GeneUI(null, GeneUI.Mode.Blacklist));
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
}
