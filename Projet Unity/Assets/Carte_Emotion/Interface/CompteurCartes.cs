using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompteurCartes : MonoBehaviour
{
    public ClasseurEmotions classeur;
    public TextMeshProUGUI texte;
    public Image remplissage;
    public int total = 5;

    void OnEnable() { Rafraichir(); }

    public void Rafraichir()
    {
        int n = classeur != null ? classeur.GetCartes().Count : 0;
        texte.text = n + " / " + total + " emotions decouvertes";
        if (remplissage != null) remplissage.fillAmount = (float)n / total;
    }
}