using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("Konfiguracja")]
    public Sprite[] introImages;
    public string gameSceneName = "GameLevel";

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
        PlayerPrefs.DeleteKey("IntroPlayed");

        if (fader != null)
        {
            fader.alpha = 1f;
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipIntro);
        }

        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        for (int i = 0; i < introImages.Length; i++)
        {
            displayImage.sprite = introImages[i];

            yield return new WaitForSeconds(blackScreenDuration);

            yield return StartCoroutine(FadeRoutine(1f, 0f));

            yield return new WaitForSeconds(timePerImage);

            yield return StartCoroutine(FadeRoutine(0f, 1f));
        }

        FinishIntro();
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

    public void SkipIntro()
    {
        StopAllCoroutines();

        FinishIntro();
    }

    private void FinishIntro()
    {
        SceneManager.LoadScene("LvL 1");
    }
}