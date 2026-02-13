using UnityEngine;

[CreateAssetMenu(menuName = "Data/MergeRecipe")]
public class MergeRecipe : ScriptableObject
{
    [Header("Skladniki fuzji")]
    public BuildingData InputA;
    public BuildingData InputB;

    [Header("Wynik")]
    public BuildingData Result;

    [Header("Ograniczenia")]
    [Tooltip("Ile razy mozna wykonac to polaczenie w trakcie gry? 0 = bez limitu.")] 
    public int MaxUses = 0;
}