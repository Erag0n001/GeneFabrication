using RimWorld;
using Verse;

namespace GeneFarbication
{
	[DefOf]
	public static class ThingDefOf
	{
		public static ThingDef Neutroamine;
		public static ThingDef GeneFab_GeneBench;
		public static ThingDef GeneFab_UnfinishedGenepack;
		static ThingDefOf()
			=> DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
	}

	[DefOf]
	public static class ResearchProjectDefOf
	{
		public static ResearchProjectDef Fabrication;
		static ResearchProjectDefOf()
			=> DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectDefOf));
	}
}
