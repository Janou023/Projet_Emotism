using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public string sceneGame;
    public string sceneParametres;

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
        if(FadeManager.Instance != null)
        {
            FadeManager.Instance.LoadSceneWithFade(sceneParametres);

        } else
        {
            SceneManager.LoadScene(sceneParametres);
        }
    }

    public void Quitter()
    {
        Application.Quit();
        Debug.Log("Quitter le jeu.");
    }
}
