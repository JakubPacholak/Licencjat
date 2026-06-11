using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshWander : MonoBehaviour
{
    public float wanderRadius = 10f;
    public float minPause = 2.5f;
    public float maxPause = 5.0f;
    public float repathTimeout = 6f;
    public int sampleTries = 8;
    public float minNextPointDist = 1f;

    public Animator mainAnimator;
    public Animator ballAnimator;
    public LayerMask groundLayer;
    public float rotationSpeed = 10f;
    public float raycastStartHeight = 1f;

    private NavMeshAgent agent;
    private Vector3 home;
    private float pauseTimer;
    private float stuckTimer;
    private float lastRemaining;

    private bool hasMainSpeed = false;
    private bool hasBallSpeed = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        home = transform.position;
        pauseTimer = Random.Range(minPause, maxPause);
        lastRemaining = Mathf.Infinity;
    }

    void Start()
    {
        if (mainAnimator != null) hasMainSpeed = HasParameter(mainAnimator, "Speed");
        if (ballAnimator != null) hasBallSpeed = HasParameter(ballAnimator, "Speed");
    }

    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    void OnEnable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            pauseTimer = 0.5f;
        }
    }

    void OnDisable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        float currentSpeed = agent.velocity.magnitude;

        if (hasMainSpeed && mainAnimator.isActiveAndEnabled) mainAnimator.SetFloat("Speed", currentSpeed);
        if (hasBallSpeed && ballAnimator.isActiveAndEnabled) ballAnimator.SetFloat("Speed", currentSpeed);

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return;
            }

            if (TrySetNewDestination())
            {
                pauseTimer = Random.Range(minPause, maxPause);
                stuckTimer = 0f;
                lastRemaining = Mathf.Infinity;
            }
            else
            {
                pauseTimer = 1f;
            }
            return;
        }
    }

    void LateUpdate()
    {
        if (agent != null && agent.enabled && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
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
        if (!agent.enabled || !agent.isOnNavMesh) return false;

        for (int i = 0; i < sampleTries; i++)
        {
            Vector2 c = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = home + new Vector3(c.x, home.y, c.y);

            if (!NavMesh.SamplePosition(candidate, out var hit, 4.0f, NavMesh.AllAreas)) continue;
            if ((hit.position - transform.position).sqrMagnitude < minNextPointDist * minNextPointDist) continue;

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                Vector3 newPosition = new Vector3(hit.position.x, hit.position.y, hit.position.z);
                agent.SetDestination(newPosition);
                return true;
            }
        }
        return false;
    }
}