using BepInEx;
using HarmonyLib;
using System;
// using System.IO;
// using System.Linq;
// using System.Collections.Generic;

namespace EpicTitles
{
    public class PatchedSkills {
        private static Skills _instance;

        public enum SkillType
        {
            None,
            Swords,
            Knives,
            Clubs,
            Polearms,
            Spears,
            Blocking,
            Axes,
            Bows,
            FireMagic,
            FrostMagic,
            Unarmed,
            Pickaxes,
            WoodCutting,
            Jump = 100,
            Sneak,
            Run,
            Swim,
            All = 999
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
                case SkillType.Axes:
                    title = "Axeman";
                    break;
                case SkillType.Blocking:
                    title = "Duelist";
                    break;
                case SkillType.Bows:
                    title = "Archer";
                    break;
                case SkillType.Clubs:
                    title = "Maceman";
                    break;
                case SkillType.Jump:
                    title = "Jumpy";
                    break;
                case SkillType.Knives:
                    title = "Assassin";
                    break;
                case SkillType.Pickaxes:
                    title = "Miner";
                    break;
                case SkillType.Run:
                    title = "Marathoner";
                    break;
                case SkillType.Polearms:
                    title = "Polearmsman";
                    break;
                case SkillType.Sneak:
                    title = "Ninja";
                    break;
                case SkillType.Spears:
                    title = "Spearman";
                    break;
                case SkillType.Swim:
                    title = "Fish'a'like";
                    break;
                case SkillType.Swords:
                    title = "Swordsman";
                    break;
                case SkillType.Unarmed:
                    title = "Wrestler";
                    break;
                case SkillType.WoodCutting:
                    title = "Lumberjack";
                    break;
                case SkillType.FireMagic:
                    title = "Fire Wizard";
                    break;
                case SkillType.FrostMagic:
                    title = "Frost Wizard";
                    break;
                default:
                    title = "SkillNotFound";
                    break;
            }
            return title;
        }
    }
}