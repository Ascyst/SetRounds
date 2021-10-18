using System;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Utils.UI;
using UnityEngine;
using System.Collections;
using UnboundLib.Networking;
using TMPro;

namespace SetRoundsPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.2.0")]
    [BepInProcess("Rounds.exe")]
    public class SetRounds : BaseUnityPlugin
    {
        private struct NetworkEventType
        {
            public static int SyncRounds = setRounds;
            public static int SyncPoints = setPoints;
        }
        private const string ModId = "com.ascyst.rounds.setrounds";

        private const string ModName = "Set Rounds";

        public static int setRounds = 5;
        public static int setPoints = 2;
        public IEnumerator SetRound(IGameModeHandler gm)
        {
            gm.ChangeSetting("roundsToWinGame", setRounds);
            gm.ChangeSetting("pointsToWinRound", setPoints);
            yield break;
        }

        private void Start()
        {
            Unbound.RegisterHandshake(ModId, OnHandShakeCompleted);
            Unbound.RegisterMenu("Set Rounds", () => { }, NewGUI, null, false);
            GameModeManager.AddHook(GameModeHooks.HookInitEnd, SetRound);
        }

        void RoundSliderAction(float val)
        {
            setRounds = (int)val;
        }

        void PointSliderAction(float val)
        {
            setPoints = (int)val;
        }
        private void NewGUI(GameObject menu)
        {
            MenuHandler.CreateText("Set Rounds", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateSlider("Rounds per Match", menu, 50, 1f, 30f, setRounds, RoundSliderAction, out UnityEngine.UI.Slider roundSlider, true);
            MenuHandler.CreateSlider("Points per Round", menu, 50, 1f, 30f, setPoints, PointSliderAction, out UnityEngine.UI.Slider pointSlider, true);
        }

        private void OnHandShakeCompleted()
        { 
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(SetRounds), nameof(UpdateValues), setRounds, setPoints);
            }
        }

        [UnboundRPC]
        private static void UpdateValues(int roundsToWin, int pointsToWin)
        {
            setRounds = roundsToWin;
            setPoints = pointsToWin;
        }
    }
}