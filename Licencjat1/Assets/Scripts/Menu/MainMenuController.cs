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
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
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

    public void OpenLevels()
    {
        Debug.Log("Otwieram panel poziomów...");
    }

    public void OpenCollections()
    {
        Debug.Log("Otwieram panel kolekcji...");
    }

    public void OpenOptions()
    {
        Debug.Log("Otwieram panel opcji...");
    }
}