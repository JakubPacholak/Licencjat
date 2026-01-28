using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    public enum BuildingPreviewState
    {
        POSITIVE,
        NEGATIVE
    }

    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

    public BuildingPreviewState State { get; private set; } = BuildingPreviewState.NEGATIVE;
    public BuildingData Data { get; private set; }
    public BuildingModels BuildingModels { get; private set; }

    private readonly List<Renderer> renderers = new();
    private readonly List<Collider> colliders = new();

    public void Setup(BuildingData data)
    {
        Data = data;
        BuildingModels = Instantiate(data.Model, transform.position, Quaternion.identity, transform);

        renderers.AddRange(BuildingModels.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BuildingModels.GetComponentsInChildren<Collider>());

        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        SetPreviewMaterial(State);
    }

    public void ChangeState(BuildingPreviewState newState)
    {
        if (newState == State)
            return;

        State = newState;
        SetPreviewMaterial(State);
    }
    public void AddRotation(float rotationStep)
    {
        BuildingModels.AddRotation(rotationStep);
    }

    public void SetRotation(float yRotation)
    {
        BuildingModels.SetRotation(yRotation);
    }

    private void SetPreviewMaterial(BuildingPreviewState newState)
    {
        Material previewMat = newState == BuildingPreviewState.POSITIVE ? positiveMaterial : negativeMaterial;

        foreach (var rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = previewMat;
            }
            rend.sharedMaterials = mats;
        }
    }
}