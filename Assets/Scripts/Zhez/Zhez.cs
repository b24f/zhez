using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;
using Core;
using Types;

namespace Zhez
{
    public class Zhez : MonoBehaviour
    {
        [Header("Detection Settings")] public float sightRange = 15f;
        public float sightAngle = 60f;
        public float hearingRange = 20f;
        public LayerMask obstacleLayerMask = 1;

        [Header("Chase Settings")] public float chaseSpeed = 6f;
        public float patrolSpeed = 3f;
        public float searchTime = 5f;
        public float searchRadius = 10f;
        public float catchDistance = 2f; // Distance at which monster catches player

        // [Header("Game Over")] public UnityEvent onPlayerCaught; // Assign this in inspector or via code

        [Header("References")] public Transform player;

        private NavMeshAgent agent;
        private Vector3 lastKnownPlayerPosition;
        private bool isChasing = false;
        private bool isSearching = false;
        private bool hasSeenPlayer = false;
        private Vector3 originalPosition;
        private Coroutine searchCoroutine;
        private bool playerCaught = false; // Prevent multiple game over calls

        public enum AIState
        {
            Patrol,
            Investigating,
            Chasing,
            Searching,
            PlayerCaught
        }

        public AIState currentState = AIState.Patrol;

        static public Zhez Current;

        private void Awake()
        {
            Current = this;
        }

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            originalPosition = transform.position;
            agent.speed = patrolSpeed;

            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        void Update()
        {
            if (playerCaught) return; // Stop all AI behavior if player is caught

            CheckIfPlayerCaught();

            switch (currentState)
            {
                case AIState.Patrol:
                    HandlePatrol();
                    break;
                case AIState.Investigating:
                    HandleInvestigation();
                    break;
                case AIState.Chasing:
                    HandleChasing();
                    break;
                case AIState.Searching:
                    HandleSearching();
                    break;
                case AIState.PlayerCaught:
                    // Monster has caught player, stop moving
                    agent.isStopped = true;
                    break;
            }

            if (currentState != AIState.PlayerCaught)
            {
                CheckForPlayer();
            }
        }

        void CheckIfPlayerCaught()
        {
            if (player == null || playerCaught) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= catchDistance)
            {
                PlayerCaught();
            }
        }

        void PlayerCaught()
        {
            if (playerCaught) return; // Prevent multiple calls

            playerCaught = true;
            currentState = AIState.PlayerCaught;
            agent.isStopped = true;

            // Trigger game over event
            EventEmitter.EmitStateChange(State.GameOver);
        }

        // Rest of your existing methods remain the same...
        void HandlePatrol()
        {
            Vector3 patrolTarget = hasSeenPlayer ? lastKnownPlayerPosition : originalPosition;

            if (!agent.hasPath && Vector3.Distance(transform.position, patrolTarget) > 1f)
            {
                agent.SetDestination(patrolTarget);
            }
        }

        void HandleInvestigation()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                StartCoroutine(LookAround(2f));
            }
        }

        void HandleChasing()
        {
            if (CanSeePlayer())
            {
                lastKnownPlayerPosition = player.position;
                agent.SetDestination(player.position);
            }
            else
            {
                if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    hasSeenPlayer = true;
                    isChasing = false;
                    currentState = AIState.Searching;
                    agent.SetDestination(lastKnownPlayerPosition);

                    if (searchCoroutine != null)
                        StopCoroutine(searchCoroutine);
                    searchCoroutine = StartCoroutine(SearchArea());
                }
                else
                {
                    agent.SetDestination(lastKnownPlayerPosition);
                }
            }
        }

        void HandleSearching()
        {
            if (!isSearching)
            {
                currentState = AIState.Patrol;
                agent.speed = patrolSpeed;
            }
        }

        void CheckForPlayer()
        {
            if (player == null) return;

            if (currentState != AIState.Chasing && CanSeePlayer())
            {
                StartChasing();
            }
        }

        bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > sightRange)
                return false;

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float allowedAngle = distanceToPlayer < 2f ? sightAngle * 1.5f : sightAngle;

            if (angleToPlayer > allowedAngle / 2)
                return false;

            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up;
            Vector3 rayTarget = player.position + Vector3.up;

            if (Physics.Raycast(rayStart, (rayTarget - rayStart).normalized, out hit, distanceToPlayer,
                    obstacleLayerMask))
            {
                if (hit.collider.transform != player)
                    return false;
            }

            return true;
        }

        public void InvestigateNoise(Vector3 noisePosition)
        {
            if (currentState == AIState.Chasing || playerCaught) return;

            float distanceToNoise = Vector3.Distance(transform.position, noisePosition);

            if (distanceToNoise <= hearingRange)
            {
                currentState = AIState.Investigating;
                agent.speed = patrolSpeed;
                agent.SetDestination(noisePosition);
            }
        }

        void StartChasing()
        {
            currentState = AIState.Chasing;
            isChasing = true;
            agent.speed = chaseSpeed;
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(player.position);
        }

        IEnumerator SearchArea()
        {
            isSearching = true;
            agent.speed = patrolSpeed;

            agent.SetDestination(lastKnownPlayerPosition);

            while (agent.pathPending || agent.remainingDistance > 1f)
            {
                yield return null;
            }

            float searchTimer = 0f;
            while (searchTimer < searchTime)
            {
                Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
                randomDirection += lastKnownPlayerPosition;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, searchRadius, 1))
                {
                    agent.SetDestination(hit.position);
                    yield return new WaitForSeconds(1f);
                }

                searchTimer += 1f;
            }

            isSearching = false;
            hasSeenPlayer = false;
        }

        IEnumerator LookAround(float duration)
        {
            Vector3 originalRotation = transform.eulerAngles;
            float timer = 0f;

            while (timer < duration)
            {
                transform.Rotate(0, 45f * Time.deltaTime, 0);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.eulerAngles = originalRotation;
            currentState = AIState.Patrol;
        }

        public void OnPlayerMadeNoise(Vector3 noisePosition)
        {
            InvestigateNoise(noisePosition);
        }

        void OnDrawGizmosSelected()
        {
            // Draw catch distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchDistance);

            // Draw sight range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);

            // Draw hearing range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, hearingRange);

            // Draw sight cone
            Gizmos.color = Color.green;
            Vector3 leftBoundary = Quaternion.Euler(0, -sightAngle / 2, 0) * transform.forward * sightRange;
            Vector3 rightBoundary = Quaternion.Euler(0, sightAngle / 2, 0) * transform.forward * sightRange;

            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}