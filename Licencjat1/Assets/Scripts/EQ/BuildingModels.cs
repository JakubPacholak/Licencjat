using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BuildingModels : MonoBehaviour
{
    [SerializeField] private Transform wrapper;

    public float Rotation => wrapper.eulerAngles.y;

    private BuildingShapeUnit[] shapeUnits;

    private void Awake()
    {
        shapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
    }
    public void AddRotation(float rotationStep)
    {
        wrapper.Rotate(new Vector3(0, rotationStep, 0));
    }

    public void SetRotation(float yRotation)
    {
        wrapper.eulerAngles = new Vector3(0, yRotation, 0);
    }

    public List<Vector3> GetAllBuldingPosition()
    {
        return shapeUnits.Select(unit => unit.transform.position).ToList();
    }
    public List<Vector3> GetRotatedShapeUnitOffsets()
    {
        if (shapeUnits == null)
        {
            shapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
        }

        return shapeUnits.Select(unit => unit.transform.position - transform.position).ToList();
    }
}