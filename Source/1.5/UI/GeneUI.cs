using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GeneFarbication
{
	public class GeneUI : Window
	{
		private Vector2 initialSize = new Vector2(400f, 600f);
		private readonly float HeightOfRow = 75;
		private readonly float HeightOfTitle = 60;

		public override Vector2 InitialSize => initialSize;
		private Vector2 scrollPosition = Vector2.zero;
		private string input = "";

		private BillStack stack;
		public enum Mode {Normal, Blacklist}
		public Mode currentMode;
		public GeneUI(BillStack stack, Mode mode)
		{
			this.stack = stack;
			draggable = true;
			this.currentMode = mode;
		}
		public override void DoWindowContents(Rect rect)
		{
			List<GeneDef> genes = GenesInSearch();

			Widgets.DrawLineHorizontal(rect.x, rect.y - 1, rect.width);
			Widgets.DrawLineHorizontal(rect.x, rect.yMax + 1, rect.width);

			Text.Font = GameFont.Medium;
			string title = "GeneFab_GeneUITitle".Translate();
			Widgets.Label(rect, title);
			Text.Font = GameFont.Small;
			input = Widgets.TextArea(new Rect(rect.x, rect.y + Text.CalcHeight(title, rect.width) + 10f, Text.CalcSize(title).x, 25), input);
			if (Widgets.CloseButtonFor(rect)) Close();

			Rect mainRect = new Rect(0, HeightOfTitle, rect.width, rect.height - 75f);
			float height = 6f + (float)genes.Count * HeightOfRow;
			Rect viewRect = new Rect(0f, HeightOfTitle, mainRect.width - 16f, height);
			Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
			float num = HeightOfRow;
			float num2 = scrollPosition.y;
			float num3 = scrollPosition.y + mainRect.height;
			int num4 = 0;

			for (int i = 0; i < genes.Count; i++)
			{
				if (num + HeightOfRow > num2 && num - HeightOfRow < num3)
				{
					Rect inRect = new Rect(0f, num, viewRect.width, HeightOfRow);
					DrawCustomRow(inRect, genes[i], num4);
				}

				num += HeightOfRow;
				num4++;
			}
			Widgets.EndScrollView();
		}

		private void DrawCustomRow(Rect rect, GeneDef thing, int index)
		{
			RecipeDef recipe = DefDatabase<RecipeDef>.GetNamed("Make_Genepack_" + thing.defName);
			Rect highLightRect = new Rect(new Vector2(rect.x, rect.y), new Vector2(rect.width - 16f, HeightOfRow));
			Rect labelRect = new Rect(new Vector2(highLightRect.x + 75, highLightRect.y), new Vector2(highLightRect.width - 75f, highLightRect.height / 3));

			if (index % 2 == 0)
			{
				if(currentMode == Mode.Normal && (recipe.researchPrerequisite == null || recipe.researchPrerequisite.IsFinished))
					Widgets.DrawLightHighlight(highLightRect); //Highlight half the cells
				else if(currentMode == Mode.Blacklist)
					Widgets.DrawLightHighlight(highLightRect); //Highlight half the cells
			}
			ShowInfo(); //Show the icon and label
			ShowCost(); //Show the cost of the gene
			if (InfoCardButtonWorker(new Rect(highLightRect.width - 25, highLightRect.y, 25, 25))) //Info button
				Find.WindowStack.Add(new Dialog_InfoCard(recipe));
			ShowHovering(); //Highlight when hovering
			OnClick();
			if ((currentMode == Mode.Normal && recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished))
			{
				Widgets.DrawStrongHighlight(highLightRect, Widgets.InactiveColor);
			}
			if(currentMode == Mode.Blacklist && ModConfigsUI.config.blacklistedGenes.Any(G => G == thing.defName))
			{
				Widgets.DrawStrongHighlight(highLightRect, Widgets.InactiveColor);
			}
			// Actual code
			void ShowInfo()
			{
				Rect mainIconRect = new Rect(new Vector2(rect.x, rect.y), new Vector2(75, 75));
				Widgets.DefIcon(mainIconRect, thing);
				Widgets.Label(labelRect, thing.label.CapitalizeFirst());
			}
			void ShowCost()
			{
				Rect neutraomineIconRect = new Rect(labelRect.x, labelRect.y + labelRect.height,25, highLightRect.height / 3);
				Rect neutraomineTextRect = new Rect(neutraomineIconRect.x + neutraomineIconRect.width, neutraomineIconRect.y, highLightRect.width, highLightRect.height / 3);

				Widgets.DefIcon(neutraomineIconRect, ThingDefOf.Neutroamine);
				Widgets.Label(neutraomineTextRect, recipe.ingredients.First().GetBaseCount().ToString());
				if (recipe.ingredients.Count > 1)
				{
					Rect architeIconRect = new Rect(new Vector2(neutraomineIconRect.x, neutraomineTextRect.y + neutraomineTextRect.height), new Vector2(25, neutraomineIconRect.height));
					Rect architeRect = new Rect(new Vector2(architeIconRect.x + architeIconRect.width, architeIconRect.y), new Vector2(highLightRect.width - 75f, highLightRect.height / 3));

					Widgets.DefIcon(architeIconRect, RimWorld.ThingDefOf.ArchiteCapsule);
					Widgets.Label(architeRect, recipe.ingredients.Last().GetBaseCount().ToString());
				}
				Widgets.Label(new Rect(neutraomineTextRect.x + 30, neutraomineTextRect.y, neutraomineTextRect.width, neutraomineTextRect.height), "Requires".Translate() + $": {recipe.skillRequirements.First().skill.label.CapitalizeFirst()} {recipe.skillRequirements.First().minLevel}");
				Widgets.Label(new Rect(neutraomineTextRect.x + 30, neutraomineTextRect.y + neutraomineTextRect.height, neutraomineTextRect.width, neutraomineTextRect.height), "WorkAmount".Translate() + $": {recipe.WorkAmountTotal(null).ToStringWorkAmount()}");
			}
			void OnClick()
			{
				if (Widgets.ButtonInvisible(highLightRect))
				{
					if (currentMode == Mode.Normal &&
						((recipe.researchPrerequisite != null && recipe.researchPrerequisite.IsFinished)
						|| recipe.researchPrerequisite == null))
					{
						stack.AddBill(BillUtility.MakeNewBill(recipe));
						this.Close();
					}
					else if (currentMode == Mode.Blacklist)
					{
						if (ModConfigsUI.config.blacklistedGenes.Any(G => G == thing.defName))
							ModConfigsUI.config.blacklistedGenes.Remove(thing.defName);
						else
							ModConfigsUI.config.blacklistedGenes.Add(thing.defName);
					}
				}
			}
			void ShowHovering()
			{
				if (Mouse.IsOver(highLightRect)) //Hover effect
				{
					Widgets.DrawLineHorizontal(highLightRect.x, highLightRect.y, highLightRect.width);
					Widgets.DrawLineHorizontal(highLightRect.x, highLightRect.yMax, highLightRect.width);
					Widgets.DrawLineVertical(highLightRect.x, highLightRect.y, highLightRect.height);
					Widgets.DrawLineVertical(highLightRect.xMax - 1, highLightRect.y, highLightRect.height);
					if (currentMode == Mode.Normal && recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished)
					{
						TooltipHandler.TipRegion(highLightRect, "GeneFab_ResearchThis".Translate() + $": {recipe.researchPrerequisite.label.CapitalizeFirst()}");
					}
					if(currentMode == Mode.Blacklist && currentMode == Mode.Blacklist && ModConfigsUI.config.blacklistedGenes.Any(G => G == thing.defName))
					{
						TooltipHandler.TipRegion(highLightRect, "GeneFab_Blacklisted".Translate());
					}
				}
			}
		}

		private List<GeneDef> GenesInSearch()
		{
			List<GeneDef> geneDefsInSearch = new List<GeneDef>();
			foreach(GeneDef gene in DefDatabase<GeneDef>.AllDefsListForReading) 
			{
				if (currentMode == Mode.Normal && ModConfigsUI.config.blacklistedGenes.Any(G => G.ToLower() == gene.defName.ToLower()))
					continue;
				if (gene.label.Contains(input.ToLower()))
					geneDefsInSearch.Add(gene);

			}
			return geneDefsInSearch;
		}

		private static bool InfoCardButtonWorker(Rect rect)
		{
			MouseoverSounds.DoRegion(rect);
			TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
			bool result = Widgets.ButtonImage(rect, TexButton.Info, GUI.color);
			UIHighlighter.HighlightOpportunity(rect, "InfoCard");
			return result;
		}
	}
}
