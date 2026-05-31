using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Building")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public BuildingModels Model { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public GameObject PlacementVFX { get; private set; }
    [field: SerializeField] public List<Material> ColorVariants { get; private set; }
    [field: SerializeField] public AudioClip PlacementSound { get; private set; }

    [field: SerializeField] public bool AllowStacking { get; private set; } = false;
    [field: SerializeField] public bool CanBeStackedOn { get; private set; } = false;
    [field: SerializeField] public bool OnlyStackOnSameType { get; private set; } = false;
    [field: SerializeField] public bool IgnoreCollision { get; private set; } = false;
}