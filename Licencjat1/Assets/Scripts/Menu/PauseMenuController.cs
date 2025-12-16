using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public Canvas pauseMenuCanvas;
    public static bool IsPaused { get; private set; } = false;


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
        if (pauseMenuCanvas.enabled)
        {
            Time.timeScale = 0f; // Pause the game
            IsPaused = true;
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            IsPaused = false;
        }
    }
}
