using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerEpicTitles
{
    [BepInPlugin("net.bdew.valheim.serverepictitles", "ServerEpicTitles", "0.0.2")]
    class ServerEpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;

        private static int _OnSkillLevelup;
        private static Func<long, ZNetPeer> _getPeer;
        private static MethodInfo _getPeerInfo;

        void Awake() {
            // var harmony = new Harmony("net.bdew.valheim.testmod");
            // harmony.PatchAll();
            _OnSkillLevelup = "OnSkillLevelup".GetStableHashCode();
            _getPeerInfo = typeof(ZRoutedRpc).GetMethod("GetPeer",BindingFlags.NonPublic | BindingFlags.Instance);
            Log = Logger;
            Harmony.CreateAndPatchAll(typeof(ServerEpicTitles));
        }

        [HarmonyPatch(typeof(ZRoutedRpc), "RouteRPC")]
        [HarmonyPrefix]
        // static void CheckSkills(Player __instance, Skills.SkillType skill, float level) {
        static void NotifyOtherPlayersOfLevelUp(ZRoutedRpc __instance, ZRoutedRpc.RoutedRPCData rpcData) {
            // Late bind to instance

            if (_getPeer == null) {
                _getPeer = (Func<long, ZNetPeer>) Delegate.CreateDelegate(typeof(Func<long, ZNetPeer>), __instance, _getPeerInfo);
            }
            if (ZNet.instance == null || !ZNet.instance.IsServer()) return;
            int[] mycodes = {1659340188, 199378019, 1491894999, -1550530018};
            if (mycodes.Contains(rpcData.m_methodHash)) return;
            Log.LogInfo("quo");
            Log.LogInfo(string.Format("{0}: {1} => {2}", rpcData.m_senderPeerID, rpcData.m_methodHash, _OnSkillLevelup));
            if (rpcData.m_methodHash != _OnSkillLevelup) return;
            Log.LogInfo("que");
            ZNetPeer peer = _getPeer(rpcData.m_senderPeerID);
            if (peer == null) return;
            
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, 
                "ShowMessage", 
                2, "Player " + peer.m_playerName + " had a skill level up.");

            // string title = getSkillTitle(string.Format("{0}", skill));
            // string rank = getSkillRank(level);
            // int levelInt = (int)level;
            // string message = "";

            // if (levelInt % 10 == 0){
            //     message = string.Format("I'm {0} {1} now. ({2})", rank, title, levelInt);
            //     Chat.instance.SendText(Talker.Type.Shout, message);
            //     MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
            //     Log.LogInfo(string.Format("SkillUp: {0} => {1} ({2} {3})", skill, levelInt, rank, title));
            // }
        }

        static string getSkillRank(float level){
            string rank = "";
            if (level < 10) rank = "Neophyte";
            else if (level < 20) rank = "Novice";
            else if (level < 30) rank = "Apprentice";
            else if (level < 40) rank = "Journeyman";
            else if (level < 50) rank = "Expert";
            else if (level < 60) rank = "Adept";
            else if (level < 70) rank = "Master";
            else if (level > 80) rank = "Grandmaster";
            else if (level < 90) rank = "Elder";
            else rank = "Legendary";
            return rank;
        }

        static string getSkillTitle(string skillName){
            string title = "";
            switch (skillName){
                case "Axe":
                    title = "Axeman";
                    break;
                case "Blocking":
                    title = "Duelist";
                    break;
                case "Bows":
                    title = "Archer";
                    break;
                case "Clubs":
                    title = "Maceman";
                    break;
                case "Jump":
                    title = "Jumpy";
                    break;
                case "Knives":
                    title = "Assassin";
                    break;
                case "Pickaxes":
                    title = "Miner";
                    break;
                case "Run":
                    title = "Runner";
                    break;
                case "Polearms":
                    title = "Polearmsman";
                    break;
                case "Sneak":
                    title = "Ninja";
                    break;
                case "Spears":
                    title = "Spearman";
                    break;
                case "Swim":
                    title = "Swimmer";
                    break;
                case "Swords":
                    title = "Swordsman";
                    break;
                case "Unarmed":
                    title = "Wrestler";
                    break;
                case "Wood Cutting":
                    title = "Lumberjack";
                    break;
                default:
                    title = "SkillNotFound";
                    break;
            }
            return title;
        }
    }
}