using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneFabrication.Core.Config;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GeneFabrication.Core;

public static class RecipeDefGenerator
{
	public static void AddRecipes() {
		foreach (RecipeDef recipeDef
		         in DefDatabase<GeneDef>.AllDefs
			         .Select(GeneFabricationDef))
		{
			DefGenerator.AddImpliedDef(recipeDef);
			
			if(ModsConfig.OdysseyActive) {
				ThingDefOf.GeneFab_CompactGeneBench.AllRecipes.Add(recipeDef);
			}
			ThingDefOf.GeneFab_GeneBench.AllRecipes.Add(recipeDef);
		}
	}

	internal static AccessTools.FieldRef<RecipeDef, bool> fa_resolvedCachedIcon =
		AccessTools.FieldRefAccess<RecipeDef, bool>(
			typeof(RecipeDef).GetField("resolvedCachedIcon",
				BindingFlags.Instance | BindingFlags.NonPublic));

	internal static AccessTools.FieldRef<RecipeDef, Texture2D> fa_cachedIcon =
		AccessTools.FieldRefAccess<RecipeDef, Texture2D>(
			typeof(RecipeDef).GetField("cachedIcon",
				BindingFlags.Instance | BindingFlags.NonPublic));

	public static RecipeDef GeneFabricationDef(GeneDef gene)
	{
		ThingFilter ingredientFilter = new();
		ingredientFilter.SetAllow(ThingDefOf.Neutroamine, allow: true);
		ingredientFilter.SetAllow(RimWorld.ThingDefOf.ArchiteCapsule, allow: gene.biostatArc != 0);
		RecipeDef recipeDef = new()
		{
			defName = "Make_Genepack_" + gene.defName,
			label = "GeneFab.Recipe.label".Translate(gene.label),
			jobString = "GeneFab.Recipe.jobString".Translate(gene.label),
			description = "GeneFab.Recipe.description".Translate(gene.label),
			descriptionHyperlinks = [new DefHyperlink(gene)],
			fixedIngredientFilter = ingredientFilter,
			defaultIngredientFilter = ingredientFilter,
			soundWorking = DefDatabase<SoundDef>.GetNamed("Recipe_Machining"),
			ingredients = CalcIngredients(gene),
			workAmount = CalcWorkAmount(gene),
			products = [new ThingDefCountClass(RimWorld.ThingDefOf.Genepack, 1)],
			modContentPack = gene.modContentPack,
			modExtensions = [new RecipeGene(gene)],
			unfinishedThingDef = ThingDefOf.GeneFab_UnfinishedGenepack,
			workSkill = SkillDefOf.Intellectual,
			skillRequirements = new List<SkillRequirement>() { new SkillRequirement { skill = SkillDefOf.Intellectual, minLevel = CalcSkill(gene) } },
			workSpeedStat = StatDefOf.GeneralLaborSpeed,
		};
		if (gene.biostatArc > 0)
		{
			recipeDef.researchPrerequisite = RimWorld.ResearchProjectDefOf.Archogenetics;
		}
		return recipeDef;
	}

	public static List<IngredientCount> CalcIngredients(GeneDef gene)
	{
		IngredientCount neutroamine = new();
		neutroamine.SetBaseCount((int)(Mathf.Clamp(
			100
			+ Mathf.Abs(gene.biostatCpx) * 25
			+ Mathf.Abs(gene.biostatMet) * 25
			+ Mathf.Abs(gene.biostatArc) * 50,
			50, 400) * ModConfigs.NeutroamineMultiplier));
		neutroamine.filter.SetAllow(ThingDefOf.Neutroamine, allow: true);
		List<IngredientCount> ingredients = [neutroamine];
		if (gene.biostatArc != 0)
		{
			IngredientCount architeCapsule = new();
			architeCapsule.SetBaseCount((int)(Mathf.Abs(gene.biostatArc) * 3 * ModConfigs.ArchiteMultiplier));
			architeCapsule.filter.SetAllow(RimWorld.ThingDefOf.ArchiteCapsule, allow: true);
			ingredients.Add(architeCapsule);
		}
		return ingredients;
	}

	public static int CalcWorkAmount(GeneDef gene)
	{
		return (int)(Mathf.Clamp(24000
		                         + Mathf.Abs(gene.biostatCpx) * 200
		                         + Mathf.Abs(gene.biostatMet) * 2000
		                         + Mathf.Abs(gene.biostatArc) * 20000,
			20000, 50000) * ModConfigs.WorkMultiplier);
	}

	public static int CalcSkill(GeneDef gene) 
	{
		return (int)(Mathf.Clamp(6
		                         + Mathf.Abs(gene.biostatCpx)
		                         + Mathf.Abs(gene.biostatMet)
		                         + Mathf.Abs(gene.biostatArc) * 2,
			6, 18
		));
	}
}