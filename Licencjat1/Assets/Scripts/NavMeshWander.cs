using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshWander : MonoBehaviour
{
    public float wanderRadius = 10f;     // zasięg spaceru od pozycji startowej
    public float minPause = 2.5f;        // pauza po dotarciu
    public float maxPause = 5.0f;
    public float repathTimeout = 6f;     // jeśli utknie, wybierz nowy cel
    public int sampleTries = 8;        // próby znalezienia punktu na NavMeshu
    public float minNextPointDist = 1f;  // nie wybieraj punktów zbyt blisko

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
        // dotarł do celu → pauza → wybór następnego punktu
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

        // detekcja utknięcia (brak progresu)
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

    bool TrySetNewDestination()
    {
        for (int i = 0; i < sampleTries; i++)
        {
            // losuj punkt w kręgu wokół "home"
            Vector2 c = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = home + new Vector3(c.x, 0f, c.y);

            // zprojekuj na NavMesh
            if (!NavMesh.SamplePosition(candidate, out var hit, 2.0f, NavMesh.AllAreas))
                continue;

            if ((hit.position - transform.position).sqrMagnitude < minNextPointDist * minNextPointDist)
                continue;

            // sprawdź, czy ścieżka jest osiągalna
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(hit.position);
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