using System;
using UnityEngine;

namespace MainGame
{
    public class MatchManger : MonoBehaviour
    {
        [SerializeField] private Player _player1;
        [SerializeField] private Player _player2;
        [SerializeField] private MazeGenerator _mazeGenerator;
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private Transform _player1LandField;
        [SerializeField] private Transform _player2LandField;
        [SerializeField] private Transform _totalLandField;

        public static Action<int[]> OnUpdateScore;
        public static Action<Transform> OnPlayerWin;

        private int _currentMatch = 0;
        private int _maxMatches = ConstantsList.MAXMATCHES;
        private int[] _gameScore = new int[2];

        private void OnEnable()
        {
            GameManager.OnMatchStarted += SetUpMatch;
            OnPlayerWin += EndMatch;
        }

        private void OnDisable()
        {
            GameManager.OnMatchStarted -= SetUpMatch;
            OnPlayerWin -= EndMatch;
        }

        void SetUpMatch(bool isPenalty = false)
        {
            if (GameManager.gameMode == GameMode.PVE)
                _player2.playerType = Player.PlayerType.AI;
            else _player2.playerType = Player.PlayerType.Player2;

            Bounds landBounds;

            if (_currentMatch % 2 == 0)
            {
                _player1.matchRole = MatchRole.Attacker;
                _player2.matchRole = MatchRole.Defender;
                landBounds = _player1LandField.GetComponent<Renderer>().bounds;
            }
            else
            {
                _player1.matchRole = MatchRole.Defender;
                _player2.matchRole = MatchRole.Attacker;
                landBounds = _player2LandField.GetComponent<Renderer>().bounds;
            }

            if (_currentMatch == 5) // penalty match
            {
                _mazeGenerator.CreateWalls();
                _player1.matchRole = MatchRole.PenaltyTaker;
                _player2.matchRole = MatchRole.Spectator;
                landBounds = _totalLandField.GetComponent<Renderer>().bounds;
            }
            SpawnBall(landBounds.center, landBounds.size);

            _player1.SetUpPlayer();
            _player2.SetUpPlayer();
        }

        void SpawnBall(Vector3 landCenter, Vector3 landSize)
        {
            var landSizeOffset = landSize - new Vector3(2, 0, 3); // shrink the land to prevent the ball to close to boundary
            Vector3 positionToSpawn = new Vector3(UnityEngine.Random.Range(landCenter.x - (landSizeOffset.x / 2), landCenter.x + (landSizeOffset.x / 2)),
                2,
                UnityEngine.Random.Range(landCenter.z - (landSizeOffset.z / 2), landCenter.z + (landSizeOffset.z / 2)));

            var oldBall = GameObject.FindObjectOfType<Ball>();
            if (oldBall)
                Destroy(oldBall.gameObject);
            Instantiate(_ballPrefab, positionToSpawn, Quaternion.identity);
        }

        public void EndMatch(Transform winPlayer)
        {
            if (winPlayer != null)
            {
                if (winPlayer == _player1.transform)
                    _gameScore[0]++;
                else _gameScore[1]++;
            }

            if (winPlayer == null && _currentMatch == 5) // penalty match and attacker failed
            {
                _gameScore[1]++;
                winPlayer = _player2.transform;
            }

            GUIManager.OnUpdateScore?.Invoke(_gameScore);

            if (_currentMatch < _maxMatches)
                _currentMatch++;

            bool gameEnded = _currentMatch >= _maxMatches || _gameScore[0] >= 3 || _gameScore[1] >= 3 ? true : false;

            ResolveMenu.OnMatchEnd?.Invoke(gameEnded, winPlayer);
        }
    }

    public enum MatchRole
    {
        Attacker, Defender, PenaltyTaker, Spectator
    }
}
