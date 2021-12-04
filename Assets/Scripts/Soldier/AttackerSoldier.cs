using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MainGame
{
    public class AttackerSoldier : Soldier
    {
        [SerializeField] protected GameObject _holdingBallHighlight;
        public static List<AttackerSoldier> attackersList = new List<AttackerSoldier>();
        private Ball _ballInPlay;
        private float _reactivateTimer = 0;
        private float _carryingBallSpeedModifier = 0.75f;

        new void OnEnable()
        {
            base.OnEnable();
            ChangeLayerMask(gameObject, attackerLayerMask);
            attackersList.Add(this);
        }

        private void OnDisable()
        {
            attackersList.Remove(this);
        }

        void FixedUpdate()
        {
            if (_isInteracting || !_isGrounded) return;

            switch (_soldierStatus)
            {
                case SoldierStatus.Chasing:
                    // chasing after the ball
                    ChasingBall();
                    break;
                case SoldierStatus.HoldingBall:
                    // move to opponent goal
                    MoveToward(_opponentGoal.transform.position, true);
                    break;
                case SoldierStatus.Active:
                    // while not chasing and holding, move straight
                    MoveStraight();
                    break;
            }
        }

        new void Update()
        {
            base.Update();

            if (_isInteracting || !_isGrounded) return;

            if (_soldierStatus == SoldierStatus.Active)
                ScanForBall();

            if (_soldierStatus == SoldierStatus.Inactive)
            {
                StopMoving();

                if (_reactivateTimer <= _reactivationTime)
                    _reactivateTimer += Time.deltaTime;
                else
                {
                    transform.rotation = _defaultRotation;
                    AdjustSoldierStatus(SoldierStatus.Active);
                    ChangeLayerMask(this.gameObject, attackerLayerMask);
                    _reactivateTimer = 0;
                }
            }
        }

        new void LateUpdate()
        {
            base.LateUpdate();

            ToggleHighlight(_soldierStatus == SoldierStatus.HoldingBall);
        }

        public void SetSoldierHoldingBall(bool holdingBall)
        {
            if (holdingBall)
            {
                ToggleDirectionIndicator(true);
                AdjustSoldierStatus(SoldierStatus.HoldingBall);
            }
        }

        private void ScanForBall()
        {
            ToggleDirectionIndicator(true);
            if (Ball.singleton != null && Ball.singleton.ballState == Ball.BallState.Free)
            {
                _ballInPlay = Ball.singleton;
                if (_ballInPlay != null && _ballInPlay.ballState == Ball.BallState.Free)
                {
                    AdjustSoldierStatus(SoldierStatus.Chasing);
                }
            }

            //Collider[] ballCollider = Physics.OverlapSphere(transform.position, ConstantsList.BOARDHEIGHT * 10, ballLayerMask);
            //if (ballCollider.Length > 0)
            //{
            //    _ballInPlay = ballCollider[0].GetComponent<Ball>();
            //    if (_ballInPlay != null && _ballInPlay.ballState == Ball.BallState.Free)
            //    {
            //        AdjustSoldierStatus(SoldierStatus.Chasing);
            //    }

            //}
        }

        private void ChasingBall()
        {
            if (_ballInPlay != null && _ballInPlay.ballState == Ball.BallState.Free)
            {
                MoveToward(_ballInPlay.transform.position);
                _animator.SetFloat(_speedParameter, _baseSpeed, 0.2f, 0.2f);
            }
            else
            {
                StopMoving();
                _reactivateTimer = 0;
                AdjustSoldierStatus(SoldierStatus.Inactive);
                ChangeLayerMask(gameObject, ignoreCollisionMask);
            }
        }

        protected override void MoveToward(Vector3 targetPos, bool isSpeacialSpeed = false)
        {
            ToggleDirectionIndicator(true);
            Vector3 offsetTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            Vector3 direction = (offsetTarget - _rigidBody.position).normalized;
            LookToward(targetPos);
            float speedMod = isSpeacialSpeed ? _carryingBallSpeedModifier : _speedModifier;
            _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, direction * _baseSpeed * speedMod, ref _zeroVelocity, _movementSmoothing);
        }

        private void MoveStraight()
        {
            _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, transform.forward * _baseSpeed * _speedModifier, ref _zeroVelocity, _movementSmoothing);
            _animator.SetFloat(_speedParameter, _baseSpeed, 0.2f, 0.2f);
        }

        public IEnumerator GetCaughtSequence(Transform collidedDefender)
        {
            ToggleDirectionIndicator(false);
            var teammate = GetNearestActiveAttacker();
            if (teammate != null)
            {
                yield return StartCoroutine(PassBallToTeammate(teammate));
                yield return StartCoroutine(DeactivateAttacker(0f));
            }
            else
            {
                // die
                Die();
                _ballInPlay.OnBallFreed?.Invoke();
                StartCoroutine(CallEndMatchWithDelay(1.25f, collidedDefender));
            }
        }

        private void Die()
        {
            AdjustSoldierStatus(SoldierStatus.Dead);
            StopMoving();
            _animator.SetTrigger(_deadParameter);
            ChangeLayerMask(gameObject, ignoreCollisionMask);
            StartCoroutine(DestroyAttacker(2f));
        }

        private IEnumerator PassBallToTeammate(Transform teammate)
        {
            yield return null;
            AdjustSoldierStatus(SoldierStatus.Passing);
            LookToward(teammate.position);
            _animator.SetTrigger(_kickParameter);
            _ballInPlay.OnBallGetKicked(teammate, _baseSpeed * _speedModifier);
        }

        private IEnumerator DeactivateAttacker(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _reactivateTimer = 0;
            AdjustSoldierStatus(SoldierStatus.Inactive);
            ChangeLayerMask(gameObject, ignoreCollisionMask);
        }

        private IEnumerator DestroyAttacker(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            gameObject.SetActive(false);
            transform.SetParent(null);
        }

        private Transform GetNearestActiveAttacker()
        {
            Collider[] attackers = Physics.OverlapSphere(transform.position, ConstantsList.BOARDHEIGHT, attackerLayer);
            var activeActtackers = attackers
                .Where(x => x.GetComponent<AttackerSoldier>().GetSoldierStatus() != SoldierStatus.Inactive
                && x.transform != transform).ToArray();

            Array.Sort(activeActtackers, new DistanceComparer(transform));
            if (activeActtackers.Length > 0)
                return activeActtackers[0].transform;
            return null;
        }

        IEnumerator CallEndMatchWithDelay(float seconds, Transform collidedDefender)
        {
            yield return new WaitForSeconds(seconds);
            MatchManger.OnPlayerWin?.Invoke(collidedDefender.parent);
        }

        private void ToggleHighlight(bool value)
        {
            _holdingBallHighlight.SetActive(value);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.TryGetComponent<DefenderSoldier>(out DefenderSoldier defenderSoldier))
            {
                if (_soldierStatus == SoldierStatus.HoldingBall)
                {
                    defenderSoldier.PlayTargetAnimation("Punch", true);
                    StartCoroutine(defenderSoldier.SetInactiveWithDelay(1f));
                    StartCoroutine(GetCaughtSequence(defenderSoldier.transform));
                }
            }

            if (collision.transform.TryGetComponent<Ball>(out Ball ball))
            {
                if (_soldierStatus == SoldierStatus.Chasing || _soldierStatus == SoldierStatus.Active)
                {
                    SetSoldierHoldingBall(true);
                    ball.AttachBall(this);
                }
            }

            if (collision.transform.TryGetComponent<PlayerFence>(out PlayerFence playerFence))
            {
                Die();
            }
        }
    }
}
