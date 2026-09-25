using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GeneFarbication;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace GeneFarbication
{
	[HarmonyPatch(typeof(ITab_Bills))]
	[HarmonyPatch("DoListing")]
	public static class DoListing_Patch
	{
		public static void Patch()
		{
			GeneFarbication.harmony.Patch(
				original: typeof(ITab_Bills)
					.GetMethod("FillTab",
						BindingFlags.NonPublic | BindingFlags.Instance),
				prefix: new HarmonyMethod(Prefix));
		}
public static void Prefix(ref ITab_Bills __instance)
{
	Type type = typeof(ITab_Bills);
	FieldInfo fieldInfo = type.GetField("WinSize", BindingFlags.NonPublic | BindingFlags.Static);
	Vector2 Winsize = (Vector2)fieldInfo.GetValue(__instance);
	Rect rect = new Rect(0f, 0f, Winsize.x, Winsize.y).ContractedBy(10f);
	Widgets.BeginGroup(rect);
	var selTableProperty = typeof(ITab_Bills).GetProperty("SelTable", BindingFlags.NonPublic | BindingFlags.Instance);
	if (selTableProperty == null) return;
	Building_WorkTable building = (Building_WorkTable)selTableProperty.GetValue(__instance);
	if(building == null) return;
	BillStack stack = building.billStack;
	if (building.def.defName == ThingDefOf.GeneFab_GeneBench.defName)
	{
		Rect rect2 = new Rect(0f, 0f, 150f, 29f);
		if(Widgets.ButtonText(rect2, ""))
		{
			Find.WindowStack.Add(new GeneUI(stack, GeneUI.Mode.Normal));
		}
	}
	Widgets.EndGroup();
}
	}
}
