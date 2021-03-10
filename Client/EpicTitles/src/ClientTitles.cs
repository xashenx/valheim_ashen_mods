using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace EpicTitles
{
    [BepInPlugin("net.bdew.valheim.epictitles", "EpicTitles", "0.1")]
    class EpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;

        void Awake() {
            // var harmony = new Harmony("net.bdew.valheim.testmod");
            // harmony.PatchAll();
            Log = Logger;
            // Log.LogInfo(string.Format("[EPICTITLESSS!!!!!] YEAAAAAAAAAAAAAAAHHH"));
            Harmony.CreateAndPatchAll(typeof(EpicTitles));
        }

        [HarmonyPatch(typeof(Player), "OnSkillLevelup")]
        [HarmonyPrefix]
        static void CheckSkills(Player __instance, Skills.SkillType skill, float level) {
            string title = getSkillTitle(string.Format("{0}", skill));
            string rank = getSkillRank(level);
            int levelInt = (int)level;
            string message = "";

            if (levelInt % 10 == 0){
                message = string.Format("I'm {0} {1} now. ({2})", rank, title, levelInt);
                Chat.instance.SendText(Talker.Type.Shout, message);
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
                Log.LogInfo(string.Format("SkillUp: {0} => {1} ({2} {3})", skill, levelInt, rank, title));
            }
            // Skills char_skills = __instance.GetSkills();
            // foreach (var skillx in char_skills){
            //     Log.LogInfo(string.Format("Skill: {0}", skillx));
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