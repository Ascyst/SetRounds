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
using BepInEx.Configuration;

namespace SetRoundsPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.2.1")]
    [BepInProcess("Rounds.exe")]
    public class SetRounds : BaseUnityPlugin
    {
        private static ConfigEntry<int> SetRoundsConfig;
        private static ConfigEntry<int> SetPointsConfig;

        private const string ModId = "com.ascyst.rounds.setrounds";

        private const string ModName = "Set Rounds";
        private const string CompatibilityModName = "SetRounds";

        public static int setRounds;
        public static int setPoints;

        public IEnumerator SetRound(IGameModeHandler gm)
        {
            if (setRounds > 0) { gm.ChangeSetting("roundsToWinGame", setRounds); }
            if (setPoints > 0) { gm.ChangeSetting("pointsToWinRound", setPoints); }
            yield break;
        }

        private void Awake()
        {
            SetRoundsConfig = Config.Bind(CompatibilityModName, "Number of rounds required to win the game", 0, "0 uses game mode's default value");
            SetPointsConfig = Config.Bind(CompatibilityModName, "Number of points required to win a round", 0, "0 uses game mode's default value");
        }
        private void Start()
        {
            // call settings not as not to orphan them
            setRounds = SetRoundsConfig.Value;
            setPoints = SetPointsConfig.Value;

            Unbound.RegisterHandshake(ModId, OnHandShakeCompleted);
            Unbound.RegisterMenu("Set Rounds", () => { }, NewGUI, null, false);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, SetRound);
        }

        void RoundSliderAction(float val)
        {
            SetRoundsConfig.Value = (int)val;
            setRounds = (int)val;
        }

        void PointSliderAction(float val)
        {
            SetPointsConfig.Value = (int)val;
            setPoints = (int)val;
        }
        private void NewGUI(GameObject menu)
        {
            MenuHandler.CreateText("Set Rounds", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateSlider("Rounds per Match", menu, 50, 0f, 30f, SetRoundsConfig.Value, RoundSliderAction, out UnityEngine.UI.Slider roundSlider, true);
            MenuHandler.CreateSlider("Points per Round", menu, 50, 0f, 30f, SetPointsConfig.Value, PointSliderAction, out UnityEngine.UI.Slider pointSlider, true);
            MenuHandler.CreateText("0 corresponds to the game mode's default value", menu, out TextMeshProUGUI _, 30);
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