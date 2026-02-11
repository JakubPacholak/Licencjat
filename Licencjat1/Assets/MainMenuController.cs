using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Ustawienia Scen")]
    [Tooltip("Wpisz tu nazw? sceny z Twoim intrem, np. IntroScene")]
    public string introSceneName = "IntroScene";

    [Tooltip("Wpisz tu nazw? sceny z gr?, do tej pory by?o to SampleScene")]
    public string gameSceneName = "SampleScene";

    public void StartGame()
    {
        if (PlayerPrefs.HasKey("IntroPlayed"))
        {
            Debug.Log("Intro by?o ju? odtworzone. ?aduj? gr?...");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.Log("Pierwsze uruchomienie. ?aduj? intro...");
            SceneManager.LoadScene(introSceneName);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}