using UnityEngine;
using UnityEngine.AI;

public class TestRuchu : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Wyznaczamy punkt 5 jednostek przed kotem
        Vector3 cel = transform.position + transform.forward * 5f;

        // Zlecamy agentowi ruch do tego punktu
        agent.SetDestination(cel);
        Debug.Log("Wys?ano komend? ruchu do: " + cel);
    }
}