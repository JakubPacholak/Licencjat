using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Wymagane do dzia?ania Coroutine

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

    private bool isTransitioning = false;

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

        // Je?li punkty osi?gn? 100 i jeszcze nie zacz?li?my odliczania
        if (HP_CurrentPoints >= 100 && !isTransitioning)
        {
            StartCoroutine(DelayedNextLevel());
        }

        HandleEmoticons();
    }

    private IEnumerator DelayedNextLevel()
    {
        isTransitioning = true; // Blokujemy ponowne wywo?anie

        Debug.Log("Poziom uko?czony! Przej?cie za 7 sekund...");

        // Tutaj mo?esz doda? co? ekstra, np. dzi?kujemy graczowi albo odpalamy konfetti
        ShowHappyEmoticon();

        yield return new WaitForSeconds(7f); // Czekamy dok?adnie 7 sekund

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("To by? ostatni poziom w Build Settings!");
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