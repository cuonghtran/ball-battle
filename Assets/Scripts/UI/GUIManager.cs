using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

namespace MainGame
{
    public class GUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _player1_Text;
        [SerializeField] private TMP_Text _player2_Text;
        [SerializeField] private TMP_Text _timer_Text;
        [SerializeField] private TMP_Text _score_Text;
        [SerializeField] private Image[] _player1EnergyBars = new Image[6];
        [SerializeField] private Image[] _player2EnergyBars = new Image[6];
        public TMP_Text ball_Text;

        private float _duration = 0;
        private bool _matchPlaying = false;
        private bool _isPenaltyMatch = false;
        private int _currentWholeEnergies1 = 0;
        private int _currentWholeEnergies2 = 0;

        public static Action<int[]> OnUpdateScore;
        public static Action<string, string> OnPlayerName;
        public static Action<string> OnBallDetected;

        private void OnEnable()
        {
            GameManager.OnMatchStarted += StartTimer;
            OnUpdateScore += UpdateScore;
            OnPlayerName += SetPlayersName;
            OnBallDetected = SetBallName;
        }

        private void OnDisable()
        {
            GameManager.OnMatchStarted -= StartTimer;
            OnUpdateScore -= UpdateScore;
            OnPlayerName -= SetPlayersName;
        }

        private void SetBallName(string name)
        {
            //ball_Text.text = ball_Text.text + ", " + name;
        }

        private void Update()
        {
            if (_matchPlaying)
            {
                if (_duration > 0)
                {
                    _duration -= Time.deltaTime;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(Mathf.RoundToInt(_duration).ToString());
                    sb.Append("s");
                    _timer_Text.text = sb.ToString();
                }
                else
                {
                    _duration = 0;
                    _timer_Text.text = "0s";
                    _matchPlaying = false;
                    MatchManger.OnPlayerWin?.Invoke(null);
                }
            }
        }

        #region Match Info

        private void StartTimer(bool isPenalty)
        {
            float matchDuration = isPenalty ? 100 : ConstantsList.MATCHDURATION;
            _duration = matchDuration;
            _matchPlaying = true;
            _isPenaltyMatch = isPenalty;
        }

        private void SetPlayersName(string playerType, string name)
        {
            if (playerType.ToLower() == "player1")
                _player1_Text.text = name;
            else _player2_Text.text = name;
        }

        #endregion

        #region Scores

        private void UpdateScore(int[] gameScore)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(gameScore[0]);
            sb.Append(" - ");
            sb.Append(gameScore[1]);
            _score_Text.text = sb.ToString();
        }

        #endregion

        #region Energy

        public void DisplayCurrentEnergyForPlayer1(float energy)
        {
            int wholeEnergies = Mathf.FloorToInt(energy);
            float fillingEnergy = energy - wholeEnergies;

            if (_currentWholeEnergies1 != wholeEnergies)
            {
                _currentWholeEnergies1 = wholeEnergies;
                UpdateWholeEnergies(_currentWholeEnergies1, _player1EnergyBars);
            }

            UpdateFillingEnergy(fillingEnergy, _currentWholeEnergies1, _player1EnergyBars);
        }

        public void DisplayCurrentEnergyForPlayer2(float energy)
        {
            int wholeEnergies = Mathf.FloorToInt(energy);
            float fillingEnergy = energy - wholeEnergies;

            if (_currentWholeEnergies2 != wholeEnergies)
            {
                _currentWholeEnergies2 = wholeEnergies;
                UpdateWholeEnergies(_currentWholeEnergies2, _player2EnergyBars);
            }

            UpdateFillingEnergy(fillingEnergy, _currentWholeEnergies2, _player2EnergyBars);
        }

        private void UpdateWholeEnergies(int wholeEnergies, Image[] playerEnergyBar)
        {
            Array.ForEach(playerEnergyBar, (e) => e.fillAmount = 0);

            for (int i = 0; i < wholeEnergies; i++)
            {
                playerEnergyBar[i].fillAmount = 1;
            }
        }

        private void UpdateFillingEnergy(float fillingEnergy, int wholeEnergies, Image[] playerEnergyBar)
        {
            if (fillingEnergy <= 0f) return;
            if (wholeEnergies == 6) Debug.Log(fillingEnergy);
            playerEnergyBar[wholeEnergies].fillAmount = fillingEnergy;
        }

        #endregion

        public void OnArButton_Click()
        {
            SceneController.Instance.FadeAndLoadScene(ConstantsList.Scenes["ARScene"]);
        }

        public void OnExitButton_Click()
        {
            SceneController.Instance.FadeAndLoadScene(ConstantsList.Scenes["OpeningScene"]);
        }
    }
}