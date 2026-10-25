using System.Collections.Generic;
using Verse;

namespace GeneFabrication.Core;

internal class GeneCategory
{
    public bool IsDisabled = true;
    public List<GeneDef> GeneDefs = new List<GeneDef>();
    public GeneCategoryDef CategoryDef;
    public GeneCategory(List<GeneDef> geneDefs, GeneCategoryDef categoryDef)
    {
        GeneDefs = geneDefs;
        CategoryDef = categoryDef;
    }
}