using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class hp_logic : MonoBehaviour
{
    public TextMeshProUGUI hp_label;
    public Image image;
    public int MaxPoints;

    int hp = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hp_label.SetText(hp.ToString());    
    }

    // Update is called once per frame
    void Update()
    {
        hp = CalculateHP();
        hp_label.SetText(hp.ToString());
        image.fillAmount = (float)hp / (float)MaxPoints;
    }

    int CalculateHP()
    {
        int hp = 0;
        int buildingLayer = LayerMask.NameToLayer("Building");

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == buildingLayer)
            {
                Building b = obj.GetComponent<Building>();
                if (b != null)
                {
                    hp += b.Cost;
                }                  
            }
        }

        return hp;
    }
}
