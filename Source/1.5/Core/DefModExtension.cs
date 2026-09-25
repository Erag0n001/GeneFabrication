using Verse;

namespace GeneFarbication
{
	public class RecipeGene(GeneDef gene) : DefModExtension
	{
		[Unsaved]
		public readonly GeneDef gene = gene;
		public override string ToString()
			=> "RecipeGene=" + gene.defName;
	}
}
