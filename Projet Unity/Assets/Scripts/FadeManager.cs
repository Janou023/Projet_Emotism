using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;
    public static FadeManager Instance;
    private bool transitionEnCours;

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
        if (transitionEnCours || string.IsNullOrWhiteSpace(sceneName)) return;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("FadeManager : scène absente des scènes de build : " + sceneName);
            return;
        }

        transitionEnCours = true;
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return FadeOut();
        AsyncOperation chargement = SceneManager.LoadSceneAsync(sceneName);
        if (chargement == null)
        {
            transitionEnCours = false;
            yield break;
        }

        yield return chargement;
        yield return FadeIn();
        transitionEnCours = false;
    }

    public IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.alpha = 1;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = 1 - (elapsedTime / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = true;
    }

    public IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = true;
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
