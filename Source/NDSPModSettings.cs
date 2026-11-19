using Verse;

namespace NoDisturbedSleepPlus
{
    public class NDSPModSettings : ModSettings
    {
        public bool disableDisturbedSleep = false;
        public bool disableDisturbedSleepForHospitals = false;
        public bool disableDisturbedSleepForPolyLovers = false;

        public bool debugMode = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disableDisturbedSleep, "disableDisturbedSleep", false);
            Scribe_Values.Look(ref disableDisturbedSleepForHospitals, "disableDisturbedSleepForHospitals", false);
            Scribe_Values.Look(ref disableDisturbedSleepForPolyLovers, "disableDisturbedSleepForPolyLovers", false);
            Scribe_Values.Look(ref debugMode, "debugMode", false);
        }
    }
}
