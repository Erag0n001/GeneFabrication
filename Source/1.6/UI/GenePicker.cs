using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GeneFabrication.Core;
using GeneFabrication.Core.Config;
using RimWorld;
using UnityEngine;
using Verse;
using RecipeDefGenerator = GeneFabrication.Core.RecipeDefGenerator;
using ThingDefOf = RimWorld.ThingDefOf;

namespace GeneFabrication.UI;

public class GenePicker : Window
{
	public override Vector2 InitialSize { get; } = new Vector2(WindowWidth, WindowHeight);

	private readonly BillStack BillStack;
	private readonly GeneUIMode Mode;
	private static Dictionary<GeneCategoryDef, GeneCategory> GenesByCategory = new();
	
	private static readonly CachedTexture GeneBackgroundArchite = new("UI/Icons/Genes/GeneBackground_ArchiteGene");
	private static readonly CachedTexture GeneBackgroundXenogene = new("UI/Icons/Genes/GeneBackground_Xenogene");
	
	private Vector2 ScrollPosition = Vector2.zero;

	private string Search;
	private string DisplaySearch;
	private const int GeneRectExtraWidth = 8;
	private const int WindowWidth = 1200;
	private const int WindowHeight = 800;
	private const int HeightOfIcon = 87;
	private const int WidthOfIcon = 100;
	private const int WidthOfGeneCost = 34;
	private const int WidthOfGeneBox = WidthOfIcon + IconPadding + WidthOfGeneCost + WidthOfGeneCost;
	private const int IconPadding = 18;
	private const int RowHeightPadding = 0;
	private const int RowHeight = IconPadding + HeightOfIcon + RowHeightPadding;
	private const int PaddingFromCategoryTitleToBody = 6;
	private const int SearchBarWidth = 150;
	private const int HeightOfCategoryTitle = 29;
	private const int HeightOfFooter = 40;
	private static readonly int MediumTextHeight;
	private static bool PreviousVEStarjackConfig;
	private float ScrollRectWidth;

	private int IconsPerRow => (int)(ScrollRectWidth / (WidthOfGeneBox));
	
	static GenePicker()
	{
		Text.Font = GameFont.Medium;
		MediumTextHeight = (int)Text.LineHeight;
	}
	
	public GenePicker(BillStack billStack, GeneUIMode mode) {
		BillStack = billStack;
		Mode = mode;
		if (PreviousVEStarjackConfig != ModConfigs.IgnoreAstrogenes) {
			Printer.Log("Recalculating categories due to config change");
			SetupCategories();
		}
	}
	
	internal static void SetupCategories()
	{
		GenesByCategory.Clear();
		PreviousVEStarjackConfig = ModConfigs.IgnoreAstrogenes;
		// ReSharper disable once StringLiteralTypo
		var categoryDefs = DefDatabase<GeneCategoryDef>.AllDefsListForReading.ToList();
		categoryDefs.SortBy(x => x.label[0]);
		IEnumerable<GeneDef> genesForReading = DefDatabase<GeneDef>.AllDefsListForReading;
		if (GeneFarbication.IsVEStarJackEnabled && ModConfigs.IgnoreAstrogenes) {
			genesForReading = genesForReading.Where(x => !x.defName.EndsWith("Astrogene"));
		}
		var genesByCategory = genesForReading.ToLookup(x => x.displayCategory);
		foreach (var categoryDef in categoryDefs)
		{
			var genesInCategory = genesByCategory[categoryDef].ToList();
			if (genesInCategory.Count == 0)
				continue;
			GenesByCategory.Add(categoryDef, new GeneCategory(genesInCategory, categoryDef));
		}
	}
	
	public override void DoWindowContents(Rect mainRect)
	{
		var scrollHeight = GetHeightOfScrollRect();

		Text.Font = GameFont.Medium; 
		var title = Mode == GeneUIMode.Blacklist ? "GeneFab_GeneUITitleBlacklist".Translate() : "GeneFab_GeneUITitle".Translate();
		var titleSize = Text.CalcSize(title);
		Widgets.Label(new Rect(Vector2.zero, titleSize), title);
		ScrollRectWidth = mainRect.width;
		var viewRect = new Rect(0, 0 + titleSize.y, ScrollRectWidth + 24, mainRect.height - titleSize.y - HeightOfFooter);
		var scrollRect = new Rect(0, 0, ScrollRectWidth, scrollHeight);
		DrawSearchBar(mainRect);
		Widgets.BeginScrollView(viewRect, ref ScrollPosition, scrollRect);
		int currentHeight = 0;
		
		foreach (var category in GenesByCategory.Values)
		{
			try {
				DrawCategory(category, ref currentHeight, scrollRect);
			}
			catch (Exception ex) {
				Printer.Error($"Critical error while rendering gene category for Gene Fabrication : {ex}");
			}
		}
		Widgets.EndScrollView();
	}

	private void DrawSearchBar(Rect mainRect)
	{
		var searchBarRect = new Rect(mainRect.width - SearchBarWidth, 0, SearchBarWidth,
			Text.LineHeight);
		DisplaySearch = Widgets.TextArea(searchBarRect, DisplaySearch);
		Search = DisplaySearch.ToLower();
	}
	
	private void DrawCategory(GeneCategory category, ref int currentHeight, in Rect rect)
	{
		int heightOfCategory = GetHeightOfCategory(category);

		if (IsVisible(currentHeight, heightOfCategory, rect))
		{
			var titleRect = new Rect(0, currentHeight, rect.width, HeightOfCategoryTitle);
			Text.Font = GameFont.Medium;
			var minimizeButtonRect =
				new Rect(Text.CalcSize(category.CategoryDef.label.CapitalizeFirst()).x + titleRect.x + 5, titleRect.y, 25,
					25);
			if (category.IsDisabled)
			{
				if (Widgets.ButtonImage(minimizeButtonRect, TexButton.Reveal))
				{
					category.IsDisabled = false;
				}
			}
			else
			{
				if (Widgets.ButtonImage(minimizeButtonRect, TexButton.Collapse))
				{
					category.IsDisabled = true;
				}
			}
			
			Widgets.Label(titleRect, category.CategoryDef.label.CapitalizeFirst());
			Widgets.DrawLineHorizontal(titleRect.position.x, titleRect.position.y + HeightOfCategoryTitle, rect.width);
			if(!category.IsDisabled)
			{
				DrawGenes(category.GeneDefs, currentHeight + PaddingFromCategoryTitleToBody);
			}
		}

		currentHeight += heightOfCategory;
	}

	private void DrawGenes(List<GeneDef> genes, float categoryTop)
	{
		Text.Font = GameFont.Tiny;
		Text.Anchor = TextAnchor.UpperCenter;
		int inSearch = 0;
		foreach(var gene in genes) {
			try {
				if (!IsInSearch(gene)) {
					continue;
				}
				
				var row = inSearch / IconsPerRow;
				var col = inSearch % IconsPerRow;

				var totalGeneRect = GetGeneRect(col, row, categoryTop);
				var x = totalGeneRect.x;
				Widgets.DrawBox(totalGeneRect);
				if (Mode == GeneUIMode.Blacklist && ModConfigs.BlacklistedGenes.Any(g => g == gene.defName)) {
					Widgets.DrawStrongHighlight(totalGeneRect);
				}
				else {
					Widgets.DrawHighlight(totalGeneRect);
				}

				GeneUIUtility.DrawBiostats(gene.biostatCpx, gene.biostatMet, gene.biostatArc, ref x, totalGeneRect.y);
				var geneRect = new Rect(x, totalGeneRect.y, WidthOfIcon, HeightOfIcon);
				GeneUIUtility.DrawGeneDef(gene, geneRect, GeneType.Xenogene, null, false, false);
				x += WidthOfIcon - 12;
				var recipe = DefDatabase<RecipeDef>.GetNamedSilentFail("Make_Genepack_" + gene.defName);
				if (recipe == null) {
					TooltipHandler.TipRegion(totalGeneRect, "GeneFab_GeneUIGeneError".Translate());
					var color = Color.black;
					color.a = 0.9f;
					var previous = GUI.color;
					GUI.color = color;
					GUI.Box(totalGeneRect, "");
					GUI.color = previous;
					Printer.ErrorOnce($"Error while rendering {gene.defName}, no recipes found, ignoring", HashCode.Combine(gene.shortHash, "GeneFabrication".GetHashCode()));
					continue;
				}

				DrawGeneCost(recipe, gene.biostatArc, x, totalGeneRect.y);
				if (Mouse.IsOver(totalGeneRect)) {
					Widgets.DrawLineHorizontal(totalGeneRect.x, totalGeneRect.y, totalGeneRect.width);
					Widgets.DrawLineHorizontal(totalGeneRect.x, totalGeneRect.yMax, totalGeneRect.width);
					Widgets.DrawLineVertical(totalGeneRect.x, totalGeneRect.y, totalGeneRect.height);
					Widgets.DrawLineVertical(totalGeneRect.xMax - 1, totalGeneRect.y, totalGeneRect.height);
				}

				if (Widgets.ButtonInvisible(totalGeneRect)) {
					if (Mode == GeneUIMode.Normal) {
						BillStack.AddBill(recipe.MakeNewBill());
						this.Close();
					}
					else {
						if (ModConfigs.BlacklistedGenes.Any(g => g == gene.defName))
							ModConfigs.BlacklistedGenes.Remove(gene.defName);
						else
							ModConfigs.BlacklistedGenes.Add(gene.defName);
					}
				}

				if (Mode == GeneUIMode.Blacklist) {
					var buttonRect = new Rect(totalGeneRect.x, totalGeneRect.y + totalGeneRect.height, totalGeneRect.width, 16);
					if (ModConfigs.BlacklistedGenes.Any(g => g == gene.defName)) {
						if (Widgets.ButtonText(buttonRect, "GeneFab_Unblacklist".Translate())) {
							ModConfigs.BlacklistedGenes.Remove(gene.defName);
						}
					}
					else {
						if (Widgets.ButtonText(buttonRect, "GeneFab_Blacklist".Translate())) {
							ModConfigs.BlacklistedGenes.Add(gene.defName);
						}
					}
				}
			}
			catch (Exception ex) {
				Printer.Error($"Critical error while rendering gene {gene.defName} for Gene Fabrication: {ex}");
			}
			finally {
				inSearch++;
			}
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}
	
	/// <summary>
	/// Copied from vanilla <see cref="GeneUIUtility"/>
	/// </summary>
	public static void DrawGeneCost(RecipeDef recipe, int arc, float curX, float curY, float margin = 6f)
	{
		var num = GeneCreationDialogBase.GeneSize.y / 3f;
		var num2 = 0f;
		var num3 = Text.LineHeightOf(GameFont.Small);
		var iconRect = new Rect(curX, curY + margin + num2, num3, num3);
		var neutroamineDef = GeneFabrication.Core.ThingDefOf.Neutroamine;
		DrawStat(iconRect, neutroamineDef, recipe.ingredients[0].GetBaseCount().ToString(CultureInfo.InvariantCulture), num3);
		var rect = new Rect(curX, iconRect.y, 45f, num3);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		num2 += num;
		if (arc != 0)
		{
			var iconRect2 = new Rect(curX, curY + margin + num2, num3, num3);
			DrawStat(iconRect2, ThingDefOf.ArchiteCapsule, recipe.ingredients[1].GetBaseCount().ToString(CultureInfo.InvariantCulture), num3);
			var rect2 = new Rect(curX, iconRect2.y, 45f, num3);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, "ArchitesRequired".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ArchitesRequiredDesc".Translate());
			}
		}
		return;

		static void DrawStat(Rect iconRect, Def def, string stat, float iconWidth)
		{
			Widgets.DefIcon(iconRect, def);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.LabelFit(new Rect(iconRect.xMax, iconRect.y, 45f - iconWidth, iconWidth), stat);
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
	
	private bool IsVisible(float top, float height, Rect viewRect)
	{
		float bottom = top + height;

		float viewTop = ScrollPosition.y;
		float viewBottom = ScrollPosition.y + viewRect.height;

		return bottom > viewTop && top < viewBottom;
	}
	
	private static Rect GetGeneRect(int columnIndex, int rowIndex, float categoryTop)
	{
		return new Rect(
			columnIndex * (WidthOfGeneBox),
			categoryTop + HeightOfCategoryTitle + rowIndex * RowHeight,
			WidthOfIcon + WidthOfGeneCost + WidthOfGeneCost,
			HeightOfIcon
		);
	}

	private int GetHeightOfScrollRect()
	{
		var scrollHeight = 0;
		foreach (var category in GenesByCategory.Values)
		{
			scrollHeight += GetHeightOfCategory(category);
		}
		return scrollHeight;
	}
	
	private int GetHeightOfCategory(GeneCategory category)
	{
		var height = HeightOfCategoryTitle + RowHeightPadding;
		if (!string.IsNullOrWhiteSpace(Search))
		{
			if (AreAnyGenesInSearch(category))
			{
				height += GetHeightOfGenes(category.GeneDefs);
				category.IsDisabled = false;
				return height;
			}

			category.IsDisabled = true;
		}
		if (category.IsDisabled)
		{
			return height;
		}
		height += GetHeightOfGenes(category.GeneDefs);
		height += PaddingFromCategoryTitleToBody;
		return height;
	}
	
	private int GetHeightOfGenes(List<GeneDef> genes)
	{
		var genesInSearch = GenesInSearch(genes);
		var amountOfRows = (int)Math.Ceiling((float)genesInSearch / IconsPerRow);
		var heightOfCategory = amountOfRows * RowHeight;
		return heightOfCategory;
	}

	private bool AreAnyGenesInSearch(GeneCategory category)
	{
		foreach (var gene in category.GeneDefs)
		{
			if (IsInSearch(gene))
			{
				return true;
			}
		}
		return false;
	}

	private int GenesInSearch(List<GeneDef> defs)
	{
		int count = 0;
		foreach (var gene in defs)
		{
			if(Mode == GeneUIMode.Normal && ModConfigs.BlacklistedGenes.Any(g => g == gene.defName))
			{
				continue;
			}
			if (IsInSearch(gene)) {
				count++;
			}
		}
		return count;
	}
	
	private bool IsInSearch(GeneDef def)
	{
		return string.IsNullOrWhiteSpace(Search) || def.label.Contains(Search);
	}
}
