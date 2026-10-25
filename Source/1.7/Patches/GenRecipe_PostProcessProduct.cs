using System.Reflection;
using GeneFabrication.Core;
using HarmonyLib;
using RimWorld;
using Verse;

// The crafted genepack did not contain any genes initially
// We add the gene here

namespace GeneFabrication.Patches;

[HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
public static class GenRecipePostProcessProduct
{
	[HarmonyPostfix]
	public static void Postfix(ref Thing __result, RecipeDef recipeDef)
	{
		if (__result is not Genepack genepack)
			return;
		var gene = recipeDef.GetModExtension<RecipeGene>()?.gene;
		if (gene is null)
			return;
		genepack.Initialize([gene]);
	}
}