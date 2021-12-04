using System;
using UnityEngine;

namespace MainGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameMode gameMode;

        public static Action<bool> OnMatchStarted;
        

        private void Awake()
        {
            Application.targetFrameRate = 30;
            Invoke(nameof(Theme), 0.5f);

            DontDestroyOnLoad(this);
        }

        public static void StartMatch(bool isPenalty = false)
        {
            OnMatchStarted.Invoke(isPenalty);
        }

        void Theme()
        {
            AudioManager.SharedInstance.PlayTheme("Main_Theme");
        }
    }

    public enum GameMode
    {
        PVE, PVP
    }
}