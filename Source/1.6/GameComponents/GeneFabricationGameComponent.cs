using GeneFabrication.Core;
using RimWorld;
using Verse;

namespace GeneFabrication.GameComponents;

public class GeneFabricationGameComponent(Game game) : GameComponent {
    private bool WarnedAboutBillTab;

    public override void StartedNewGame() {
        if (!WarnedAboutBillTab && GeneFarbication.NiceBillTabEnabled) {
            Find.LetterStack.ReceiveLetter($"GeneFab_NiceBillTabIncompatibility_Title".Translate(), 
                $"GeneFab_NiceBillTabIncompatibility_Description".Translate(),
                LetterDefOf.ThreatBig);
            WarnedAboutBillTab = true;
        }
        base.StartedNewGame();
    }

    public override void LoadedGame() {
        if (!WarnedAboutBillTab && GeneFarbication.NiceBillTabEnabled) {
            Find.LetterStack.ReceiveLetter($"GeneFab_NiceBillTabIncompatibility_Title".Translate(), 
                $"GeneFab_NiceBillTabIncompatibility_Description".Translate(),
                LetterDefOf.ThreatBig);
            WarnedAboutBillTab = true;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref WarnedAboutBillTab, "IsNewToSaveFileWithNiceBillTab", false);
    }
}