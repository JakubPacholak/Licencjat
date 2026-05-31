using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshWander : MonoBehaviour
{
    public float wanderRadius = 10f;
    public float minPause = 2.5f;
    public float maxPause = 5.0f;
    public float repathTimeout = 6f;
    public int sampleTries = 8;
    public float minNextPointDist = 1f;

    [Header("Animation Control")]
    public Animator animator;
    public List<string> allowedMovementStates = new List<string> { "rig_Walking", "Cat_Ball_Body_BallAnimation" };

    [Header("Ground Alignment")]
    public LayerMask groundLayer;
    public float rotationSpeed = 10f;
    public float raycastStartHeight = 1f;

    NavMeshAgent agent;
    Vector3 home;
    float pauseTimer;
    float stuckTimer;
    float lastRemaining;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        home = transform.position;
        pauseTimer = Random.Range(minPause, maxPause);
        lastRemaining = Mathf.Infinity;
    }

    void Update()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool canMove = false;

            foreach (string state in allowedMovementStates)
            {
                if (stateInfo.IsName(state))
                {
                    canMove = true;
                    break;
                }
            }

            if (!canMove)
            {
                agent.isStopped = true;
                return;
            }
            else
            {
                agent.isStopped = false;
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (pauseTimer > 0f) { pauseTimer -= Time.deltaTime; return; }
            if (TrySetNewDestination())
            {
                pauseTimer = Random.Range(minPause, maxPause);
                stuckTimer = 0f;
                lastRemaining = Mathf.Infinity;
            }
            return;
        }

        if (!agent.pathPending)
        {
            float rem = agent.remainingDistance;
            if (Mathf.Abs(rem - lastRemaining) < 0.05f) stuckTimer += Time.deltaTime;
            else stuckTimer = 0f;
            lastRemaining = rem;

            if (stuckTimer > repathTimeout)
            {
                TrySetNewDestination();
                stuckTimer = 0f;
                lastRemaining = Mathf.Infinity;
            }
        }
    }

    void LateUpdate()
    {
        if (agent != null && !agent.isStopped)
        {
            Vector3 rayStart = transform.position + (Vector3.up * raycastStartHeight);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastStartHeight + 1.5f, groundLayer))
            {
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    bool TrySetNewDestination()
    {
        for (int i = 0; i < sampleTries; i++)
        {
            Vector2 c = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = home + new Vector3(c.x, home.y, c.y);

            if (!NavMesh.SamplePosition(candidate, out var hit, 2.0f, NavMesh.AllAreas))
                continue;

            if ((hit.position - transform.position).sqrMagnitude < minNextPointDist * minNextPointDist)
                continue;

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                Vector3 newPosition = new Vector3(hit.position.x, home.y, hit.position.z);
                agent.SetDestination(newPosition);
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? home : transform.position, wanderRadius);
    }
}