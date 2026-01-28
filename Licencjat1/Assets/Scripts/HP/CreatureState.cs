
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureState : MonoBehaviour
{
    public GameObject emoticonThinking; // Reference to the thinking emoticon GameObject
    public GameObject emoticonHappy;    // Reference to the happy emoticon GameObject

    public TextMeshProUGUI hp_label;
    public Image image;
    public int HP_MaxPoints;
    int HP_CurrentPoints = 0;

    public enum State   // Enum to represent the creature's state
    {
        Thinking,
        Happy,
        Idle
    }

    public State currentState = State.Idle; // Initial state of the creature
    private float nextThinkingTime = 0f; // Time for the next thinking emoticon display
    private float thinkingCounter = 5f; // Counter to next thinking emoticon display

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emoticonHappy.SetActive(false);
        emoticonThinking.SetActive(false);

        hp_label.SetText(HP_CurrentPoints.ToString());
        nextThinkingTime = Random.Range(10f, 30f);  // Initial random time for the first thinking emoticon display
    }

    // Update is called once per frame
    void Update()
    {
        HP_CurrentPoints = CalculateHP();
        hp_label.SetText(HP_CurrentPoints.ToString());
        image.fillAmount = (float)HP_CurrentPoints / (float)HP_MaxPoints;
        
        if(currentState == State.Thinking) // If currently thinking, do not count down
            return;

        thinkingCounter -= Time.deltaTime;
        
        if (thinkingCounter <= 0f)
        {
            ShowThinkingEmoticon();
            thinkingCounter = Random.Range(10f, 30f); // Reset the counter to a new random value
        }
    }

    // Show thinking emoticon
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
        Invoke("HideHappyEmoticon", 4f); // Hide happy emoticon after 3 seconds
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
