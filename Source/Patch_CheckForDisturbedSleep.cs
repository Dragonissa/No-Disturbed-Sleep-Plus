using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace NoDisturbedSleepPlus
{

    [HarmonyPatch(typeof(Pawn), "CheckForDisturbedSleep")]
    public static class Patch_CheckForDisturbedSleep
    {

        private static NDSPModSettings Settings => LoadedModManager.GetMod<NoDisturbedSleepPlus>()?.GetSettings<NDSPModSettings>() ?? NoDisturbedSleepPlus.settings;
        private static AccessTools.FieldRef<Pawn, int> lastSleepDisturbedTickRef = AccessTools.FieldRefAccess<Pawn, int>("lastSleepDisturbedTick");
        private static bool Prefix(Pawn __instance, Pawn source)
        {
            /* In vanilla, the code tries to prevent bonded animals from disturbing sleeping pawns, but the check misses the fact that
             * a relation is bi-derectional, meaning that every pawn with a bond will never disturb anyone.
             * 
             * It is reported to the devs and hopefully fixed someday.
             */

            Pawn_NeedsTracker needs = __instance.needs;
            bool flag = ((needs != null) ? needs.mood : null) == null || RestUtility.Awake(__instance) || __instance.Faction != Faction.OfPlayer || Find.TickManager.TicksGame < lastSleepDisturbedTickRef.Invoke(__instance) + 300 || __instance.Deathresting;
            var bed = __instance.CurrentBed();
            var room = __instance.GetRoom();

            if (flag)
            {
                return true;
            }
            if (source == null)
            {
                return true;
            }

            if (Settings.disableDisturbedSleep)
            {
                if (Prefs.DevMode & Settings.debugMode)
                {
                    Log.Message(string.Format("[NoDisturbedSleep(Disabled)] Disturbed sleep is disabled in settings. Skipping disturbance for {0}", __instance.Name));
                }
                return false;
            }
            if (Settings.disableDisturbedSleepForHospitals)
            {
                if (bed.GetRoom().Role == RoomRoleDefOf.Hospital && IsPatient(__instance))
                {
                    if (IsDoctorOrTending(source) || IsCarryingInjuredPawn(source))
                    {
                        if (Prefs.DevMode & Settings.debugMode)
                        {
                            Log.Message(string.Format("[NoDisturbedSleep(Hospital)] Disturbed sleep is disabled for hospitals in settings. Skipping disturbance for {0} due to source {1}.", __instance.Name, source.Name));
                        }
                        return false;
                    }
                }

            }
            if (Settings.disableDisturbedSleepForPolyLovers)
            {
                if (!bed.Medical && bed.TotalSleepingSlots == 1)
                {
                    if (Prefs.DevMode & Settings.debugMode)
                    {
                        Log.Message($"[NoDisturbedSleep(Poly)] Not skipping disturbance for {__instance.Name} from {source.Name} due to single sleeping slot.");
                    }
                    return true;
                }

                bool ignoreDisturbance =
                    LovePartnerRelationUtility.LovePartnerRelationExists(__instance, source)
                    || bed.CurOccupants
                        .Concat(bed.OwnersForReading ?? Enumerable.Empty<Pawn>())
                        .Distinct()
                        .Any(p =>
                            p != __instance &&
                            p != source &&
                            LovePartnerRelationUtility.LovePartnerRelationExists(__instance, p) &&
                            LovePartnerRelationUtility.LovePartnerRelationExists(source, p));

                if (ignoreDisturbance)
                {
                    if (Prefs.DevMode & Settings.debugMode)
                    {
                        Log.Message($"[NoDisturbedSleep(Poly)] Skipping disturbance for {__instance.Name} from {source.Name} due to romantic connection.");
                    }
                    return false;
                }
            }

            return true;   
        }

        public static bool IsPatient(Pawn pawn)
        {
            var bed = pawn.CurrentBed();
            return bed != null && bed.Medical;
        }

        public static bool IsDoctorOrTending(Pawn pawn)
        {
            return (pawn.workSettings != null &&
                    pawn.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
                || (pawn.CurJob != null &&
                    pawn.CurJob.def == JobDefOf.TendPatient);
        }

        public static bool IsCarryingInjuredPawn(Pawn pawn)
        {
            var carried = pawn.carryTracker?.CarriedThing as Pawn;
            if (carried == null) return false;

            return carried.health?.HasHediffsNeedingTend() ?? false;
        }

    }

}
