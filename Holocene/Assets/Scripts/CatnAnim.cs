using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatInteractions : MonoBehaviour
{
    [System.Serializable]
    public struct BuildingAnimationLink
    {
        public BuildingData requiredBuilding;
        public string animationTriggerName;
        public float interactionDistance;

        [Header("Korekty Pozycji i Rotacji")]
        [Tooltip("Przesuni?cie wzgl?dem punktu interakcji budynku.")]
        public Vector3 positionOffset;
        [Tooltip("Dodatkowy obrót kota (w stopniach).")]
        public Vector3 rotationOffset;
        [Tooltip("Zaznacz, by kot le?a? p?asko (przydatne przy rig_sleep).")]
        public bool forceFlatRotation;
    }

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NavMeshWander wanderScript;

    [Header("Settings")]
    [SerializeField] private float animationInterval = 20f;

    [Tooltip("Stany, do których kot wraca po interakcji (z Twojego folderu animacji).")]
    [SerializeField] private List<string> returnStateNames = new List<string> { "rig_Idle", "rig_Walking" };

    [SerializeField] private List<BuildingAnimationLink> buildingAnimations;

    private float timer = 0f;
    private bool isInteracting = false;

    private void Update()
    {
        if (isInteracting) return;

        timer += Time.deltaTime;

        if (timer >= animationInterval)
        {
            timer = 0f;
            TryStartInteraction();
        }
    }

    private void TryStartInteraction()
    {
        if (animator == null || agent == null) return;

        var validBuildings = new List<(Building building, BuildingAnimationLink link)>();

        foreach (var building in Building.ActiveBuildings)
        {
            foreach (var link in buildingAnimations)
            {
                if (building.Data == link.requiredBuilding)
                {
                    validBuildings.Add((building, link));
                }
            }
        }

        if (validBuildings.Count > 0)
        {
            int randomIndex = Random.Range(0, validBuildings.Count);
            var selected = validBuildings[randomIndex];

            StartCoroutine(InteractionRoutine(selected.building, selected.link));
        }
    }

    private IEnumerator InteractionRoutine(Building targetBuilding, BuildingAnimationLink link)
    {
        isInteracting = true;

        if (wanderScript != null) wanderScript.enabled = false;

        agent.isStopped = false;
        agent.SetDestination(targetBuilding.transform.position);

        float originalStoppingDist = agent.stoppingDistance;
        float targetDist = link.interactionDistance > 0f ? link.interactionDistance : 1.5f;
        agent.stoppingDistance = targetDist;

        while (targetBuilding != null && (agent.pathPending || agent.remainingDistance > targetDist + 0.1f))
        {
            yield return null;
        }

        if (targetBuilding == null)
        {
            EndInteraction(originalStoppingDist);
            yield break;
        }

        agent.isStopped = true;

        Transform interactionPoint = FindInteractionPoint(targetBuilding.transform);
        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;
        bool snappedToPoint = false;

        if (interactionPoint != null)
        {
            agent.enabled = false;

            transform.position = interactionPoint.position + interactionPoint.TransformDirection(link.positionOffset);

            Quaternion baseRot = link.forceFlatRotation
                ? Quaternion.Euler(0, interactionPoint.rotation.eulerAngles.y, 0)
                : interactionPoint.rotation;

            transform.rotation = baseRot * Quaternion.Euler(link.rotationOffset);
            snappedToPoint = true;
        }
        else
        {
            Vector3 directionToBuilding = (targetBuilding.transform.position - transform.position).normalized;
            directionToBuilding.y = 0;
            if (directionToBuilding != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToBuilding);
            }
        }

        animator.SetTrigger(link.animationTriggerName);

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (animator == null) break;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isFinished = false;

            foreach (string state in returnStateNames)
            {
                if (stateInfo.IsName(state))
                {
                    isFinished = true;
                    break;
                }
            }

            if (isFinished || (snappedToPoint && targetBuilding == null)) break;

            yield return null;
        }

        if (snappedToPoint)
        {
            transform.position = originalPos;
            transform.rotation = originalRot;
            agent.enabled = true;
        }

        EndInteraction(originalStoppingDist);
    }

    private void EndInteraction(float originalStoppingDist)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.stoppingDistance = originalStoppingDist;
            agent.isStopped = false;
        }
        if (wanderScript != null) wanderScript.enabled = true;

        isInteracting = false;
        timer = 0f;
    }

    private Transform FindInteractionPoint(Transform parent)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name == "InteractionPoint") return child;
        }
        return null;
    }
}