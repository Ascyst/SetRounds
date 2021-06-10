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

        public static int roundsToSet;
        private void Awake()
        {
            new Harmony(ModId).PatchAll();
            NetworkingManager.RegisterEvent(NetworkEventType.SyncRounds, sync => setRounds = (string)sync[0]);
            On.GM_ArmsRace.StartGame += GM_ArmsRace_StartGame;
        }
        private void GM_ArmsRace_StartGame(On.GM_ArmsRace.orig_StartGame orig, GM_ArmsRace self)
        {
            if (int.TryParse(setRounds, out roundsToSet))
            {
                if (roundsToSet > 1)
                {
                    self.roundsToWinGame = roundsToSet;
                    UIHandler.instance.InvokeMethod("SetNumberOfRounds", roundsToSet);
                }
            }
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