using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

// The crafted genepack did not contain any genes initially
// We add the gene here

namespace GeneFarbication.Patches
{
	public class GenRecipe_PostProcessProduct
	{
		public static void Patch()
		{
			GeneFarbication.harmony.Patch(
				original: typeof(GenRecipe)
					.GetMethod("PostProcessProduct",
						BindingFlags.NonPublic | BindingFlags.Static),
				postfix: new HarmonyMethod(Postfix));
		}
		public static void Postfix(ref Thing __result, RecipeDef recipeDef)
		{
			if (__result is not Genepack genepack)
				return;
			GeneDef gene = recipeDef.GetModExtension<RecipeGene>()?.gene;
			if (gene is null)
				return;
			genepack.Initialize([gene]);
		}
	}
}
