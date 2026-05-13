using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referencje Canvasów")]
    public Canvas pauseMenuCanvas;
    public Canvas eqCanvas;
    public TextMeshProUGUI triviaText;

    [Header("Panele Menu Pauzy")]
    public GameObject panelPauseMain;
    public GameObject panelLevels;
    public GameObject panelCollection;
    public GameObject panelOptions;
    public GameObject panelKeybinds;

    [Header("Ustawienia Menu")]
    [Tooltip("Wpisz tutaj nazwe sceny Twojego menu g?ównego")]
    public string mainMenuSceneName = "MainMenu";

    [TextArea] public string[] facts = new string[5];

    public static bool IsPaused { get; private set; } = false;

    void Start()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.enabled = false;
        }
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

            ShowPanel(panelPauseMain);

            if (facts != null && facts.Length > 0 && triviaText != null)
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

    private void ShowPanel(GameObject panelToShow)
    {
        if (panelPauseMain != null) panelPauseMain.SetActive(false);
        if (panelLevels != null) panelLevels.SetActive(false);
        if (panelCollection != null) panelCollection.SetActive(false);
        if (panelOptions != null) panelOptions.SetActive(false);
        if (panelKeybinds != null) panelKeybinds.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }

    public void OpenPauseMain() => ShowPanel(panelPauseMain);
    public void OpenLevels() => ShowPanel(panelLevels);
    public void OpenCollections() => ShowPanel(panelCollection);
    public void OpenOptions() => ShowPanel(panelOptions);
    public void OpenKeybinds() => ShowPanel(panelKeybinds);

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}