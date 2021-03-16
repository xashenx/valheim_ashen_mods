using BepInEx;
using HarmonyLib;

namespace EpicTitles
{
    public class PatchedConsole {
        private static Console _instance;

        [HarmonyPatch(typeof(Console), "Awake")]
        [HarmonyPostfix]
        private static void Console_Awake(ref Console __instance)
        {
            _instance = __instance;
        }

        [HarmonyPatch(typeof(Console), "InputText")]
        [HarmonyPrefix]
        private static void ConsolePrePatch(ref Console __instance){
            string text = __instance.m_input.text;

            if (text.StartsWith("ladder")){
                    ZRoutedRpc.instance.InvokeRoutedRPC("LadderRequest", 
                        text, Player.m_localPlayer.GetPlayerName());
                    // EpicTitles.Log.LogInfo("LadderRequestSent");
                    return;
            }
        }

        [HarmonyPatch(typeof(Console), "InputText")]
        [HarmonyPostfix]
        private static void ConsolePostPatch(ref Console __instance){
            string text = __instance.m_input.text;

            if (text == "help"){
                writeOnChat("ladder [name] - shows the selected ladder");
            }
        }

        public static void writeOnChat(string text){
            _instance.Print(text);
        }
    }
}