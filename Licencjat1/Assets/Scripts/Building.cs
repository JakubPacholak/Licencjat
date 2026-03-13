using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    public static List<Building> ActiveBuildings = new List<Building>();

    public string Description => data.Description;
    public int Cost => data.Cost;

    public BuildingData Data => data;
    public float Rotation => model.Rotation;

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

    public void Setup(BuildingData data, float rotation)
    {
        this.data = data;
        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.SetRotation(rotation);
    }
}