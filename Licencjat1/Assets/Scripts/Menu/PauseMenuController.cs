using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referencje")]
    public Canvas pauseMenuCanvas;
    public Canvas eqCanvas;
    public TextMeshProUGUI triviaText;

    [Header("Ustawienia Menu")]
    [Tooltip("Wpisz tutaj nazw? sceny Twojego menu g?ównego")]
    public string mainMenuSceneName = "MainMenu";

    [TextArea] public string[] facts = new string[5];

    public static bool IsPaused { get; private set; } = false;

    void Start()
    {
        pauseMenuCanvas.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenuCanvas.enabled = !pauseMenuCanvas.enabled;

        if (eqCanvas != null)
            eqCanvas.enabled = !eqCanvas.enabled;

        if (pauseMenuCanvas.enabled)
        {
            Time.timeScale = 0f;
            IsPaused = true;
            if (facts.Length > 0)
            {
                int randomIndex = Random.Range(0, facts.Length);
                triviaText.text = facts[randomIndex];
            }
        }
        else
        {
            Time.timeScale = 1f;
            IsPaused = false;
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OpenLevels()
    {
        Debug.Log("Otwieram panel poziomów w pauzie...");
    }

    public void OpenCollections()
    {
        Debug.Log("Otwieram panel kolekcji w pauzie...");
    }

    public void OpenOptions()
    {
        Debug.Log("Otwieram panel opcji w pauzie...");
    }
}