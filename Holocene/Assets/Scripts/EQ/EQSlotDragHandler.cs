using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class EQSlotDragHandler : MonoBehaviour, IPointerClickHandler
{
    private BuildingData data;
    private BuildingEQ parent;

    public AudioClip clickSound;
    public AudioMixerGroup soundMixerGroup; 

    public void Setup(BuildingData buildingData, BuildingEQ parentScript)
    {
        data = buildingData;
        parent = parentScript;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Level2QuestManager lvl2Manager = FindObjectOfType<Level2QuestManager>();
        if (lvl2Manager != null && lvl2Manager.IsSatelliteBlocked)
        {
            if (transform.GetSiblingIndex() == 3) return;
        }

        if (clickSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, 0.8f);
        }

        if (parent != null)
        {
            parent.SelectBuilding(data);
        }
    }
}