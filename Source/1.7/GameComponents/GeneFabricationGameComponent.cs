using GeneFabrication.Core;
using GeneFabrication.Core.Config;
using RimWorld;
using Verse;

namespace GeneFabrication.GameComponents;

public class GeneFabricationGameComponent(Game game) : GameComponent {
    private bool WarnedAboutBillTab;

    public override void StartedNewGame() {
        Printer.Log($"Started new game {WarnedAboutBillTab}");
        if (!WarnedAboutBillTab && GeneFarbication.NiceBillTabEnabled) {
            Find.LetterStack.ReceiveLetter($"GeneFab_NiceBillTabIncompatibility_Title".Translate(), 
                $"GeneFab_NiceBillTabIncompatibility_Description".Translate(),
                LetterDefOf.ThreatBig);
            WarnedAboutBillTab = true;
        }
        base.StartedNewGame();
    }

    public override void LoadedGame() {
        Printer.Log($"Loaded game {WarnedAboutBillTab}");
        if (!WarnedAboutBillTab && !ModConfigs.DisableUI && GeneFarbication.NiceBillTabEnabled) {
            Find.LetterStack.ReceiveLetter($"GeneFab_NiceBillTabIncompatibility_Title".Translate(), 
                $"GeneFab_NiceBillTabIncompatibility_Description".Translate(),
                LetterDefOf.ThreatBig);
            WarnedAboutBillTab = true;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref WarnedAboutBillTab, "IsNewToSaveFileWithNiceBillTab");
        Printer.Log($"Expose Data {WarnedAboutBillTab}");
    }
}