using TMPro;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public Canvas pauseMenuCanvas;
    public Canvas eqCanvas;
    public TextMeshProUGUI triviaText;
    public static bool IsPaused { get; private set; } = false;
    [TextArea] public string[] facts = new string[5];


    void Start()
    {
        pauseMenuCanvas.enabled = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        pauseMenuCanvas.enabled = !pauseMenuCanvas.enabled;
        eqCanvas.enabled = !eqCanvas.enabled;
        if (pauseMenuCanvas.enabled)
        {
            Time.timeScale = 0f; // Pause the game
            IsPaused = true;
            if (facts.Length > 0)
            {
                // Losowanie indeksu od 0 do d³ugoœci tablicy
                int randomIndex = Random.Range(0, facts.Length);
                triviaText.text = facts[randomIndex];
            }
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            IsPaused = false;
        }
    }
}
