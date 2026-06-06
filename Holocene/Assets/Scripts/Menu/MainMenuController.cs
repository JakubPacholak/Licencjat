using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panele Menu")]
    public GameObject panelMainMenu;
    public GameObject panelLevels;
    public GameObject panelCollection;
    public GameObject panelCollectionDetails;
    public GameObject panelOptions;
    public GameObject panelKeybinds;

    [Header("Ustawienia Gry")]
    public string gameSceneName = "GameScene";

    [Header("Przyciski Opcji (Teksty)")]
    public TextMeshProUGUI musicButtonText;
    public TextMeshProUGUI soundButtonText;

    private bool isMusicOn = true;
    private bool isSoundOn = true;

    void Start()
    {
        ShowPanel(panelMainMenu);
        UpdateOptionsUI();
    }

    private void ShowPanel(GameObject panelToShow)
    {
        panelMainMenu.SetActive(false);
        panelLevels.SetActive(false);
        panelCollection.SetActive(false);
        panelCollectionDetails.SetActive(false);
        panelOptions.SetActive(false);
        panelKeybinds.SetActive(false);

        panelToShow.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Intro");
    }

    public void OpenMainMenu() => ShowPanel(panelMainMenu);
    public void OpenLevels() => ShowPanel(panelLevels);
    public void OpenCollection() => ShowPanel(panelCollection);
    public void OpenCollectionDetails() => ShowPanel(panelCollectionDetails);
    public void CloseCollectionDetails() => ShowPanel(panelCollection);
    public void OpenOptions() => ShowPanel(panelOptions);
    public void OpenKeybinds() => ShowPanel(panelKeybinds);

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        UpdateOptionsUI();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        UpdateOptionsUI();
    }

    private void UpdateOptionsUI()
    {
        if (musicButtonText != null)
            musicButtonText.text = isMusicOn ? "Music: ON" : "Music: OFF";

        if (soundButtonText != null)
            soundButtonText.text = isSoundOn ? "Sound: ON" : "Sound: OFF";
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}