using System;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace Another_Archery_Patcher
{
    public class Program
    {
        private static Lazy<Settings> _lazySettings = null!;
        private static Settings Settings => _lazySettings.Value; // convenience wrapper

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings("Settings", "settings.json", out _lazySettings)
                .SetTypicalOpen(GameRelease.SkyrimSE, "AnotherArcheryPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine("\n--- PATCHER STARTING ---"); // begin

            // Handle Game Settings
            Settings.GameSettings.AddGameSettingsToPatch(state);

            // Handle Projectiles
            var count = 0;
            foreach (var proj in state.LoadOrder.PriorityOrder.Projectile().WinningOverrides()) {
                if (!Settings.IsValidPatchTarget(proj)) continue;
                Console.Write("Processing projectile: \"" + proj.EditorID + '\"');
                var countChanges = 0u;
                string appliedIdentifier = "[NONE]";
                try {
                    (_, countChanges, appliedIdentifier) = Settings.ApplyHighestPriorityStats(state.PatchMod.Projectiles.GetOrAddAsOverride(proj));
                }
                finally {
                    Console.WriteLine(" using category: \"" + appliedIdentifier + '\"');
                    Console.WriteLine("\tChanged " + countChanges + " values.");
                    count += countChanges > 0 ? 1 : 0;
                }
            }
            Console.WriteLine("--- PATCHER COMPLETE ---");
            Console.WriteLine("Modified " + count + " projectile records.\n");
        }
    }
}
