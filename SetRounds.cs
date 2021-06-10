using System;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnboundLib;
using UnityEngine;

namespace SetRoundsPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.0.0.1")]
    [BepInProcess("Rounds.exe")]
    public class SetRounds : BaseUnityPlugin
    {
        private struct NetworkEventType
        {
            public const string SyncRounds = (ModId + "_Sync");
        }
        private const string ModId = "com.ascyst.rounds.setrounds";

        private const string ModName = "Set Rounds";

        public static string setRounds = "5";


        private void Awake()
        {
            new Harmony(ModId).PatchAll();
            NetworkingManager.RegisterEvent(NetworkEventType.SyncRounds, sync => setRounds = (string)sync[0]);
        }
        private void Start()
        {

            Unbound.RegisterGUI("Set Rounds per Game", new Action(this.DrawGUI));
            Unbound.RegisterHandshake(ModId, new Action(this.OnHandShakeCompleted));
        }

        private void DrawGUI()
        {
            string flag = GUILayout.TextField(setRounds, 3);
            if (flag != setRounds && PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RaiseEvent(NetworkEventType.SyncRounds, flag);
            }
            setRounds = flag;

        }

        private void OnHandShakeCompleted()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RaiseEvent(NetworkEventType.SyncRounds, setRounds);
            }
        }
    }
}

namespace GM_ArmsRace_Patch
{
    [HarmonyPatch(typeof(GM_ArmsRace), "StartGame")]
    public class GM_ArmsRace_StartGame_Patch
    { 
        public static int roundsToSet;
        [HarmonyPostfix]
        private static void Postfix(ref int ___roundsToWinGame)
        {
            if (int.TryParse(SetRoundsPlugin.SetRounds.setRounds, out roundsToSet))
            {
                if (roundsToSet > 1)
                {
                    ___roundsToWinGame = roundsToSet;
                    UIHandler.instance.InvokeMethod("SetNumberOfRounds", roundsToSet);
                }
            }
        }
    }
}

namespace GM_DeathMatch_Patch
{
    [HarmonyPatch(typeof(RWF.GameModes.GM_Deathmatch), "StartGame")]
    public class GM_Deathmatch_StartGame_Patch
    {
        public static int roundsToSet;
        [HarmonyPostfix]
        private static void Postfix(ref int ___roundsToWinGame)
        {
            if (int.TryParse(SetRoundsPlugin.SetRounds.setRounds, out roundsToSet))
            {
                if (roundsToSet > 1)
                {
                    ___roundsToWinGame = roundsToSet;
                    UIHandler.instance.InvokeMethod("SetNumberOfRounds", roundsToSet);
                }
            }
        }
    }
}