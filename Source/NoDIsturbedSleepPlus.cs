using HarmonyLib;
using Verse;

namespace NoDisturbedSleepPlus
{
    public class NoDisturbedSleepPlus : Mod
    {
        public static NDSPModSettings settings;
        public NoDisturbedSleepPlus(ModContentPack content)
          : base(content)
        {
            settings = GetSettings<NDSPModSettings>();
            new Harmony("dragonissa.NoDisturbedSleepPlus").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "No Disturbed Sleep Plus";
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Disable Disturbed Sleep", ref settings.disableDisturbedSleep, "If checked, pawns will not be disturbed by other pawns sleeping nearby.");

            if (settings.disableDisturbedSleep)
            {
                listingStandard.Label("<color=yellow><b>Note:</b> Disable this option to access a more fine-grained control over disturbed sleep behavior.</color>");
            } 
            else 
            {
                listingStandard.GapLine();
                listingStandard.CheckboxLabeled("Disable Disturbed Sleep for Hospitals", ref settings.disableDisturbedSleepForHospitals, "If checked, pawns in hospitals will not be disturbed by other pawns doing medical work.");
                listingStandard.CheckboxLabeled("Disable Disturbed Sleep for Polyamorous Lovers", ref settings.disableDisturbedSleepForPolyLovers, "If checked, pawns sleeping will not be disturbed by their polyamorous lovers.");
            }
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("Debug Mode", ref settings.debugMode, "If checked, debug messages will be logged to the console.");

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}