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
    public class LaddersHandler {
        private static Dictionary<string, Dictionary<string, byte>> skillLadders;
        private static readonly string ET_DataPath =
            Paths.BepInExRootPath + Path.DirectorySeparatorChar + "epictitles_data" 
            + Path.DirectorySeparatorChar;

        public static void loadSkillLadder(){
            skillLadders = new Dictionary<string, Dictionary<string, byte>>();
            if (!Directory.Exists(ET_DataPath))
                Directory.CreateDirectory(ET_DataPath);

            if (!File.Exists(@"" + ET_DataPath + "skill_ladders.json")){
                EpicTitles.Log.LogInfo("No SkillLadders.json ...");
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

            EpicTitles.Log.LogInfo("SkillLadders loaded from file");
        }

        public static void saveSkillLadder(){
            File.WriteAllText(@"" + ET_DataPath + "skill_ladders.json", MiniJSON.Json.Serialize(skillLadders));
            EpicTitles.Log.LogInfo("SkillLadders saved to file");
        }

        public static void OnClientLadderRequest(long sender, String request, String player){
            string response = "";
            if (request.Length < 8)
                response = listAvailableLadders();
            else {
                string ladderName = request.Substring(7).ToLower();
                // EpicTitles.Log.LogInfo($"Skill ladder request from {sender}: {ladderName}");
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
                        // EpicTitles.Log.LogInfo("sender == peer; Skip notification");
                        // peer.m_rpc.Invoke(peer.m_uid, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), String.Format("{0}", skill), (int)level);
                        continue;
                    }
                    // Not viable for now :(
                    // PatchedPlayer.sendMessageToPlayer(peer.m_uid, message);
                    ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "SkillRankUpNotification", message);
                    // EpicTitles.Log.LogInfo($"SkillRankUpNotification sent to {peer.m_playerName}");
                    // ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), String.Format("{0}", skill), (int)level);
                }
            }
        }

      static String getSkillLadder(string skill, string playerName = ""){
            string message = "";
            if (skillLadders.ContainsKey(skill)){
                var counter = 1;
                var _title = PatchedSkills.getSkillTitle(skill);
                var position = 0;
                foreach (KeyValuePair<string, byte> skillLadder in skillLadders[skill].OrderByDescending(key => key.Value)){
                    var _rank = PatchedSkills.getSkillRank(skillLadder.Value);
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

        public static void LadderResponse(long sender, String message){
            PatchedConsole.writeOnChat(message);
        }

        // public static void OnClientSkillUpdate(long sender, String playerName, String skill, int level){
        //     // EpicTitles.Log.LogInfo($"Received SkillUpdate from {playerName} on {skill}:{level}");
        //     var byteLevel = (byte)level;
        //     if (byteLevel % 10 == 0) {
        //         // send the notification to other peers
        //         // EpicTitles.Log.LogInfo($"Sending notification of SkillRankUp of {playerName} on {skill}");
        //         var _playerName = playerName;
        //         if (playerName == "Tomu") _playerName = "Paloma";
        //         // TODO use Message() on Player class!
        //         NotifityOtherClients(sender, $"{_playerName} is now a {PatchedSkills.getSkillRank(byteLevel)} {PatchedSkills.getSkillTitle(skill)}!");
        //     }

        //     updateLadder(playerName, skill, byteLevel);
        //     // if (skillLadders.ContainsKey(skill))
        //     //     skillLadders[skill][playerName] = byteLevel;
        //     // else
        //     //     skillLadders[skill] = new Dictionary<string, byte>(){{playerName, byteLevel}};
        // }

        // public static void OnSpawnedClientSkillUpdate(long sender, String playerName, ZPackage pkg){
        public static void OnClientSkillUpdate(long sender, String playerName, ZPackage pkg){
            if (pkg != null && pkg.Size() > 0) {
                int numLines = pkg.ReadInt();
                if (numLines == 0) {
                    EpicTitles.Log.LogError("Got zero line config file from server. Cannot load.");
                    return;
                }

                for (int i = 0; i < numLines; i++)  {
                    string line = pkg.ReadString();
                    string [] words = line.Split(':');
                    string skill = words[0].ToLower();
                    byte level = Byte.Parse(words[1]);
                    
                    EpicTitles.Log.LogInfo($"{playerName}|RECEIVED SKILL:{skill}:{level}");

                    updateLadder(playerName, skill, level);
                    
                    // if (skillLadders.ContainsKey(skill))
                    //     skillLadders[skill][playerName] = byteLevel;
                    // else
                    //     skillLadders[skill] = new Dictionary<string, byte>(){{playerName, byteLevel}};
                    if (numLines == 0) { // OnSkillLevelup
                        if (level % 10 == 0) {
                            // send the notification to other peers
                            // EpicTitles.Log.LogInfo($"Sending notification of SkillRankUp of {playerName} on {skill}");
                            var _playerName = playerName;
                            if (playerName == "Tomu") _playerName = "Paloma";
                            // TODO use Message() on Player class!
                            NotifityOtherClients(sender, $"{_playerName} is now a {PatchedSkills.getSkillRank(level)} {PatchedSkills.getSkillTitle(skill)}!");
                        }
                    }
                }
                // EpicTitles.Log.LogError($"{playerName}: {item.m_info.m_skill}=>{item.m_level}");
                // item.m_info.m_icon SKILL ICON!
            }
        }

        private static void updateLadder(string player, string skill, byte level){
            if (skillLadders.ContainsKey(skill))
                skillLadders[skill][player] = level;
            else
                skillLadders[skill] = new Dictionary<string, byte>(){{player, level}};
        }

        public static void SkillRankUpNotification(long sender, String message){
            // EpicTitles.Log.LogInfo($"SkillRankUpNotification: {message}");
            PatchedPlayer.ShowMessage(message);
        }
    }
}