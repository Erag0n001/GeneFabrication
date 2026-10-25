using System;
using System.Diagnostics;
using GeneFabrication.Core.Config;
using GeneFabrication.Patches;
using GeneFabrication.UI;
using HarmonyLib;
using Verse;

namespace GeneFabrication.Core;

[StaticConstructorOnStartup]
public class GeneFarbication
{
	public static readonly bool IsVEStarJackEnabled;
	public static bool NiceBillTabEnabled;
	static GeneFarbication() {
		if (!ModsConfig.BiotechActive)
			return;
		IsVEStarJackEnabled = ModLister.GetActiveModWithIdentifier("vanillaracesexpanded.starjack", true) != null;
		NiceBillTabEnabled = ModLister.GetActiveModWithIdentifier("andromeda.nicebilltab", true) != null;
		RecipeDefGenerator.AddRecipes();
		new Harmony(id: "AmCh.Eragon.GeneFarbication").PatchAll();
		if (GeneFarbication.NiceBillTabEnabled && !ModConfigs.DisableUI) {
			Printer.Warn("NiceBillTab detected, but UI for gene fabrication active! This may cause visual artefacts");
		}
		GenePicker.SetupCategories();
	}
}