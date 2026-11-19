using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Compteur « n / total emotions decouvertes » du carnet.
/// Le nombre vient de la sauvegarde (ClasseurEmotions.NombreCartes) et se met à jour
/// à l'ouverture du carnet ET à chaque fois que la progression change.
/// </summary>
public class CompteurCartes : MonoBehaviour
{
    public ClasseurEmotions classeur;
    public TextMeshProUGUI texte;
    public Image remplissage;
    public int total = 5;

    void OnEnable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged += Rafraichir;
        Rafraichir();
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= Rafraichir;
    }

    public void Rafraichir()
    {
        int n = classeur != null ? classeur.NombreCartes : 0;
        texte.text = n + " / " + total + " emotions decouvertes";
        if (remplissage != null && total > 0)
            remplissage.fillAmount = Mathf.Clamp01((float)n / total);
    }
}
