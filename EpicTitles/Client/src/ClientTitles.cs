using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
// using System.Collections.Generic;
// using System.Reflection;
// using UnityEngine;
using System;
// using System.Linq;
// using System.String;
// using BepInEx.Configuration;

namespace EpicTitles
{
    [BepInPlugin("net.bdew.valheim.epictitles", "EpicTitles", "0.3")]
    class EpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;
        public static Console console;

        void Awake() {
            // var harmony = new Harmony("net.bdew.valheim.testmod");
            // harmony.PatchAll();
            Log = Logger;
            // Log.LogInfo(string.Format("[EPICTITLESSS!!!!!] YEAAAAAAAAAAAAAAAHHH"));
            Harmony.CreateAndPatchAll(typeof(EpicTitles));
        }

        [HarmonyPatch(typeof(Console), "Awake")]
        [HarmonyPostfix]
        private static void Console_Awake(ref Console __instance)
        {
            console = __instance;
        }

        [HarmonyPatch(typeof(ZNet), "Awake")]
        [HarmonyPostfix]
        private static void ZNet_Awake(ref ZNet __instance)
        {
            try
            {
                ZRoutedRpc.instance.Register<String>("SkillRankUpNotification", SkillRankUpNotification);
                ZRoutedRpc.instance.Register<String>("LadderResponse", LadderResponse);
                Log.LogInfo($"isServer: {__instance.IsServer()}");
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        [HarmonyPatch(typeof(Console), "InputText")]
        [HarmonyPrefix]
        private static void ConsolePrePatch(ref Console __instance){
            string text = __instance.m_input.text;

            if (text.StartsWith("ladder")){
                    ZRoutedRpc.instance.InvokeRoutedRPC("LadderRequest", 
                        text, Player.m_localPlayer.GetPlayerName());
                    Log.LogInfo("LadderRequestSent");
                    return;
            }
        }

        [HarmonyPatch(typeof(Console), "InputText")]
        [HarmonyPostfix]
        private static void ConsolePostPatch(ref Console __instance){
            string text = __instance.m_input.text;

            if (text == "help"){
                __instance.Print("ladder [name] - shows the selected ladder");
            }
        }

        // [HarmonyPatch(typeof(Console), "Awake")]
        // [HarmonyPostfix]
        // private static void Console_Awake(ref Console __instance)
        // {
        //     try
        //     {
        //         __instance.AddString("xxxxx");
        //         __instance.SendMessageUpwards();
        //         // ZRoutedRpc.instance.Register<String>("SkillRankUpNotification", SkillRankUpNotification);
        //         // Log.LogInfo($"isServer: {__instance.IsServer()}");
        //     }
        //     catch (Exception e)
        //     {
        //         Log.LogError(e);
        //     }
        // }

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
            string rank = Common.getSkillRank((byte)level);
            int levelInt = (int)level;
            string message = "";
            // Log.LogInfo(string.Format("SkillUp: {0} => {1} ({2} {3})", skill, levelInt, rank, title));

            // check if a new rank has been acquired
            if (levelInt % 10 == 0){
                message = string.Format("I'm {0} {1} now. ({2})", rank, title, levelInt);
                // Chat.instance.SendText(Talker.Type.Shout, message);
                ShowMessage(message);
                // Log.LogInfo(string.Format("SkillUp: {0} => {1} ({2} {3})", skill, levelInt, rank, title));
                // Log.LogInfo($"SkillUp: {skill} => {levelInt} ({rank} {title})");
            }

            // send update to the server
            // sendSkillUpdate(skill, level);
            // ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), skill.ToString(), (int)level);
            ZRoutedRpc.instance.InvokeRoutedRPC("SkillUpdate", Player.m_localPlayer.GetPlayerName(), skill.ToString(), (int)level);
            Log.LogInfo("SkillUpdateSent");

            // TODO retrieve a list of the skills with the current value
            // List<Skills.Skill> char_skills = __instance.GetSkills().GetSkillList();
            //     Log.LogInfo($"{skill.m_info}: {skill.m_level}");
            // foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in __instance.GetSkills().char_skills)
            // {
            //     list.Add(keyValuePair.Value);
            // }
            // foreach (var skillx in char_skills){
            //     Log.LogInfo(string.Format("Skill: {0}", skillx));
            // }
        }

        static void ShowMessage(string message){
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
        }

        static void SkillRankUpNotification(long sender, String message){
            Log.LogInfo($"SkillRankUpNotification: {message}");
            ShowMessage(message);
        }

        static void LadderResponse(long sender, String message){
            console.Print(message);
        }

        // [HarmonyPatch(typeof (Chat), "OnNewChatMessage")]
        // [HarmonyPostfix]
        // private static void checkForLadderCommands(long senderID, string text){
        //     Log.LogInfo("YESSS");
        //     Log.LogInfo($"{senderID} {text}");
        // }
    }
}