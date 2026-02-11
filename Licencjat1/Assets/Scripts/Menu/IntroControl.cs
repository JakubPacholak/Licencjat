using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("Konfiguracja")]
    [Tooltip("Lista ilustracji do wy?wietlenia w kolejno?ci")]
    public Sprite[] introImages;
    [Tooltip("Nazwa sceny z w?a?ciw? gr?, która ma si? w??czy? po intro")]
    public string gameSceneName = "GameLevel";

    [Header("Elementy UI")]
    public Image displayImage;
    public CanvasGroup fader;

    [Header("Ustawienia Czasu")]
    public float timePerImage = 10f;
    public float fadeDuration = 1.5f;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        for (int i = 0; i < introImages.Length; i++)
        {
            displayImage.sprite = introImages[i];
            yield return StartCoroutine(FadeRoutine(1f, 0f));
            yield return new WaitForSeconds(timePerImage);
            yield return StartCoroutine(FadeRoutine(0f, 1f));
        }
        FinishIntro();
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            fader.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        fader.alpha = endAlpha;
    }

    private void FinishIntro()
    {
        PlayerPrefs.SetInt("IntroPlayed", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
}