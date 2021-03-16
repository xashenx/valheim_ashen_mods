using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace EpicTitles
{
    public class PatchedPlayer {
        private static Player _instance;

        [HarmonyPatch(typeof(Player), "Awake")]
        [HarmonyPostfix]
        private static void Awake(ref Player __instance)
        {
            _instance = __instance;
            // EpicTitles.Log.LogInfo("Player Patched!");

        }

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        [HarmonyPostfix]
        private static void OnSpawned(ref Player __instance)
        {
            var skills = Player.m_localPlayer.GetSkills().GetSkillList();
            // EpicTitles.Log.LogError(Player.m_localPlayer.GetPlayerName());
            // EpicTitles.Log.LogError(skills.Count);
            // EpicTitles.Log.LogError(skills);
            // EpicTitles.Log.LogError(Player.m_localPlayer.GetSkills().GetTotalSkill());
            ZPackage pkg = new ZPackage();

            //Add number of clean lines to package
            pkg.Write(skills.Count);

            //Add each line to the package
            foreach (var item in skills)
            {
                pkg.Write($"{item.m_info.m_skill}:{(byte)item.m_level}");

               EpicTitles.Log.LogInfo($"SENT SKILL: {item.m_info.m_skill}:{item.m_level}");
            }
            ZRoutedRpc.instance.InvokeRoutedRPC("SkillUpdate", Player.m_localPlayer.GetPlayerName(), pkg);
        }

        [HarmonyPatch(typeof(Player), "OnSkillLevelup")]
        [HarmonyPrefix]
        static void CheckSkills(Player __instance, Skills.SkillType skill, float level) {
            string title = PatchedSkills.getSkillTitle(skill.ToString().ToLower());
            string rank = PatchedSkills.getSkillRank((byte)level);
            int levelInt = (int)level;
            string message = "";

            ZPackage pkg = new ZPackage();

            //Add number of clean lines to package
            pkg.Write(1);

            // check if a new rank has been acquired
            if (levelInt % 10 == 0){
                message = string.Format("I'm {0} {1} now. ({2})", rank, title, levelInt);
                // TODO use Message() on Player class!
                _instance.Message(MessageHud.MessageType.Center, message);
                // ShowMessage(message);
            }

            pkg.Write($"{skill.ToString()}:{(byte)level}");

            EpicTitles.Log.LogInfo($"SENT SKILL: {skill.ToString()}:{(byte)level}");

            ZRoutedRpc.instance.InvokeRoutedRPC("SkillUpdate", Player.m_localPlayer.GetPlayerName(), pkg);

            // send update to the server
            // sendSkillUpdate(skill, level);
            // ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SkillUpdate", Player.m_localPlayer.GetPlayerName(), skill.ToString(), (int)level);
            // ZRoutedRpc.instance.InvokeRoutedRPC("SkillUpdate", Player.m_localPlayer.GetPlayerName(), skill.ToString(), (int)level);
            // EpicTitles.Log.LogInfo("SkillUpdateSent");

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

        public static void ShowMessage(string message){
            // TODO use Message() on Player class!
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
        }

        public static List<Player> getAllPlayers(){
            return Player.GetAllPlayers();
        }

        public static void sendMessageToPlayer(long playerId, string message){
            // Not viable for now :(
            // player.Message(MessageHud.MessageType.Center, message);
            return ;
        }
    }
}