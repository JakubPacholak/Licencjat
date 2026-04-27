using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    public static List<Building> ActiveBuildings = new List<Building>();

    public string Description => data.Description;
    public int Cost => data.Cost;

    public BuildingData Data => data;
    public float Rotation => model.Rotation;
    public Material CurrentVariant { get; private set; }

    private BuildingModels model;
    private BuildingData data;

    private void OnEnable()
    {
        ActiveBuildings.Add(this);
    }

    private void OnDisable()
    {
        ActiveBuildings.Remove(this);
    }

    public void Setup(BuildingData data, float rotation, Material variant = null)
    {
        this.data = data;
        CurrentVariant = variant;

        model = Instantiate(data.Model, transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        model.SetRotation(rotation);

        if (variant != null)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                Material[] mats = new Material[rend.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = variant;
                }
                rend.sharedMaterials = mats;
            }
        }
    }
}