using UnityEngine;

[CreateAssetMenu(menuName = "Data/MergeRecipe")]
public class MergeRecipe : ScriptableObject
{
    [Header("Sk?adniki fuzji")]
    public BuildingData InputA;
    public BuildingData InputB;

    [Header("Wynik")]
    public BuildingData Result;

    [Header("Ograniczenia")]
    [Tooltip("Ile razy mo?na wykona? to po??czenie w trakcie gry? 0 = bez limitu.")]
    public int MaxUses = 0;
}