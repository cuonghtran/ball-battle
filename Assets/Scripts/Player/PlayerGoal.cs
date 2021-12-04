using System.Collections;
using UnityEngine;

namespace MainGame
{
    public class PlayerGoal : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _goalParticles;

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent<Ball>(out Ball ball))
            {
                if (ball.ballState == Ball.BallState.Free) return;

                ball.ballState = Ball.BallState.Goal;
                var playerInPossession = ball.PlayerInPossession;
                StartCoroutine(GoalSequence(ball.gameObject, playerInPossession));
                _goalParticles.Play();
            }
        }

        IEnumerator GoalSequence(GameObject ballObject, Transform playerInPossession)
        {
            yield return new WaitForSeconds(0.13f);
            Destroy(ballObject);
            yield return new WaitForSeconds(0.13f);
            MatchManger.OnPlayerWin?.Invoke(playerInPossession);
        }
    }
}
