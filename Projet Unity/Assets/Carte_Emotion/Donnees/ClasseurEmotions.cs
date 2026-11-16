using System.Collections.Generic;
using UnityEngine;

public class ClasseurEmotions : MonoBehaviour
{
    private const string CleCartesPossedees = "SauvegardeJoueur_CartesPossedees";

    [System.Serializable]
    private class DonneesCartes
    {
        public List<string> ids = new List<string>();
    }

    [SerializeField] private List<CarteEmotion> cartesPossedees = new List<CarteEmotion>();

    private void Awake()
    {
        ChargerProgression();
    }

    public void AjouterCarte(CarteEmotion carte)
    {
        if (carte != null && !Possede(carte))
        {
            cartesPossedees.Add(carte);
            SauvegarderProgression();
        }
    }

    public bool Possede(CarteEmotion carte)
    {
        return carte != null && cartesPossedees.Exists(c => c != null && c.id == carte.id);
    }

    public List<CarteEmotion> GetCartes()
    {
        return cartesPossedees;
    }

    public static void SauvegarderProgression()
    {
        ClasseurEmotions[] classeurs = FindObjectsByType<ClasseurEmotions>(FindObjectsInactive.Include);
        if (classeurs.Length == 0)
        {
            return;
        }

        DonneesCartes donnees = new DonneesCartes();
        HashSet<string> ids = new HashSet<string>();
        foreach (ClasseurEmotions classeur in classeurs)
        {
            foreach (CarteEmotion carte in classeur.cartesPossedees)
            {
                if (carte != null && !string.IsNullOrWhiteSpace(carte.id) && ids.Add(carte.id))
                {
                    donnees.ids.Add(carte.id);
                }
            }
        }

        PlayerPrefs.SetString(CleCartesPossedees, JsonUtility.ToJson(donnees));
        PlayerPrefs.Save();
    }

    private void ChargerProgression()
    {
        if (!PlayerPrefs.HasKey(CleCartesPossedees))
        {
            return;
        }

        DonneesCartes donnees = JsonUtility.FromJson<DonneesCartes>(PlayerPrefs.GetString(CleCartesPossedees));
        if (donnees == null || donnees.ids == null)
        {
            Debug.LogWarning("La sauvegarde des cartes est invalide et ne peut pas etre chargee.", this);
            return;
        }

        foreach (string id in donnees.ids)
        {
            if (!string.IsNullOrWhiteSpace(id) && !cartesPossedees.Exists(c => c != null && c.id == id))
            {
                cartesPossedees.Add(new CarteEmotion { id = id });
            }
        }
    }
}