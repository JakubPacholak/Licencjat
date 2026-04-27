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
    public Material ChosenVariant { get; private set; }

    private readonly List<Renderer> renderers = new();
    private readonly List<Collider> colliders = new();

    public void Setup(BuildingData data, Material variant = null)
    {
        Data = data;
        ChosenVariant = variant;

        BuildingModels = Instantiate(data.Model, transform);
        BuildingModels.transform.localPosition = Vector3.zero;
        BuildingModels.transform.localRotation = Quaternion.identity;

        renderers.AddRange(BuildingModels.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BuildingModels.GetComponentsInChildren<Collider>());

        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Animator[] animators = BuildingModels.GetComponentsInChildren<Animator>();
        foreach (var anim in animators)
        {
            anim.enabled = false;
        }

        Animation[] legacyAnimations = BuildingModels.GetComponentsInChildren<Animation>();
        foreach (var anim in legacyAnimations)
        {
            anim.enabled = false;
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