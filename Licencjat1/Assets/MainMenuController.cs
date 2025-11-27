using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BuildScene");
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
