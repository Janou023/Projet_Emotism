using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public string sceneGame;
    public string sceneParametres;
    public GameObject menuPrincipalUI;

    readonly List<EventSystem> eventSystemsMenu = new List<EventSystem>();
    bool transitionParametresEnCours;

    public void Jouer()
    {
        if(FadeManager.Instance != null)
        {
            FadeManager.Instance.LoadSceneWithFade(sceneGame);
        } else
        {
            SceneManager.LoadScene(sceneGame);
        }
    }

    public void OuvrirParametres()
    {
        if (transitionParametresEnCours || SceneManager.GetSceneByName(sceneParametres).isLoaded)
            return;
        transitionParametresEnCours = true;
        StartCoroutine(OuvrirParametresRoutine());
    }

    IEnumerator OuvrirParametresRoutine()
    {
        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeOut();

        if (menuPrincipalUI != null)
            menuPrincipalUI.SetActive(false);

        eventSystemsMenu.Clear();
        foreach (EventSystem eventSystem in FindObjectsByType<EventSystem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (eventSystem.gameObject.scene != gameObject.scene) continue;
            eventSystem.enabled = false;
            eventSystemsMenu.Add(eventSystem);
        }

        AsyncOperation chargement = SceneManager.LoadSceneAsync(
            sceneParametres, LoadSceneMode.Additive);
        if (chargement == null)
        {
            RestaurerMenuPrincipal();
            transitionParametresEnCours = false;
            if (FadeManager.Instance != null)
                yield return FadeManager.Instance.FadeIn();
            yield break;
        }

        yield return chargement;
        Scene sceneReglages = SceneManager.GetSceneByName(sceneParametres);
        if (sceneReglages.IsValid())
            SceneManager.SetActiveScene(sceneReglages);

        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeIn();
        transitionParametresEnCours = false;
    }

    public void FermerParametres(Scene sceneReglages)
    {
        if (transitionParametresEnCours) return;
        transitionParametresEnCours = true;
        StartCoroutine(FermerParametresRoutine(sceneReglages));
    }

    IEnumerator FermerParametresRoutine(Scene sceneReglages)
    {
        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeOut();

        SceneManager.SetActiveScene(gameObject.scene);
        AsyncOperation dechargement = SceneManager.UnloadSceneAsync(sceneReglages);
        if (dechargement != null)
            yield return dechargement;

        RestaurerMenuPrincipal();
        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeIn();
        transitionParametresEnCours = false;
    }

    void RestaurerMenuPrincipal()
    {
        if (menuPrincipalUI != null)
            menuPrincipalUI.SetActive(true);

        foreach (EventSystem eventSystem in eventSystemsMenu)
            if (eventSystem != null) eventSystem.enabled = true;
        eventSystemsMenu.Clear();
    }

    public void Quitter()
    {
        Application.Quit();
        Debug.Log("Quitter le jeu.");
    }
}
