using System.Collections.Generic;
using UnityEngine;

public class ClasseurEmotions : MonoBehaviour
{
    [SerializeField] private List<CarteEmotion> cartesPossedees = new List<CarteEmotion>();

    public void AjouterCarte(CarteEmotion carte)
    {
        if (carte != null && !Possede(carte))
        {
            cartesPossedees.Add(carte);
        }
    }

    public bool Possede(CarteEmotion carte)
    {
        return cartesPossedees.Exists(c => c.id == carte.id);
    }

    public List<CarteEmotion> GetCartes()
    {
        return cartesPossedees;
    }
}