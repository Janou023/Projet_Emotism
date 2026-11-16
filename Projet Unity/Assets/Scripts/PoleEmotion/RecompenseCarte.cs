using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Donne une carte émotion au joueur : l'ajoute au classeur (donc à la sauvegarde),
/// puis affiche la fenêtre « Nouvelle carte obtenue ! » si la carte est nouvelle.
/// Utilisé par le système de défis quand une étape du JSON contient "carte_obtenue".
/// </summary>
public static class RecompenseCarte
{
    /// <summary>
    /// Cherche une carte dans la liste complète du jeu.
    /// "colere", "Colère" et "COLERE" désignent la même carte.
    /// </summary>
    public static CarteEmotion Trouver(IEnumerable<CarteEmotion> catalogue, string idOuEmotion)
    {
        if (catalogue == null) return null;

        string cle = ProgressData.Normalize(idOuEmotion);
        if (cle == "") return null;

        foreach (var carte in catalogue)
        {
            if (carte != null && ProgressData.Normalize(carte.id) == cle)
                return carte;
        }

        // Repli : correspondance avec l'énumération Emotion (Joie, Colere, Peur...)
        if (EtapeDefi.TryParseEmotion(idOuEmotion, out Emotion emotion))
        {
            foreach (var carte in catalogue)
            {
                if (carte != null && carte.emotion == emotion)
                    return carte;
            }
        }

        return null;
    }

    /// <summary>
    /// Donne la carte au joueur.
    /// quandFini est TOUJOURS appelé une fois : tout de suite si la carte est introuvable ou
    /// déjà possédée, ou à la fermeture de la fenêtre si elle est nouvelle.
    /// </summary>
    /// <returns>true si la fenêtre d'obtention a été affichée.</returns>
    public static bool Accorder(UIClasseur classeur, CardRewardPopup popup, string idCarte, Action quandFini)
    {
        CarteEmotion carte = classeur != null ? Trouver(classeur.toutesLesCartes, idCarte) : null;
        if (carte == null)
        {
            Debug.LogWarning("[Recompense] Carte introuvable : « " + idCarte +
                             " ». Vérifier la liste toutesLesCartes du classeur.");
            quandFini?.Invoke();
            return false;
        }

        bool nouvelle;
        if (classeur.classeurData != null)
            nouvelle = classeur.classeurData.AjouterCarte(carte);
        else
            nouvelle = ProgressManager.Instance != null && ProgressManager.Instance.UnlockEmotion(carte.id);

        classeur.RafraichirUI();

        if (!nouvelle || popup == null)
        {
            quandFini?.Invoke();
            return false;
        }

        Action fermeture = null;
        fermeture = () =>
        {
            popup.Closed -= fermeture;
            quandFini?.Invoke();
        };
        popup.Closed += fermeture;
        popup.Show(carte);
        return true;
    }
}
