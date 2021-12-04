using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class DefenderSoldier : Soldier
    {
        [SerializeField] protected GameObject _visionIndicator;
        private Transform _holdingBallAttacker;
        private float _detectionRadius;
        private Vector3 _originalPosition;
        private float _returningSpeedModifier = 2f;
        private float _scanCooldown = 0.1f;
        private float _scanTimer = 0;
        private float _reactivateTimer = 0;

        new void OnEnable()
        {
            base.OnEnable();
            ChangeLayerMask(gameObject, defenderLayerMask);
            _originalPosition = new Vector3(transform.position.x, 1, transform.position.z);
            _detectionRadius = ConstantsList.BOARDWIDTH * 0.35f;
        }

        void FixedUpdate()
        {
            if (_isInteracting || !_isGrounded) return;

            switch (_soldierStatus)
            {
                case SoldierStatus.Chasing:
                    // chasing after the ball
                    ChasingTarget();
                    break;
                case SoldierStatus.Active:
                    // stand still and scan for ball
                    ScanForAttackerWithBall();
                    break;
            }
        }

        new void Update()
        {
            base.Update();

            if (_isInteracting || !_isGrounded) return;

            if (_soldierStatus == SoldierStatus.Inactive)
            {
                MoveToward(_originalPosition, true);

                if (_reactivateTimer <= _reactivationTime)
                    _reactivateTimer += Time.deltaTime;
                else
                {
                    ToggleVisionIndicator(true);
                    AdjustSoldierStatus(SoldierStatus.Active);
                    ChangeLayerMask(gameObject, defenderLayerMask);
                    _reactivateTimer = 0;
                }
            }
        }

        private void ChasingTarget()
        {
            _holdingBallAttacker = ScanForTarget();
            if (_holdingBallAttacker != null)
            {
                MoveToward(_holdingBallAttacker.position);
                _animator.SetFloat(_speedParameter, _baseSpeed, 0.2f, 0.2f);
            }
            else AdjustSoldierStatus(SoldierStatus.Active);
        }

        private void ScanForAttackerWithBall()
        {
            StopMoving();
            // limit number of times a defender can scan for attack for performance reason
            if (Time.time >= _scanTimer) 
            {
                _scanTimer = Time.time + _scanCooldown;
                var attacker = ScanForTarget();
                if (attacker != null)
                {
                    _holdingBallAttacker = attacker;
                    AdjustSoldierStatus(SoldierStatus.Chasing);
                    ToggleDirectionIndicator(true);
                }
                ToggleVisionIndicator(true);
            }
        }

        private Transform ScanForTarget()
        {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, _detectionRadius, attackerLayer);
            foreach(Collider collider in colliderArray)
            {
                if (collider.transform.TryGetComponent<AttackerSoldier>(out AttackerSoldier attacker))
                    if (attacker.GetSoldierStatus() == SoldierStatus.HoldingBall)
                        return collider.transform;
            }
            return null;
        }

        protected override void MoveToward(Vector3 targetPos, bool returning = false)
        {
            if (GetDistance(transform.position, targetPos) < 0.5f)
            {
                ToggleDirectionIndicator(false);
                StopMoving();
            }
            else
            {
                ToggleDirectionIndicator(true);
                Vector3 offsetTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
                Vector3 direction = (offsetTarget - _rigidBody.position).normalized;
                LookToward(targetPos);
                float speedMod = returning ? _returningSpeedModifier : _speedModifier;
                _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, direction * _baseSpeed * speedMod, ref _zeroVelocity, _movementSmoothing);
            }
        }

        private void ToggleVisionIndicator(bool isActive)
        {
            _visionIndicator.SetActive(isActive);
        }

        public IEnumerator SetInactiveWithDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _reactivateTimer = 0;
            ChangeLayerMask(gameObject, ignoreCollisionMask);
            AdjustSoldierStatus(SoldierStatus.Inactive);
            ToggleVisionIndicator(false);
            ToggleDirectionIndicator(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
    }
}
