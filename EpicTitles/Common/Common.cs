
namespace EpicTitles
{
    class Common{
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
                    title = "Marathoner";
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
                    title = "Fish'a'like";
                    break;
                case "Swords":
                    title = "Swordsman";
                    break;
                case "Unarmed":
                    title = "Wrestler";
                    break;
                case "WoodCutting":
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