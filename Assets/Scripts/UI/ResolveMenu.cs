using System;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.Rendering;

namespace MainGame
{
    public class ResolveMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ObjectPooler _soldierPooler;
        [SerializeField] private TMP_Text _player1Name_Text;
        [SerializeField] private TMP_Text _player2Name_Text;
        [SerializeField] private Volume _blurVolume;

        [Header("UI")]
        [SerializeField] private GameObject _resolvePanel;
        [SerializeField] private GameObject _continueButton;
        [SerializeField] private GameObject _exitButton;
        [SerializeField] private TMP_Text _description_Text;
        [SerializeField] private TMP_Text _matchResult_Text;

        public static bool gameIsPaused;
        public static Action<bool, Transform> OnMatchEnd;

        bool _penaltyTrigger = false;

        private void OnEnable()
        {
            OnMatchEnd += ResolveSequence;
        }

        private void OnDisable()
        {
            OnMatchEnd -= ResolveSequence;
        }

        private void ResolveSequence(bool gameEnded, Transform winner)
        {
            StringBuilder matchResultString = new StringBuilder();
            if (winner != null)
            {
                if (winner.TryGetComponent<Player>(out Player winPlayer))
                {
                    var name = winPlayer.DisplayName.Split('(')[0];
                    _description_Text.text = name.Trim();
                }
            }

            if (gameEnded)
            {
                if (winner != null)
                {
                    matchResultString.Append("Won the game!");
                    _continueButton.SetActive(false);
                    _exitButton.SetActive(true);
                }
                else
                {
                    _description_Text.text = "Nobody won!";
                    matchResultString.Append("Penalty match");
                    _continueButton.SetActive(true);
                    _exitButton.SetActive(false);
                    _penaltyTrigger = true;
                }
            }
            else
            {
                if (winner != null)
                    matchResultString.Append("Won this match!");
                else matchResultString.Append("Draw!");
                _exitButton.SetActive(false);
            }
            
            _matchResult_Text.text = matchResultString.ToString();
            _resolvePanel.SetActive(true);
            _blurVolume.weight = 1;

            PauseGame(true);
        }

        private void PauseGame(bool isPaused)
        {
            gameIsPaused = isPaused;
            if (gameIsPaused)
                Time.timeScale = 0;
            else Time.timeScale = 1;
        }

        public void OnContinue_Click()
        {
            _soldierPooler.ResetAllSoldier();
            GameManager.StartMatch(_penaltyTrigger);
            _resolvePanel.SetActive(false);
            _blurVolume.weight = 0;
            PauseGame(false);
        }

        public void OnExit_Click()
        {
            PauseGame(false);
            SceneController.Instance.FadeAndLoadScene(ConstantsList.Scenes["OpeningScene"]);
        }
    }
}
