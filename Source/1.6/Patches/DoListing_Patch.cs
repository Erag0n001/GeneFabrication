using System;
using System.Reflection;
using GeneFabrication.Core;
using GeneFabrication.Core.Config;
using GeneFabrication.UI;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using ThingDefOf = GeneFabrication.Core.ThingDefOf;

namespace GeneFabrication.Patches;

[HarmonyPatch(typeof(ITab_Bills), "FillTab")]
public static class DoListingPatch
{
	private static readonly Vector2 WinSize = (Vector2)AccessTools.Field(typeof(ITab_Bills), "WinSize").GetValue(null);

	private delegate Building_WorkTable SelTableDelegate(ITab_Bills tab);
	private static readonly SelTableDelegate SelTableDel = (SelTableDelegate)SelTableDelegate.CreateDelegate(typeof(SelTableDelegate), AccessTools.Property(typeof(ITab_Bills), "SelTable").GetMethod);
	
	[HarmonyPrefix]
	public static void Prefix(ref ITab_Bills __instance)
	{
		if (ModConfigs.DisableUI) {
			return;
		}
		var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
		Widgets.BeginGroup(rect);
		var building = SelTableDel(__instance);
		if(building == null) return;
		var stack = building.billStack;
		if (building.def == ThingDefOf.GeneFab_GeneBench || (ModsConfig.OdysseyActive && building.def ==  ThingDefOf.GeneFab_CompactGeneBench))
		{
			if (stack == null) {
				Printer.Error("Error while creating UI for bench for Gene Fabrication, no billstack? Mod incompatibility?");
				return;
			}
			var rect2 = new Rect(0f, 0f, 150f, 29f);
			if(Widgets.ButtonText(rect2, "")) {
				Window window = ModConfigs.LegacyUI
#pragma warning disable CS0612 // Type or member is obsolete
					? new GeneUI(stack, GeneUIMode.Normal)
#pragma warning restore CS0612 // Type or member is obsolete
					: new GenePicker(stack, GeneUIMode.Normal); 
				Find.WindowStack.Add(window);
			}
		}
		Widgets.EndGroup();
	}
}