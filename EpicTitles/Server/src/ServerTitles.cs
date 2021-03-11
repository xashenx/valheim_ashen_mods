using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
// using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
// using System.Text;
// using System.Threading.Tasks;
using UnityEngine.UI;

namespace EpicTitles
{
    [BepInPlugin("net.bdew.valheim.serverepictitles", "ServerEpicTitles", "0.2")]
    public class ServerEpicTitles : BaseUnityPlugin {
        public static ManualLogSource Log;

        void Awake() {
            Log = Logger;
            Harmony.CreateAndPatchAll(typeof(ServerEpicTitles));
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

        public static void OnClientSkillUpdate(long sender, String playerName, String skill, int level){
            if (level % 10 == 0) {
                // send the notification to other peers
                Log.LogInfo($"Sending notification of SkillRankUp of {playerName} on {skill}");
                NotifityOtherClients(sender, $"{playerName} is now a {Common.getSkillRank(level)} {Common.getSkillTitle(skill)}!");
            }
            Log.LogInfo(String.Format("{0}: {1} {2} {3}", sender, skill, level, playerName));
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
    }
}