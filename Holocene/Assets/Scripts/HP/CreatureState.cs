using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreatureState : MonoBehaviour
{
    [Header("Emoticons")]
    public GameObject emoticonThinking;
    public GameObject emoticonHappy;

    [Header("Happiness Bar UI")]
    public TextMeshProUGUI hp_label;
    public Image image;
    public int HP_MaxPoints;
    int HP_CurrentPoints = 0;

    [Header("Level Completion UI")]
    public GameObject levelChoiceWindow;
    public GameObject reopenWindowButton;
    public Button btnYes;
    public Button btnNo;
    public Button btnReopen;

    [Header("Progression Settings")]
    [Tooltip("Zaznacz to TYLKO na ostatnim poziomie, aby w??czy? Outro")]
    public bool isFinalLevel = false;
    public string outroSceneName = "Outro";

    private bool isTransitioning = false;
    private bool levelFinished = false;

    public enum State
    {
        Thinking,
        Happy,
        Idle
    }

    public State currentState = State.Idle;
    private float thinkingCounter = 5f;

    void Start()
    {
        emoticonHappy.SetActive(false);
        emoticonThinking.SetActive(false);

        if (levelChoiceWindow != null) levelChoiceWindow.SetActive(false);
        if (reopenWindowButton != null) reopenWindowButton.SetActive(false);

        if (btnYes != null) btnYes.onClick.AddListener(GoToNextLevel);
        if (btnNo != null) btnNo.onClick.AddListener(CloseChoiceWindow);
        if (btnReopen != null) btnReopen.onClick.AddListener(OpenChoiceWindow);

        if (hp_label != null) hp_label.SetText(HP_CurrentPoints.ToString());
    }

    void Update()
    {
        HP_CurrentPoints = CalculateHP();
        if (hp_label != null) hp_label.SetText(HP_CurrentPoints.ToString());

        if (HP_MaxPoints > 0 && image != null)
        {
            image.fillAmount = (float)HP_CurrentPoints / (float)HP_MaxPoints;
        }

        if (HP_CurrentPoints >= 100 && !isTransitioning && !levelFinished)
        {
            StartCoroutine(DelayedLevelCompletion());
        }

        HandleEmoticons();
    }

    private IEnumerator DelayedLevelCompletion()
    {
        isTransitioning = true;
        levelFinished = true;

        ShowHappyEmoticon();

        // Czekamy 7 sekund na nacieszenie si? widokiem
        yield return new WaitForSeconds(7f);

        // Sprawdzamy, czy to ostatni poziom
        if (isFinalLevel)
        {
            // Je?li tak, odpalamy Outro
            SceneManager.LoadScene(outroSceneName);
        }
        else
        {
            // Je?li nie, otwieramy standardowe okienko przej?cia do nast?pnego poziomu
            OpenChoiceWindow();
        }
    }

    public void OpenChoiceWindow()
    {
        if (levelChoiceWindow != null) levelChoiceWindow.SetActive(true);
        if (reopenWindowButton != null) reopenWindowButton.SetActive(false);
    }

    public void CloseChoiceWindow()
    {
        if (levelChoiceWindow != null) levelChoiceWindow.SetActive(false);
        if (reopenWindowButton != null) reopenWindowButton.SetActive(true);
    }

    public void GoToNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    private void HandleEmoticons()
    {
        if (currentState == State.Thinking) return;

        thinkingCounter -= Time.deltaTime;
        if (thinkingCounter <= 0f)
        {
            ShowThinkingEmoticon();
            thinkingCounter = Random.Range(10f, 30f);
        }
    }

    void ShowThinkingEmoticon()
    {
        if (emoticonThinking != null) emoticonThinking.SetActive(true);
        currentState = State.Thinking;
    }

    public void ShowHappyEmoticon()
    {
        if (emoticonThinking != null) emoticonThinking.SetActive(false);
        if (emoticonHappy != null) emoticonHappy.SetActive(true);
        currentState = State.Happy;
        Invoke("HideHappyEmoticon", 4f);
    }

    private void HideHappyEmoticon()
    {
        if (emoticonHappy != null) emoticonHappy.SetActive(false);
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
                if (b != null) hp += b.Cost;
            }
        }
        return hp;
    }
}