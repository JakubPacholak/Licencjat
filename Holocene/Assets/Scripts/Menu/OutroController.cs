using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OutroController : MonoBehaviour
{
    [Header("Konfiguracja")]
    public Sprite[] outroImages;
    public string mainMenuSceneName = "MainMenu";

    [Header("Elementy UI")]
    public Image displayImage;
    public CanvasGroup fader;
    public Button skipButton;

    [Header("Ustawienia Czasu")]
    public float timePerImage = 3f;
    public float fadeDuration = 1.5f;
    public float blackScreenDuration = 0.5f;

    private void Start()
    {

        if (fader != null)
        {
            fader.alpha = 1f;
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipOutro);
        }

        StartCoroutine(PlayOutroSequence());
    }

    private IEnumerator PlayOutroSequence()
    {
        for (int i = 0; i < outroImages.Length; i++)
        {
            displayImage.sprite = outroImages[i];

            yield return new WaitForSeconds(blackScreenDuration);

            yield return StartCoroutine(FadeRoutine(1f, 0f));

            yield return new WaitForSeconds(timePerImage);

            yield return StartCoroutine(FadeRoutine(0f, 1f));
        }

        FinishOutro();
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timeElapsed = 0f;
        fader.alpha = startAlpha;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            fader.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        fader.alpha = endAlpha;
    }

    public void SkipOutro()
    {
        StopAllCoroutines();
        FinishOutro();
    }

    private void FinishOutro()
    {
        SceneManager.LoadScene("MainMenu");
    }
}