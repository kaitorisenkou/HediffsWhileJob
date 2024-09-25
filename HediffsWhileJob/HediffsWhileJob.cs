using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace HediffsWhileJob {
    [StaticConstructorOnStartup]
    public class HediffsWhileJob {
        static HediffsWhileJob() {

            Log.Message("[HediffsWhileJob] Now active");
            var harmony = new Harmony("kaitorisenkou.HediffsWhileJob");
            harmony.Patch(
                AccessTools.Method(typeof(JobDriver), nameof(JobDriver.DriverTick), null, null),
                null,
                new HarmonyMethod(typeof(HediffsWhileJob), nameof(Patch_DriverTick), null),
                null,
                null
                );
            harmony.Patch(
                AccessTools.Method(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob), null, null),
                new HarmonyMethod(typeof(HediffsWhileJob), nameof(Patch_EndCurrentJob), null),
                null,
                null,
                null
                );
            Log.Message("[HediffsWhileJob] Harmony patch complete!");
        }

        public static void Patch_DriverTick(ref JobDriver __instance) {
            var ext = __instance.job?.def?.GetModExtension<ModExtension_HediffsWhileJob>();
            if (ext == null) {
                return;
            }
            var pawn = __instance.pawn;
            var healthTracker = pawn.health;
            if (__instance.ended) {
                if (healthTracker.hediffSet.TryGetHediff(ext.hediffDef, out Hediff hediff)) {
                    healthTracker.RemoveHediff(hediff);
                }
            } else {
                if (healthTracker.hediffSet.HasHediff(ext.hediffDef)) {
                    return;
                }
                BodyPartRecord record = null;
                if (ext.bodyPartDef != null) {
                    healthTracker.hediffSet.GetBodyPartRecord(ext.bodyPartDef);
                }
                var hediff = HediffMaker.MakeHediff(ext.hediffDef, __instance.pawn, record);
                hediff.Severity = ext.severity;
                healthTracker.AddHediff(hediff);

            }
        }

        public static void Patch_EndCurrentJob(Pawn ___pawn, Pawn_JobTracker __instance) {
            var ext = __instance.curJob?.def?.GetModExtension<ModExtension_HediffsWhileJob>();
            if (ext == null) {
                return;
            }
            var healthTracker = ___pawn.health;
            if (healthTracker.hediffSet.TryGetHediff(ext.hediffDef, out Hediff hediff)) {
                healthTracker.RemoveHediff(hediff);
            }
        }
    }
}
