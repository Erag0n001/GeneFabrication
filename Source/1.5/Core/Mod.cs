using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Noise;
using GeneFarbication.Patches;

namespace GeneFarbication
{
	[StaticConstructorOnStartup]
	public class GeneFarbication
	{
		public static Harmony harmony = new(id: "AmCh.Eragon.GeneFarbication");

		static GeneFarbication()
		{
			if (!ModsConfig.BiotechActive)
				return;
			RecipeDefGenerator.AddRecipes();
			GenRecipe_PostProcessProduct.Patch();
			DoListing_Patch.Patch();
		}
	}
}
