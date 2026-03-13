
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureState : MonoBehaviour
{
    public GameObject emoticonThinking;
    public GameObject emoticonHappy;

    public TextMeshProUGUI hp_label;
    public Image image;
    public int HP_MaxPoints;
    int HP_CurrentPoints = 0;

    public enum State
    {
        Thinking,
        Happy,
        Idle
    }

    public State currentState = State.Idle;
    private float nextThinkingTime = 0f;
    private float thinkingCounter = 5f;

    void Start()
    {
        emoticonHappy.SetActive(false);
        emoticonThinking.SetActive(false);

        hp_label.SetText(HP_CurrentPoints.ToString());
        nextThinkingTime = Random.Range(10f, 30f);
    }

    void Update()
    {
        HP_CurrentPoints = CalculateHP();
        hp_label.SetText(HP_CurrentPoints.ToString());
        image.fillAmount = (float)HP_CurrentPoints / (float)HP_MaxPoints;
        
        if(currentState == State.Thinking)
            return;

        thinkingCounter -= Time.deltaTime;
        
        if (thinkingCounter <= 0f)
        {
            ShowThinkingEmoticon();
            thinkingCounter = Random.Range(10f, 30f);
        }
    }

    void ShowThinkingEmoticon()
    {
        emoticonThinking.SetActive(true);
        currentState = State.Thinking;
    }

    public void ShowHappyEmoticon()
    {
        emoticonThinking.SetActive(false);
        emoticonHappy.SetActive(true);
        currentState = State.Happy;
        Invoke("HideHappyEmoticon", 4f);
    }

    private void HideHappyEmoticon()
    {
        emoticonHappy.SetActive(false);
        currentState = State.Idle;
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
