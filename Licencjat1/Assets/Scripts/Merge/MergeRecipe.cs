using UnityEngine;

[CreateAssetMenu(menuName = "Data/MergeRecipe")]
public class MergeRecipe : ScriptableObject
{
    [Header("Sk?adniki fuzji")]
    public BuildingData InputA;
    public BuildingData InputB;

    [Header("Wynik")]
    [Tooltip("Jaki budynek powstanie po po??czeniu?")]
    public BuildingData Result;
}