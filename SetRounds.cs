using System;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using System.Collections;
using UnboundLib.Networking;

namespace SetRoundsPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.1.2")]
    [BepInProcess("Rounds.exe")]
    public class SetRounds : BaseUnityPlugin
    {
        private struct NetworkEventType
        {
            public const string SyncRounds = (ModId + "_SyncRounds");
            public const string SyncPoints = (ModId + "_SyncPoints");
        }
        private const string ModId = "com.ascyst.rounds.setrounds";

        private const string ModName = "Set Rounds";

        public static string setRounds = "5";
        public static string setPoints = "2";

        public static int syncRounds = 5;
        public static int syncPoints = 2;
        private IEnumerator SetRound(IGameModeHandler gm)
        {
            gm.ChangeSetting("roundsToWinGame", syncRounds);
            gm.ChangeSetting("pointsToWinRound", syncPoints);
            yield break;
        }
        private void Awake()
        {
            new Harmony(ModId).PatchAll();
            GameModeManager.AddHook(GameModeHooks.HookInitEnd, SetRound);
        }



        private void Start()
        {
            Unbound.RegisterGUI("Set Rounds per Game", new Action(this.DrawGUI));
            Unbound.RegisterHandshake(ModId, new Action(this.OnHandShakeCompleted));
        }

        private void DrawGUI()
        {
            GUILayout.Label("Rounds per Game");
            var rounds = GUILayout.TextField(setRounds, 3);

            GUILayout.Label("Points per Round");
            var points = GUILayout.TextField(setPoints, 1);

            if (PhotonNetwork.OfflineMode || PhotonNetwork.CurrentRoom == null)
            {
                setRounds = rounds;
                setPoints = points;
                int.TryParse(setRounds, out syncRounds);
                int.TryParse(setPoints, out syncPoints);
            }
        }

        private void OnHandShakeCompleted()
        {
            int.TryParse(setRounds, out syncRounds);
            int.TryParse(setPoints, out syncPoints);
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(SetRounds), nameof(UpdateValues), syncRounds, syncPoints);
            }
        }

        [UnboundRPC]
        private static void UpdateValues(int roundsToWin, int pointsToWin)
        {
            syncRounds = roundsToWin;
            syncPoints = pointsToWin;
        }
    }
}