using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;
    public static FadeManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return FadeOut();
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return FadeIn();
    }

    public IEnumerator FadeIn()
    {
        fadeCanvasGroup.alpha = 1;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = 1 - (elapsedTime / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
    }

    public IEnumerator FadeOut()
    {
        fadeCanvasGroup.alpha = 0;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = elapsedTime / fadeDuration;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
    }
}