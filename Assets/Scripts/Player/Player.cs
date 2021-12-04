using UnityEngine;
using UnityEngine.Events;

namespace MainGame
{
    public class Player : MonoBehaviour
    {
        public enum PlayerType
        {
            Player1, Player2, AI
        }

        [Header("References")]
        public ObjectPooler objectPooler;
        [SerializeField] private LayerMask _playGroundLayer;
        [SerializeField] private Transform _ownLandField;
        [SerializeField] private Transform _ownGoal;
        [SerializeField] private Transform _opponentGoal;
        [SerializeField] private GameObject _penaltySoldierPrefab;

        [Header("Information")]
        public string DisplayName;
        public PlayerType playerType;
        public MatchRole matchRole;

        [Header("Materials")]
        [SerializeField] private Color _activeSoldierColor;

        public UnityEvent<float> OnEnergyRegen;

        private PlayerInput _playerInput;
        private float _currentEnergy;
        private float _energyRegenRate = 0.5f;
        private readonly float _maxEnergy = 6;
        private float _energyCostToSpawn;
        private float defaultYPos = 2.7f;
        private Vector3 defaultRotation = new Vector3(0, 0, 0);
        private float _aiSpawnCooldown = 1f;
        private float _spawnTimer = 0;
        private float _aiSpawnRate = 0f;
        private readonly float AiAttackerSpawnRate = 0.4f;
        private readonly float AiDefenderSpawnRate = 0.6f;
        private Vector3 _penaltySoldierPos = new Vector3(4, 1, -6);

        // Start is called before the first frame update
        void OnEnable()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        // Update is called once per frame
        void Update()
        {
            if (matchRole == MatchRole.Spectator || matchRole == MatchRole.PenaltyTaker) return;

            if (_currentEnergy < _maxEnergy)
            {
                _currentEnergy += _energyRegenRate * Time.deltaTime;
                _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxEnergy);
                OnEnergyRegen.Invoke(_currentEnergy);
            }

            if (playerType == PlayerType.AI)
            {
                if (Time.time >= _spawnTimer)
                {
                    _spawnTimer = Time.time + _aiSpawnCooldown;
                    if (Random.value > (1 - _aiSpawnRate))
                        AISpawnHandler();
                }
            }
            else
            {
                var ray = _playerInput.GetRayFromInput();
                if (ray.HasValue)
                    CastRay(ray.Value);
            }
        }

        void CastRay(Ray ray)
        {
            if (_currentEnergy < _energyCostToSpawn) return;

            if (Physics.Raycast(ray, out RaycastHit hit, 50, _playGroundLayer))
            {
                if (hit.transform == _ownLandField)
                {
                    var spawnPos = new Vector3(hit.point.x, defaultYPos, hit.point.z);
                    SpawnSoldier(spawnPos);
                    _currentEnergy -= _energyCostToSpawn;
                }
            }
        }

        void AISpawnHandler()
        {
            if (_currentEnergy < _energyCostToSpawn) return;

            if (matchRole == MatchRole.Attacker)
                AIAttackerLogic();
            else AIDefenderLogic();
        }

        void AIAttackerLogic()
        {
            RandomlySpawnSoldierOnOwnField();
        }

        void AIDefenderLogic()
        {
            if (AttackerSoldier.attackersList.Count == 0)
            {
                RandomlySpawnSoldierOnOwnField();
            }
            else
            {
                int index = Random.Range(0, AttackerSoldier.attackersList.Count);
                var randomAttacker = AttackerSoldier.attackersList[index];

                bool validSpawnPos = false;
                while (validSpawnPos == false) // repeat until get the valid position
                {
                    Vector3 direction = (_ownGoal.position - randomAttacker.transform.position).normalized;
                    float distance = (_ownGoal.position - randomAttacker.transform.position).magnitude;
                    Vector3 spawnPos = randomAttacker.transform.position + direction * Random.Range(0, distance);
                    spawnPos = new Vector3(spawnPos.x, defaultYPos, spawnPos.z);
                    Ray ray = new Ray(spawnPos, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit, 50, _playGroundLayer))
                    {
                        if (hit.transform == _ownLandField)
                        {
                            SpawnSoldier(spawnPos);
                            _currentEnergy -= _energyCostToSpawn;
                            validSpawnPos = true;
                        }
                    }
                }
            }
        }

        void RandomlySpawnSoldierOnOwnField()
        {
            Bounds landBounds = _ownLandField.GetComponent<Renderer>().bounds;
            var landSizeOffset = landBounds.size - new Vector3(1, 0, 2); // shrink land to prevent soldiers clipping with boundary
            Vector3 positionToSpawn = new Vector3(Random.Range(landBounds.center.x - (landSizeOffset.x / 2), landBounds.center.x + (landSizeOffset.x / 2)),
                defaultYPos,
                Random.Range(landBounds.center.z - (landSizeOffset.z / 2), landBounds.center.z + (landSizeOffset.z / 2)));

            SpawnSoldier(positionToSpawn);
            _currentEnergy -= _energyCostToSpawn;
        }

        void SpawnSoldier(Vector3 spawnPos)
        {
            var soldier = matchRole == MatchRole.Attacker ? objectPooler.GetAttackerSoldier() : objectPooler.GetDefenderSoldier();
            soldier.transform.position = spawnPos;
            soldier.transform.eulerAngles = defaultRotation;
            soldier.GetComponent<Soldier>().SetActiveColorAndGoal(_activeSoldierColor, _opponentGoal);
            soldier.transform.SetParent(this.transform);
            soldier.gameObject.SetActive(true);
        }

        void SpawnPenaltySoldier(Vector3 spawnPos)
        {
            var soldier = Instantiate(_penaltySoldierPrefab, spawnPos, Quaternion.identity);
            soldier.GetComponent<Soldier>().SetActiveColorAndGoal(_activeSoldierColor, _opponentGoal);
            soldier.transform.SetParent(this.transform);
            soldier.gameObject.SetActive(true);
        }

        public void SetUpPlayer()
        {
            if (playerType == PlayerType.Player1)
                DisplayName = "Player 1 (" + matchRole.ToString() + ")";
            else
            {
                defaultRotation = new Vector3(0, 180, 0);
                DisplayName = playerType == PlayerType.AI
                    ? "Enemy - AI (" + matchRole.ToString() + ")"
                    : "Player 2 (" + matchRole.ToString() + ")";
            }

            GUIManager.OnPlayerName?.Invoke(playerType.ToString(), DisplayName);
            _currentEnergy = 0;
            
            _energyCostToSpawn = matchRole == MatchRole.Attacker ? ConstantsList.ATTACKERENERGYCOST : ConstantsList.DEFENDERENERGYCOST;
            _aiSpawnRate = matchRole == MatchRole.Attacker ? AiAttackerSpawnRate : AiDefenderSpawnRate;

            if (matchRole == MatchRole.PenaltyTaker)
                SpawnPenaltySoldier(_penaltySoldierPos);
        }
    }
}