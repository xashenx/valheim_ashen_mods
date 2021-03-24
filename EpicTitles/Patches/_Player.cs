using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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
                message = $"{rank} {title}!";
                // TODO use Message() on Player class!
                // _instance.Message(, message);
                var icon = PatchedSkills.getSkillIcon(skill);
                // MessageHud.MessageType type, string msg, int amount = 0, icon = null)
                
                ShowMessage("self", message, icon: icon);
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

        public static void ShowMessage(string skill, string message, Sprite icon = null){
            // TODO use Message() on Player class!
            // MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
            if (icon){
                // EpicTitles.Log.LogInfo(icon);
                // MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message, 5, icon);
                // MessageHud.instance.QueueUnlockMsg(icon, playerName, message);
                if (skill == "self"){
                    MessageHud.instance.ShowBiomeFoundMsg(message, true);
                    GameObject prefab = ZNetScene.instance.GetPrefab("vfx_Potion_health_medium");
                    if (prefab != null)
                    {
                        UnityEngine.Object.Instantiate<GameObject>(prefab, _instance.transform.position, Quaternion.identity);
                    }
                }
                else
                    MessageHud.instance.QueueUnlockMsg(icon, skill, message);
            }
            else
                // MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
                MessageHud.instance.QueueUnlockMsg(null, skill, message);
            // Mod.PlayEffect(Mod.effectFeedbackEnabled.Value, "vfx_Potion_health_medium", targetContainer.transform.position);
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