using Verse;

namespace GeneFabrication.Core;

public class RecipeGene(GeneDef gene) : DefModExtension
{
	[Unsaved]
	public readonly GeneDef gene = gene;
	public override string ToString()
		=> "RecipeGene=" + gene.defName;
}