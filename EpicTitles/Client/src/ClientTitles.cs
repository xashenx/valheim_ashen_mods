using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
// using System.Collections.Generic;
// using System.Reflection;
using UnityEngine;
using System;
using System.Linq;
// using System.String;
// using BepInEx.Configuration;

namespace EpicTitles
{
    [BepInPlugin("net.bdew.valheim.epictitles", "EpicTitles", "0.2")]
    class EpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;

        void Awake() {
            // var harmony = new Harmony("net.bdew.valheim.testmod");
            // harmony.PatchAll();
            Log = Logger;
            // Log.LogInfo(string.Format("[EPICTITLESSS!!!!!] YEAAAAAAAAAAAAAAAHHH"));
            Harmony.CreateAndPatchAll(typeof(EpicTitles));
        }

        [HarmonyPatch(typeof(ZNet), "Awake")]
        [HarmonyPostfix]
        private static void ZNet_Awake(ref ZNet __instance)
        {
            try
            {
                ZRoutedRpc.instance.Register<String>("SkillRankUpNotification", SkillRankUpNotification);
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        // [HarmonyPatch(typeof (ZNet), "OnNewConnection")]
        // private  class ZnetPatchOnNewConnection
        // {
        //     // ReSharper disable once InconsistentNaming
        //     private static void Postfix(ZNetPeer peer, ZNet __instance)
        //     {
        //         if (__instance.IsServer())
        //         {
        //             Log.LogInfo("Registered Server Events");
                    
        //             // peer.m_rpc.Register<ZPackage>("OnClientInitialDataPin", new Action<ZRpc, ZPackage>(InitialPinSync.OnClientInitialDataPin));
        //         }
        //         else
        //         {
        //             Log.LogInfo("Registered Client Events");
                    
        //             // peer.m_rpc.Register<ZPackage>("sendSkillUpdate", new Action<ZRpc, ZPackage>(sendSkillUpdate));
        //         }
        //     }
        // }

        [HarmonyPatch(typeof(Player), "OnSkillLevelup")]
        [HarmonyPrefix]
        static void CheckSkills(Player __instance, Skills.SkillType skill, float level) {
            string title = Common.getSkillTitle(string.Format("{0}", skill));
            string rank = Common.getSkillRank(level);
            int levelInt = (int)level;
            string message = "";
            // Log.LogInfo(string.Format("SkillUp: {0} => {1} ({2} {3})", skill, levelInt, rank, title));

            // check if a new rank has been acquired
            if (levelInt % 10 == 0){
                message = string.Format("I'm {0} {1} now. ({2})", rank, title, levelInt);
                Chat.instance.SendText(Talker.Type.Shout, message);
                ShowMessage(message);
                // Log.LogInfo(string.Format("SkillUp: {0} => {1} ({2} {3})", skill, levelInt, rank, title));
                // Log.LogInfo($"SkillUp: {skill} => {levelInt} ({rank} {title})");
            }

            // send update to the server
            sendSkillUpdate(skill, level);

            // TODO retrieve a list of the skills with the current value
            // Skills char_skills = __instance.GetSkills();
            // foreach (var skillx in char_skills){
            //     Log.LogInfo(string.Format("Skill: {0}", skillx));
            // }
        }

        static void ShowMessage(string message){
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
        }

        static void sendSkillUpdate(Skills.SkillType skill, float level){
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), String.Format("{0}", skill), (int)level);
            Log.LogInfo("SkillUpdateSent");
            // Log.LogInfo();
            
            // !ZNet.instance.IsServer())
        }

        static void SkillRankUpNotification(long sender, String message){
            Log.LogInfo($"SkillRankUpNotification from {sender}: {message}");
            ShowMessage(message);
        }
    }
}