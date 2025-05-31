using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Zhez
{
    public class Zhez : MonoBehaviour
    {
        [Header("References")]
        public Transform player;

        [Header("Vision Settings")]
        public float viewAngle = 100f;
        // public float chaseRange = 12f;
        public float losePlayerAfterSeconds = 3f;

        [Header("Idle Settings")]
        public float wanderRadius = 10f;
        public Vector2 idleTimeRange = new Vector2(3f, 8f);
        
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;

        private NavMeshAgent agent;

        // Vision
        private bool hasVisual;
        private float lastSeenTime = -Mathf.Infinity;

        // Hearing
        private Vector3 lastHeardPosition;
        private bool heardSomething;
        private bool isSearching;

        // Idle
        private float idleWanderTimer;

        // Singleton
        public static Zhez Current;

        private void Awake()
        {
            Current = this;
        }

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            ResetIdleTimer();
        }

        private void Update()
        {
            if (agent.velocity.magnitude != 0)
            {
                animator.SetBool("Walking", true);    
            }
            else
            {
                animator.SetBool("Walking", false);
            }
            
            if (ShouldChasePlayerBySight())
                return;

            // if (ShouldChasePlayerByProximity())
            //     return;

            if (ShouldContinueChasingLastSeen())
                return;

            if (ShouldSearchHeardNoise())
                return;

            HandleIdleWander();
        }

        private void OnAnimatorMove()
        {
            
        }

        private bool ShouldChasePlayerBySight()
        {
            // Debug.Log($"Can See Player {CanSeePlayer()}");
            if (CanSeePlayer())
            {
                hasVisual = true;
                lastSeenTime = Time.time;
                agent.SetDestination(player.position);
                return true;
            }
            return false;
        }

        // private bool ShouldChasePlayerByProximity()
        // {
        //     if (Vector3.Distance(transform.position, player.position) < chaseRange)
        //     {
        //         agent.SetDestination(player.position);
        //         return true;
        //     }
        //     return false;
        // }

        private bool ShouldContinueChasingLastSeen()
        {
            if (hasVisual && Time.time - lastSeenTime < losePlayerAfterSeconds)
            {
                agent.SetDestination(player.position);
                return true;
            }

            hasVisual = false;
            return false;
        }

        private bool ShouldSearchHeardNoise()
        {
            if (heardSomething && !isSearching)
            {
                StartCoroutine(SearchLastHeard());
                return true;
            }
            return false;
        }

        private void HandleIdleWander()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                idleWanderTimer -= Time.deltaTime;

                if (idleWanderTimer <= 0f)
                {
                    WanderRandomly();
                    ResetIdleTimer();
                }
            }
        }

        private bool CanSeePlayer()
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            
            Debug.DrawLine(transform.position + Vector3.up, player.position, Color.red);
            
            Vector3 startPos = transform.position + Vector3.up;
            Vector3 endPos = startPos + transform.forward * 3f;

            Debug.DrawLine(startPos, endPos, Color.green);
  
            if (angle < viewAngle / 2f)
            {
                if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out var hit, 20f))
                {
                    if (hit.transform.gameObject == player.gameObject || hit.transform.IsChildOf(player))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void HearNoise(Vector3 position)
        {
            if (!heardSomething)
            {
                lastHeardPosition = position;
                heardSomething = true;
            }
        }

        private IEnumerator SearchLastHeard()
        {
            isSearching = true;
            agent.SetDestination(lastHeardPosition);

            // Wait until reaching the heard spot
            while (agent.pathPending || agent.remainingDistance > 1f)
                yield return null;

            // Search around
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 5f;
                offset.y = 0f;

                Vector3 searchSpot = lastHeardPosition + offset;

                agent.SetDestination(searchSpot);
                yield return new WaitForSeconds(Random.Range(2f, 4f));
            }

            heardSomething = false;
            isSearching = false;
        }

        private void WanderRandomly()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius + transform.position;

            if (NavMesh.SamplePosition(randomDirection, out var hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        private void ResetIdleTimer()
        {
            idleWanderTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
        }
    }
}
