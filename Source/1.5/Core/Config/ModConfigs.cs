using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GeneFarbication
{
	public class ModConfigs : ModSettings
	{
		public List<string> blacklistedGenes = new List<string>();
		public float neutroamineMultiplier = 1f;
		public float architeMultiplier = 1f;
		public float workMultiplier = 1f;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref neutroamineMultiplier, "Neutroamine_cost_multiplier", 1f);
			Scribe_Values.Look(ref architeMultiplier, "Archite_capsules_cost_multiplier", 1f);
			Scribe_Values.Look(ref workMultiplier, "Work_amount_multiplier", 1f);
			Scribe_Collections.Look(ref blacklistedGenes, "Blacklisted_genes", LookMode.Value);

			base.ExposeData();
		}
	}
}
