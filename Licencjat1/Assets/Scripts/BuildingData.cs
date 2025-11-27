// BuildingData.cs (z dodatkiem Icon, pe?ny skrypt)
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Building")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public BuildingModels Model { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }  // Nowy: Ikona dla ekwipunku
}