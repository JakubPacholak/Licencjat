using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ModelSwapper : MonoBehaviour
{
    [Header("Model Zwykly (Stojacy/Interakcje)")]
    [Tooltip("Przeciagnij tu obiekty siatek normalnego kota (np. M_Cat_Body i M_Cat_Parts)")]
    public GameObject[] normalCatMeshes;

    [Header("Model Kulki (W ruchu)")]
    [Tooltip("Przeciagnij tu g?owny obiekt kuli (F_Catcreature_Ball)")]
    public GameObject ballCatRoot;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        bool isMoving = agent.isActiveAndEnabled && agent.velocity.magnitude > 0.1f;

        if (ballCatRoot != null && ballCatRoot.activeSelf != isMoving)
        {
            ballCatRoot.SetActive(isMoving);
        }
        foreach (GameObject mesh in normalCatMeshes)
        {
            if (mesh != null && mesh.activeSelf == isMoving)
            {
                mesh.SetActive(!isMoving);
            }
        }
    }
}