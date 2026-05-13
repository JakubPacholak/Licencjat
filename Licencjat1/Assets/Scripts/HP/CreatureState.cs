using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreatureState : MonoBehaviour
{
    public GameObject emoticonThinking;
    public GameObject emoticonHappy;

    public TextMeshProUGUI hp_label;
    public Image image;
    public int HP_MaxPoints;
    int HP_CurrentPoints = 0;

    [Header("Level Up UI")]
    public GameObject levelUpPanel;
    private bool levelUpPromptShown = false;

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

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        hp_label.SetText(HP_CurrentPoints.ToString());
        nextThinkingTime = Random.Range(10f, 30f);
    }

    void Update()
    {
        HP_CurrentPoints = CalculateHP();
        hp_label.SetText(HP_CurrentPoints.ToString());

        if (HP_MaxPoints > 0)
        {
            image.fillAmount = (float)HP_CurrentPoints / (float)HP_MaxPoints;
        }

        if (HP_CurrentPoints >= 100 && !levelUpPromptShown)
        {
            ShowLevelUpPrompt();
        }
        if (levelUpPromptShown && Input.GetKeyDown(KeyCode.Y))
        {
            if (levelUpPanel != null && levelUpPanel.activeSelf)
            {
                StayOnCurrentLevel();
            }
            else
            {
                ShowLevelUpPrompt();
            }
        }

        if (currentState == State.Thinking)
            return;

        thinkingCounter -= Time.deltaTime;

        if (thinkingCounter <= 0f)
        {
            ShowThinkingEmoticon();
            thinkingCounter = Random.Range(10f, 30f);
        }
    }

    public void ShowLevelUpPrompt()
    {
        levelUpPromptShown = true;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void GoToNextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void StayOnCurrentLevel()
    {
        Time.timeScale = 1f;
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
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