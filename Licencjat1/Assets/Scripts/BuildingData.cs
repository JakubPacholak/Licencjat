using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Building")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public BuildingModels Model { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    [field: SerializeField]
    [Tooltip("Czy ten budynek mo?e by? stawiany na innych budynkach (stackowanie)")]
    public bool AllowStacking { get; private set; } = false;

    [field: SerializeField]
    [Tooltip("Czy inne budynki mog? sta? na tym budynku")]
    public bool CanBeStackedOn { get; private set; } = false;

    [CreateAssetMenu(menuName = "Data/MergeRecipe")]
    public class MergeRecipe : ScriptableObject
    {
        public List<BuildingData> Ingredients;
        public BuildingData Result;
    }
}