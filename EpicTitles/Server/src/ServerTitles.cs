using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MiniJSON;

namespace EpicTitles
{
    [BepInPlugin("net.bdew.valheim.serverepictitles", "ServerEpicTitles", "0.2")]
    public class ServerEpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;
        public static Dictionary<string, Dictionary<string, byte>> skillLadders;

        void Awake() {
            Log = Logger;
            Harmony.CreateAndPatchAll(typeof(ServerEpicTitles));
            loadSkillLadder();
        }

        [HarmonyPatch(typeof(ZNet), "Awake")]
        [HarmonyPostfix]
        private static void ZNet_Awake(ref ZNet __instance)
        {
            try
            {
                ZRoutedRpc.instance.Register<String, String, int>("SkillUpdate", OnClientSkillUpdate);
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        [HarmonyPatch(typeof (ZNet), "SaveWorldThread")]
        [HarmonyPostfix]
        private static void ZNet_SaveWorld(ref ZNet __instance){
            saveSkillLadder();
        }

        public static void OnClientSkillUpdate(long sender, String playerName, String skill, int level){
            Log.LogInfo($"Received SkillUpdate from {playerName} on {skill}:{level}");
            var byteLevel = (byte)level;
            if (byteLevel % 10 == 0) {
                // send the notification to other peers
                Log.LogInfo($"Sending notification of SkillRankUp of {playerName} on {skill}");
                var _playerName = playerName;
                if (playerName == "Tomu") _playerName = "Paloma";
                NotifityOtherClients(sender, $"{playerName} is now a {Common.getSkillRank(byteLevel)} {Common.getSkillTitle(skill)}!");
            }
            if (skillLadders.ContainsKey(skill))
                skillLadders[skill][playerName] = byteLevel;
            else
                skillLadders[skill] = new Dictionary<string, byte>(){{playerName, byteLevel}};

            getSkillLadder(skill);
        }

        static void NotifityOtherClients(long sender, String message){
            var znet =  Traverse.Create(typeof(ZNet)).Field("m_instance").GetValue() as ZNet;
            var mPeers = Traverse.Create((znet)).Field("m_peers").GetValue() as List<ZNetPeer>;

            foreach (var peer in mPeers)
            {
                if (peer.IsReady())
                {
                    if (peer.m_uid == sender)
                    {
                        Log.LogInfo("sender == peer; Skip notification");
                        // peer.m_rpc.Invoke(peer.m_uid, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), String.Format("{0}", skill), (int)level);
                        continue;
                    }
                    ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "SkillRankUpNotification", message);
                    Log.LogInfo($"SkillRankUpNotification sent to {peer.m_playerName}");
                    // peer.m_rpc.Invoke("OnServerRemovePin", (object) ExplorationDatabase.PackPin(pin));
                    // ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), String.Format("{0}", skill), (int)level);
                }
            }
        }

        static void getSkillLadder(string skill){
            string message;
            if (skillLadders.ContainsKey(skill)){
                var counter = 1;
                var _title = Common.getSkillTitle(skill);
                message = $"Ladder for {skill}\n";
                foreach (KeyValuePair<string, byte> skillLadder in skillLadders[skill].OrderByDescending(key => key.Value)){
                    var _rank = Common.getSkillRank(skillLadder.Value);
                    message += $"{counter++}. {skillLadder.Key}, the {_rank} {_title} ({skillLadder.Value})\n";
                }
            }
            else
                message = $"There's no ladder for {skill}!";
            Log.LogInfo(message);
        }

        static void loadSkillLadder(){
            skillLadders = new Dictionary<string, Dictionary<string, byte>>();
            if (!File.Exists(@"SkillLadders.json")){
                Log.LogInfo("No SkillLadders.json ...");
                return;
            }

            Dictionary<string, object> tmp = MiniJSON.Json.Deserialize(File.ReadAllText(@"SkillLadders.json")) as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> _skill in tmp)
            {
                Dictionary<string, byte> chars = new Dictionary<string, byte>();
                Dictionary<string, object> characters = _skill.Value as Dictionary<string, object>;
                foreach (KeyValuePair<string, object> character in characters)
                    chars.Add(character.Key, (byte)(long)character.Value);
                skillLadders.Add(_skill.Key, chars);
            }

            foreach (KeyValuePair<string, Dictionary<string, byte>> skillLadder in skillLadders){
                foreach (KeyValuePair<string, byte> character in skillLadder.Value)
                    Log.LogInfo($"{skillLadder.Key}: {character.Key}-{character.Value}");
            }

            Log.LogInfo("SkillLadders loaded from file");
        }

        static void saveSkillLadder(){
            File.WriteAllText(@"SkillLadders.json", MiniJSON.Json.Serialize(skillLadders));
            Log.LogInfo("SkillLadders saved to file");
        }

        // [HarmonyPatch(typeof (Chat), "OnNewChatMessage")]
        // [HarmonyPostfix]
        // private static void checkForLadderCommands(long senderID, string text){
        //     Log.LogInfo("YESSS");
        //     Log.LogInfo($"{senderID} {text}");
        // }

        // [HarmonyPatch(typeof (Chat), "AddInworldText")]
        // [HarmonyPostfix]
        // private static void checkForLadderCommands2(long senderID, string text){
        //     Log.LogInfo("IW");
        //     Log.LogInfo($"{senderID} {text}");
        // }

        // [HarmonyPatch(typeof (Console), "InputText")]
        // [HarmonyPrefix]
        // private void checkConsole(ref Console __instance){
        //     Console.instance.AddString("BAZUZU");
        //     // this.AddString("BAZUZU");
        //     // Log.LogInfo("IW");
        //     // Log.LogInfo($"{senderID} {text}");
        // }
        
    }
}