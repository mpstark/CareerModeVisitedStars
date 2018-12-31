using BattleTech;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CareerModeVisitedStars
{
    [HarmonyPatch(typeof(StarmapRenderer), "RefreshSystems")]
    public static class StarmapRenderer_RefreshSystems_Patch
    {
        private static MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        public static void Postfix(StarmapRenderer __instance, Dictionary<GameObject, StarmapSystemRenderer> ___systemDictionary)
        {
            var simGame = Traverse.Create(__instance.starmap).Field("sim").GetValue<SimGameState>();


            if (!Patches.Settings.CareerOnly || simGame.IsCareerMode())
            {
                var visitedSystems = Traverse.Create(simGame).Field("VisitedStarSystems").GetValue<List<string>>();

                mpb.Clear();
                foreach (var system in visitedSystems)
                {
                    var systemRenderer = __instance.GetSystemRenderer(system);
                    var starOuter = Traverse.Create(systemRenderer).Field("starOuter").GetValue<Renderer>();
                    var newColor = systemRenderer.systemColor / 3f;

                    // set outer color
                    mpb.SetColor("_Color", newColor);
                    starOuter.SetPropertyBlock(mpb);
                }
            }
        }
    }

    public class ModSettings
    {
        public bool CareerOnly = true;
    }

    public static class Patches
    {
        public static ModSettings Settings;

        public static void Init(string modDir, string modSettings)
        {
            var harmony = HarmonyInstance.Create("io.github.mpstark.CareerModeVisitedStars");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // read settings
            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(modSettings);
            }
            catch (Exception)
            {
                Settings = new ModSettings();
            }
        }
    }
}
