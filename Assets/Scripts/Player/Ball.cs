using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class Ball : MonoBehaviour
    {
        public enum BallState
        {
            Free, OnHold, Goal
        }
        public static Ball singleton;
        public BallState ballState;
        private Transform target;
        [SerializeField] float ballVelocity = 100;
        [SerializeField] private Rigidbody _rigidBody;
        private float _passingSpeed;
        private Vector3 _zeroVelocity = Vector3.zero;
        private float _movementSmoothing = .05f;
        private Transform _playerInPosession;
        public Transform PlayerInPossession { get { return _playerInPosession; } }

        private readonly string ballLayerMask = "Ball";
        private readonly string ignoreCollisionMask = "IgnoreCollision";

        public Action OnBallFreed;
        public Action<Transform, float> OnBallGetKicked;

        private void Awake()
        {
            singleton = this;
            ballState = BallState.Free;
        }

        private void OnEnable()
        {
            OnBallFreed += FreeBall;
            OnBallGetKicked += KickBall;
        }

        private void OnDisable()
        {
            OnBallFreed -= FreeBall;
            OnBallGetKicked -= KickBall;
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            if (ballState == BallState.OnHold)
            {
                Vector3 direction = (target.position - _rigidBody.position).normalized;
                _rigidBody.AddForce(direction * ballVelocity * Time.deltaTime);
            }

            if (ballState == BallState.Free)
            {
                Vector3 direction = (target.position - _rigidBody.position).normalized;
                //_rigidBody.velocity = direction * _passingSpeed * Time.deltaTime;
                _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, direction * _passingSpeed, ref _zeroVelocity, _movementSmoothing);
            }
        }

        private void FreeBall()
        {
            ballState = BallState.Free;
            transform.SetParent(null);
            target = null;
            gameObject.layer = LayerMask.NameToLayer(ballLayerMask);
        }

        private void KickBall(Transform targetPlayer, float force)
        {
            FreeBall();
            target = targetPlayer;
            _passingSpeed = force;
        }

        public void AttachBall(Soldier attackerSoldier)
        {
            _playerInPosession = attackerSoldier.transform.parent;  // set the attacking player controlling the ball
            transform.SetParent(attackerSoldier.transform);
            target = attackerSoldier.ballAttachPoint;  // relative position for the soldier to dribble the bll
            ballState = BallState.OnHold;
            gameObject.layer = LayerMask.NameToLayer(ignoreCollisionMask);
        }
    }
}
