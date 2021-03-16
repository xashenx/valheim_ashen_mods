using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
// using System;
// using System.IO;
// using System.Linq;
// using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;

namespace EpicTitles
{
    [BepInPlugin("net.bdew.valheim.epictitles", "EpicTitles", "1.1.0")]
    public class EpicTitles : BaseUnityPlugin {
        // public ETLogger mylogger;
        public static ManualLogSource Log;

        void Awake() {
            // mylogger = new ETLogger(Logger);
            Log = Logger;
            Harmony.CreateAndPatchAll(typeof(PatchedZNet));
            Harmony.CreateAndPatchAll(typeof(PatchedPlayer));
            Harmony.CreateAndPatchAll(typeof(PatchedConsole));
            // ETLogger.logMeOn("xxx");
            // Harmony harmony = new Harmony("mod.epictitles");
            // harmony.PatchAll();
            // Harmony.CreateAndPatchAll(typeof(EpicTitles));
            // Harmony.CreateAndPatchAll(typeof(PatchedZNet));
        }
    }
}