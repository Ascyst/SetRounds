using System;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using System.Collections;

namespace SetRoundsPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.0.0")]
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

        public static int syncRounds = 5;


        private void Awake()
        {
            new Harmony(ModId).PatchAll();
            NetworkingManager.RegisterEvent(NetworkEventType.SyncRounds, sync => setRounds = (string)sync[0]);
            GameModeManager.AddHook(GameModeHooks.HookInitEnd, (gm) =>
            {
                gm.ChangeSetting("roundsToWinGame", syncRounds);
            });
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
            if (PhotonNetwork.OfflineMode)
            {
                int.TryParse(setRounds, out syncRounds);
            }
        }

        private void OnHandShakeCompleted()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RaiseEvent(NetworkEventType.SyncRounds, setRounds);
            }
            int.TryParse(setRounds, out syncRounds);
        }
    }
}