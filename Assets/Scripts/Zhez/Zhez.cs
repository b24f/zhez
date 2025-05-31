using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Zhez
{
    
    public class Zhez : MonoBehaviour
    {
        [Header("Detection Settings")]
        public float sightRange = 15f;
        public float sightAngle = 60f;
        public float hearingRange = 20f;
        public LayerMask obstacleLayerMask = 1;
        
        [Header("Chase Settings")]
        public float chaseSpeed = 6f;
        public float patrolSpeed = 3f;
        public float searchTime = 5f;
        public float searchRadius = 10f;
        
        [Header("References")]
        public Transform player;
        
        [Header("Patrol Settings")]
        public float patrolRadius = 5f;
        private Vector3 currentPatrolTarget;
        private bool hasPatrolTarget = false;
        
        private NavMeshAgent agent;
        private Vector3 lastKnownPlayerPosition;
        private bool isChasing = false;
        private bool isSearching = false;
        private bool hasSeenPlayer = false;
        private Vector3 originalPosition;
        private Coroutine searchCoroutine;
        
        public enum AIState
        {
            Patrol,
            Investigating,
            Chasing,
            Searching
        }
        
        public AIState currentState = AIState.Patrol;
        
        public static Zhez Current;

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
            }
            
            CheckForPlayer();
        }
        
        void HandlePatrol()
        {
            Vector3 patrolCenter = hasSeenPlayer ? lastKnownPlayerPosition : originalPosition;
    
            // If we don't have a patrol target or we've reached it, pick a new one
            if (!hasPatrolTarget || (!agent.hasPath && agent.remainingDistance < 1f))
            {
                Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
                randomDirection += patrolCenter;
                randomDirection.y = patrolCenter.y; // Keep same height
        
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
                {
                    currentPatrolTarget = hit.position;
                    agent.SetDestination(currentPatrolTarget);
                    hasPatrolTarget = true;
                }
            }
        }
        
        void HandleInvestigation()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // Reached investigation point, look around briefly then return to patrol
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
                // Lost sight of player
                hasSeenPlayer = true;
                isChasing = false;
                currentState = AIState.Searching;
                agent.SetDestination(lastKnownPlayerPosition);
                
                if (searchCoroutine != null)
                    StopCoroutine(searchCoroutine);
                searchCoroutine = StartCoroutine(SearchArea());
            }
        }
        
        void HandleSearching()
        {
            // Searching is handled by the SearchArea coroutine
            if (!isSearching)
            {
                currentState = AIState.Patrol;
                agent.speed = patrolSpeed;
            }
        }
        
        void CheckForPlayer()
        {
            if (player == null) return;
            
            // Only check for sight if not already chasing
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
            
            // Check distance
            if (distanceToPlayer > sightRange)
                return false;
            
            // Check angle
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > sightAngle / 2)
                return false;
            
            // Check for obstacles using raycast
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, distanceToPlayer, obstacleLayerMask))
            {
                if (hit.collider.transform != player)
                    return false;
            }
            
            return true;
        }
        
        public void InvestigateNoise(Vector3 noisePosition)
        {
            if (currentState == AIState.Chasing) return; // Don't interrupt chasing
            
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
            
            // Go to last known position first
            agent.SetDestination(lastKnownPlayerPosition);
            
            // Wait to reach the area
            while (agent.pathPending || agent.remainingDistance > 1f)
            {
                yield return null;
            }
            
            // Search around the area
            float searchTimer = 0f;
            while (searchTimer < searchTime)
            {
                // Pick random points around last known position
                Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
                randomDirection += lastKnownPlayerPosition;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, searchRadius, 1))
                {
                    agent.SetDestination(hit.position);
                    
                    // Wait a bit at each search point
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
        
        // Call this method from your player noise system
        public void OnPlayerMadeNoise(Vector3 noisePosition)
        {
            InvestigateNoise(noisePosition);
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw sight range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            
            // Draw hearing range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, hearingRange);
            
            // Draw sight cone
            Gizmos.color = Color.red;
            Vector3 leftBoundary = Quaternion.Euler(0, -sightAngle / 2, 0) * transform.forward * sightRange;
            Vector3 rightBoundary = Quaternion.Euler(0, sightAngle / 2, 0) * transform.forward * sightRange;
            
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}
