using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Donne une carte émotion au joueur : l'ajoute au classeur (donc à la sauvegarde),
/// puis affiche la fenêtre « Nouvelle carte obtenue ! » si la carte est nouvelle.
/// Utilisé par le système de défis quand une étape du JSON contient "carte_obtenue".
/// Chaque étape écrit un message [Recompense] dans la Console pour faciliter le diagnostic.
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
        Debug.Log("[Recompense] 1/4 Demande de la carte « " + idCarte + " ».");

        // --- La carte existe-t-elle dans le classeur ? (problème de nom) ---
        CarteEmotion carte = classeur != null ? Trouver(classeur.toutesLesCartes, idCarte) : null;
        if (carte == null)
        {
            if (classeur == null)
            {
                Debug.LogWarning("[Recompense] ÉCHEC : aucun UIClasseur n'est relié au DefiController.");
            }
            else
            {
                var ids = new List<string>();
                foreach (var c in classeur.toutesLesCartes) ids.Add(c != null ? c.id : "(vide)");
                Debug.LogWarning("[Recompense] ÉCHEC : carte « " + idCarte + " » introuvable. " +
                                 "Identifiants des cartes du classeur : " + string.Join(", ", ids));
            }
            quandFini?.Invoke();
            return false;
        }

        // --- La carte est-elle nouvelle ? ---
        bool nouvelle;
        if (classeur.classeurData != null)
            nouvelle = classeur.classeurData.AjouterCarte(carte);
        else
            nouvelle = ProgressManager.Instance != null && ProgressManager.Instance.UnlockEmotion(carte.id);

        classeur.RafraichirUI();

        if (!nouvelle)
        {
            Debug.Log("[Recompense] 2/4 La carte « " + carte.id + " » était DÉJÀ débloquée : pas de fenêtre. " +
                      "Soit la sauvegarde la contenait, soit un autre script l'a débloquée avant.");
            quandFini?.Invoke();
            return false;
        }
        Debug.Log("[Recompense] 2/4 Carte « " + carte.id + " » débloquée (nouvelle).");
        if (SoundsManager.Instance != null)
            SoundsManager.Instance.PlayNewCardSound();

        // --- La fenêtre existe-t-elle ? On la cherche elle-même si elle n'est pas reliée ---
        if (popup == null)
            popup = UnityEngine.Object.FindAnyObjectByType<CardRewardPopup>(FindObjectsInactive.Include);

        if (popup == null)
        {
            Debug.LogWarning("[Recompense] ÉCHEC : la carte « " + carte.id + " » est débloquée mais AUCUN " +
                             "CardRewardPopup (PopupCarte) n'existe dans la scène : pas de fenêtre.");
            quandFini?.Invoke();
            return false;
        }

        Debug.Log("[Recompense] 3/4 Fenêtre trouvée : « " + popup.name + " ».");

        Action fermeture = null;
        fermeture = () =>
        {
            popup.Closed -= fermeture;
            Debug.Log("[Recompense] 4/4 Fenêtre fermée par le joueur.");
            quandFini?.Invoke();
        };
        popup.Closed += fermeture;
        popup.Show(carte);
        return true;
    }
}
