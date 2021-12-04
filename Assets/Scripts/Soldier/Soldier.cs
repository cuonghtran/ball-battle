using UnityEngine;

namespace MainGame
{
    public class Soldier : MonoBehaviour
    {
        public enum SoldierType
        {
            Attacker, Defender, Penalty
        }

        public enum SoldierStatus
        {
            Inactive, Active,
            Chasing, HoldingBall, Passing, Dead
        }

        [Header("References")]
        public LayerMask groundLayer;
        public LayerMask attackerLayer;
        [SerializeField] protected SoldierStatus _soldierStatus;
        public Transform ballAttachPoint;
        [SerializeField] protected GameObject _directionIndicator;

        [Header("Information")]
        [SerializeField] protected float _baseSpeed = 240;
        public float MoveSpeed { get { return _baseSpeed * _speedModifier; } }
        [SerializeField] protected float _fallingSpeed = 150;
        [SerializeField] protected float _speedModifier = 1f;
        [SerializeField] protected float _reactivationTime;
        [SerializeField] protected float _energyCost;
        public float EnergetCost { get { return _energyCost; } }
        [SerializeField] protected Color _inActiveColor, _activeColor;
        [SerializeField] protected Renderer _renderer;

        protected Transform _opponentGoal;
        protected Rigidbody _rigidBody;
        protected Animator _animator;
        protected Vector3 _zeroVelocity = Vector3.zero;
        protected float _movementSmoothing = .05f;

        protected Quaternion _defaultRotation;
        protected bool _isGrounded = false;
        protected bool _isInteracting;
        protected int _speedParameter = Animator.StringToHash("Speed");
        protected int _kickParameter = Animator.StringToHash("Kick");
        protected int _deadParameter = Animator.StringToHash("Dead");

        protected readonly string attackerLayerMask = "AttackerSoldier";
        protected readonly string defenderLayerMask = "DefenderSoldier";
        protected readonly string ignoreCollisionMask = "IgnoreCollision";

        protected void OnEnable()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();

            _defaultRotation = transform.rotation;
            _isGrounded = false;
            _isInteracting = false;
            AdjustSoldierStatus(SoldierStatus.Active);
        }

        protected void Update()
        {
            // when summoned, soldiers fall down to the land field
            HandleFalling();
        }

        protected void LateUpdate()
        {
            _isInteracting = _animator.GetBool("IsInteracting");

        }

        private void HandleFalling()
        {
            Vector3 raycastOrigin = transform.position;

            if (!_isGrounded)
            {
                if (!_isInteracting)
                {
                    PlayTargetAnimation("Falling", true);
                }
                
                _rigidBody.velocity = Vector3.down * _fallingSpeed * Time.deltaTime;
                
                if (Physics.SphereCast(raycastOrigin, 0.4f, Vector3.down, out RaycastHit hit, groundLayer))
                {
                    if (!_isGrounded && !_isInteracting)
                    {
                        PlayTargetAnimation("Landing", true);
                    }
                    _isGrounded = true;
                    AdjustSoldierStatus(SoldierStatus.Active);
                }
                else
                {
                    _isGrounded = false;
                }
            }
        }

        protected void StopMoving()
        {
            ToggleDirectionIndicator(false);
            _rigidBody.velocity = Vector3.Lerp(_rigidBody.velocity, Vector3.zero, 0.15f);
            //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            _animator.SetFloat(_speedParameter, 0, 0.15f, 0.15f);
        }

        protected virtual void MoveToward(Vector3 targetPos, bool isSpeacialSpeed = false)
        {

        }

        protected void LookToward(Vector3 targetPos)
        {
            var rotationTarget = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotationTarget, 7 * Time.deltaTime);
        }

        public void PlayTargetAnimation(string targetAnimation, bool isInteracting)
        {
            _animator.SetBool("IsInteracting", isInteracting);
            _animator.CrossFade(targetAnimation, 0f);
        }

        protected void AdjustSoldierStatus(SoldierStatus status)
        {
            _soldierStatus = status;
            if (status == SoldierStatus.Inactive)
                _renderer.material.color = _inActiveColor;
            else _renderer.material.color = _activeColor;
        }

        protected void ChangeLayerMask(GameObject soldierToChange, string layerName)
        {
            soldierToChange.layer = LayerMask.NameToLayer(layerName);
        }

        protected float GetDistance(Vector3 from, Vector3 to)
        {
            return (to - from).magnitude;
        }

        protected void ToggleDirectionIndicator(bool isMoving)
        {
            _directionIndicator.SetActive(isMoving);
        }

        public void SetActiveColorAndGoal(Color color, Transform targetGoal)
        {
            _activeColor = color;
            _opponentGoal = targetGoal;
        }

        public SoldierStatus GetSoldierStatus()
        {
            return _soldierStatus;
        }
    }
}