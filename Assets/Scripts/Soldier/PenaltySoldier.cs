using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class PenaltySoldier : Soldier
    {
        private PlayerInput _playerInput;
        private float _carryingBallSpeedModifier = 0.75f;
        Vector3 _targetPos;
        Vector3 _movementVelocity;

        new void OnEnable()
        {
            base.OnEnable();

            _playerInput = GetComponent<PlayerInput>();
            ChangeLayerMask(gameObject, attackerLayerMask);
            StartCoroutine(ChangeColor(0.5f));
        }

        IEnumerator ChangeColor(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            AdjustSoldierStatus(SoldierStatus.Active);
        }

        // Update is called once per frame
        new void Update()
        {
            var ray = _playerInput.GetRayFromInput();
            if (ray.HasValue)
                GetRayPosition(ray.Value);
        }

        private void FixedUpdate()
        {
            if (_targetPos != Vector3.zero)
                MoveToward(_targetPos);
        }

        void GetRayPosition(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 50, groundLayer))
            {
                _targetPos = hit.point;
            }
        }

        void MoveToward(Vector3 targetPos)
        {
            Vector3 offsetTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            if (GetDistance(_rigidBody.position, targetPos) > 0.2f)
            {
                Vector3 direction = (offsetTarget - _rigidBody.position).normalized;
                LookToward(targetPos);
                //_rigidBody.velocity = direction * _baseSpeed * _speedModifier * Time.deltaTime;
                _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, direction * _baseSpeed * _speedModifier, ref _zeroVelocity, _movementSmoothing);
                _animator.SetFloat(_speedParameter, _baseSpeed, 0.15f, 0.15f);
            }
            else
            {
                _rigidBody.velocity = Vector3.Lerp(_rigidBody.velocity, Vector3.zero, 0.15f);
                _animator.SetFloat(_speedParameter, 0, 0.15f, 0.15f);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.TryGetComponent<Ball>(out Ball ball))
            {
                if (_soldierStatus == SoldierStatus.Active)
                {
                    AdjustSoldierStatus(SoldierStatus.HoldingBall);
                    _speedModifier = _carryingBallSpeedModifier;
                    ball.AttachBall(this);
                }
            }
        }
    }
}
