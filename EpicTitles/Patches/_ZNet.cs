using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

namespace EpicTitles
{
    public class PatchedZNet {
        private static ZNet _instance;

        [HarmonyPatch(typeof(ZNet), "Awake")]
        [HarmonyPostfix]
        private static void ZNet_Awake(ref ZNet __instance) {
            _instance = __instance;
            // EpicTitles.Log.LogInfo("Patching ZNET");
            // try
            // {
                if (__instance.IsServer()) {
                    ZRoutedRpc.instance.Register<String, String, int>("SkillUpdate", LaddersHandler.OnClientSkillUpdate);
                    ZRoutedRpc.instance.Register<String, String>("LadderRequest", LaddersHandler.OnClientLadderRequest);
                    LaddersHandler.loadSkillLadder();
                    // EpicTitles.Log.LogInfo("ZNet patched for server side");
                }
                else {
                    ZRoutedRpc.instance.Register<String>("SkillRankUpNotification", LaddersHandler.SkillRankUpNotification);
                    ZRoutedRpc.instance.Register<String>("LadderResponse", LaddersHandler.LadderResponse);
                    // EpicTitles.Log.LogInfo("ZNet patched for client side");
                }
            // }
            // catch (Exception e)
            // {
            //     // log error, not info!
            //     // ETLogger.logMeOn(e);
            //     ETLogger.logMeOn(e.ToString());
            // }
        }

        [HarmonyPatch(typeof (ZNet), "SaveWorldThread")]
        [HarmonyPostfix]
        private static void ZNet_SaveWorld(ref ZNet __instance){
            if (isServer())
            // if (__instance.IsServer())
                LaddersHandler.saveSkillLadder();
        }

        public static bool isServer(){
            return _instance.IsServer();
        }

    }
}