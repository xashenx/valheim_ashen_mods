using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;
// using System.IO;
// using System.Linq;
// using System.Collections.Generic;

namespace EpicTitles
{
    public class PatchedSkills {
        private static Skills _instance;

        public enum SkillType
        {
            none,
            swords,
            knives,
            clubs,
            polearms,
            spears,
            blocking,
            axes,
            bows,
            firemagic,
            frostmagic,
            unarmed,
            pickaxes,
            woodcutting,
            jump = 100,
            sneak,
            run,
            swim,
            all = 999
        }

        [HarmonyPatch(typeof(Skills), "Awake")]
        [HarmonyPostfix]
        private static void Awake(ref Skills __instance)
        {
            _instance = __instance;
        }

        public static string getSkillRank(byte level){
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

        public static string getSkillTitle(string skillName){
            string title = "";
            switch (Enum.Parse(typeof(SkillType), skillName)){
                case SkillType.axes:
                    title = "Axeman";
                    break;
                case SkillType.blocking:
                    title = "Duelist";
                    break;
                case SkillType.bows:
                    title = "Archer";
                    break;
                case SkillType.clubs:
                    title = "Maceman";
                    break;
                case SkillType.jump:
                    title = "Jumpy";
                    break;
                case SkillType.knives:
                    title = "Assassin";
                    break;
                case SkillType.pickaxes:
                    title = "Miner";
                    break;
                case SkillType.run:
                    title = "Marathoner";
                    break;
                case SkillType.polearms:
                    title = "Polearmsman";
                    break;
                case SkillType.sneak:
                    title = "Ninja";
                    break;
                case SkillType.spears:
                    title = "Spearman";
                    break;
                case SkillType.swim:
                    title = "Fish'a'like";
                    break;
                case SkillType.swords:
                    title = "Swordsman";
                    break;
                case SkillType.unarmed:
                    title = "Wrestler";
                    break;
                case SkillType.woodcutting:
                    title = "Lumberjack";
                    break;
                case SkillType.firemagic:
                    title = "Fire Wizard";
                    break;
                case SkillType.frostmagic:
                    title = "Frost Wizard";
                    break;
                default:
                    title = "SkillNotFound";
                    break;
            }
            return title;
        }

        public static Sprite getSkillIcon(Skills.SkillType type){
            var skills = Player.m_localPlayer.GetSkills().GetSkillList();

            foreach (var item in skills) {
                // EpicTitles.Log.LogInfo($"{item.m_info.m_skill}={type}?{item.m_info.m_skill == type}");
                // EpicTitles.Log.LogInfo($"{item.m_info.m_description}");
                if (item.m_info.m_skill == type){
                    EpicTitles.Log.LogInfo(item.m_info.m_icon);
                    return item.m_info.m_icon;
                }
            }
            return null;
        }
    }
}