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
    [BepInPlugin("net.bdew.valheim.serverepictitles", "ServerEpicTitles", "0.5")]
    public class ServerEpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;
        public static Dictionary<string, Dictionary<string, byte>> skillLadders;
        public static readonly string ET_DataPath =
            Paths.BepInExRootPath + Path.DirectorySeparatorChar + "epictitles_data" 
            + Path.DirectorySeparatorChar;

        public Console console;

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
                ZRoutedRpc.instance.Register<String, String>("LadderRequest", OnClientLadderRequest);
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
            // Log.LogInfo($"Received SkillUpdate from {playerName} on {skill}:{level}");
            var byteLevel = (byte)level;
            if (byteLevel % 10 == 0) {
                // send the notification to other peers
                // Log.LogInfo($"Sending notification of SkillRankUp of {playerName} on {skill}");
                var _playerName = playerName;
                if (playerName == "Tomu") _playerName = "Paloma";
                NotifityOtherClients(sender, $"{playerName} is now a {Common.getSkillRank(byteLevel)} {Common.getSkillTitle(skill)}!");
            }
            if (skillLadders.ContainsKey(skill))
                skillLadders[skill][playerName] = byteLevel;
            else
                skillLadders[skill] = new Dictionary<string, byte>(){{playerName, byteLevel}};
        }

        public static void OnClientLadderRequest(long sender, String request, String player){
            string response = "";
            if (request.Length < 8)
                response = listAvailableLadders();
            else {
                string ladderName = request.Substring(7);
                Log.LogInfo($"Skill ladder request from {sender}: {ladderName}");
                if (skillLadders.ContainsKey(ladderName))
                    response = getSkillLadder(ladderName, playerName: player);
                else
                    response = listAvailableLadders();
            }

            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "LadderResponse", response);
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

        static String getSkillLadder(string skill, string playerName = ""){
            string message = "";
            if (skillLadders.ContainsKey(skill)){
                var counter = 1;
                var _title = Common.getSkillTitle(skill);
                var position = 0;
                foreach (KeyValuePair<string, byte> skillLadder in skillLadders[skill].OrderByDescending(key => key.Value)){
                    var _rank = Common.getSkillRank(skillLadder.Value);
                    if (skillLadder.Key == playerName)
                        position = counter;
                        // message += $"\n<b>{counter++}. {skillLadder.Key}, the {_rank} {_title} ({skillLadder.Value})</b>";
                    message += $"\n{counter++}. {skillLadder.Key}, the {_rank} {_title} ({skillLadder.Value})";
                }
                var skillUpper = skill.ToUpper();
                var title = $"===== {skillUpper} LADDER =====";
                var divider = "";
                for (int i = 0; i < title.Length; i++)
                    divider += "=";
                message = "\n" + divider + message;

                if (position > 0)
                    message = $"\nYou're in position #{position}!" + message;
                message = title + message;
            }
            else
                message = $"There's no ladder for {skill}!";
            return message;
        }

        static String listAvailableLadders(){
            string availableLadders = "";
            foreach (KeyValuePair<string, Dictionary<string, byte>> skillLadder in skillLadders){
                availableLadders += '\n' + skillLadder.Key;
            }
            
            if (availableLadders == "")
                availableLadders = "Sorry, there's no ladder available!";
            else
                
                availableLadders = "List of available ladders:" + availableLadders;
                availableLadders = "===== EPIC TITLES - LADDER =====\n" + availableLadders;
            return availableLadders;
        }

        static void loadSkillLadder(){
            skillLadders = new Dictionary<string, Dictionary<string, byte>>();
            if (!Directory.Exists(ET_DataPath))
                Directory.CreateDirectory(ET_DataPath);

            if (!File.Exists(@"" + ET_DataPath + "skill_ladders.json")){
                Log.LogInfo("No SkillLadders.json ...");
                return;
            }

            Dictionary<string, object> tmp = MiniJSON.Json.Deserialize(File.ReadAllText(@"" + ET_DataPath + "skill_ladders.json")) as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> _skill in tmp)
            {
                Dictionary<string, byte> chars = new Dictionary<string, byte>();
                Dictionary<string, object> characters = _skill.Value as Dictionary<string, object>;
                foreach (KeyValuePair<string, object> character in characters)
                    chars.Add(character.Key, (byte)(long)character.Value);
                skillLadders.Add(_skill.Key, chars);
            }

            // foreach (KeyValuePair<string, Dictionary<string, byte>> skillLadder in skillLadders){
            //     foreach (KeyValuePair<string, byte> character in skillLadder.Value)
            //         Log.LogInfo($"{skillLadder.Key}: {character.Key}-{character.Value}");
            // }

            Log.LogInfo("SkillLadders loaded from file");
        }

        static void saveSkillLadder(){
            File.WriteAllText(@"" + ET_DataPath + "skill_ladders.json", MiniJSON.Json.Serialize(skillLadders));
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