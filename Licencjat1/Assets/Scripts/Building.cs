using UnityEngine;

public class Building : MonoBehaviour
{
    public string Description => data.Description;
    public int Cost => data.Cost;

    public BuildingData Data => data;
    public float Rotation => model.Rotation;

    private BuildingModels model;
    private BuildingData data;

    public void Setup(BuildingData data, float rotation)
    {
        this.data = data;
        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.SetRotation(rotation);
    }
}