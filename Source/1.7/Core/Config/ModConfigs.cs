using System.Collections.Generic;
using Verse;

namespace GeneFabrication.Core.Config;

public class ModConfigs : ModSettings
{
	public static List<string> BlacklistedGenes = new List<string>();
	public static float NeutroamineMultiplier = 1f;
	public static float ArchiteMultiplier = 1f;
	public static float WorkMultiplier = 1f;
	public static bool LegacyUI = false;
	public static bool IgnoreAstrogenes = true;
	public static bool DisableUI = false;
	public override void ExposeData()
	{
		Scribe_Values.Look(ref NeutroamineMultiplier, "Neutroamine_cost_multiplier", 1f);
		Scribe_Values.Look(ref ArchiteMultiplier, "Archite_capsules_cost_multiplier", 1f);
		Scribe_Values.Look(ref WorkMultiplier, "Work_amount_multiplier", 1f);
		Scribe_Values.Look(ref LegacyUI, "LegacyUI");
		Scribe_Values.Look(ref IgnoreAstrogenes, "IgnoreAstrogenes", true);
		Scribe_Values.Look(ref DisableUI, "DisableUI", false);
		Scribe_Collections.Look(ref BlacklistedGenes, "Blacklisted_genes", LookMode.Value);
		
		base.ExposeData();
	}
}