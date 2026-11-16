#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu « Jeu > Sauvegarde » de l'éditeur Unity : réinitialiser la progression ou les réglages,
/// et ouvrir le dossier où les fichiers sont enregistrés.
/// À placer dans un dossier nommé Editor (ex. Assets/Editor/OutilsSauvegarde.cs).
/// Outil de développement : il n'existe pas dans le jeu final.
/// </summary>
public static class OutilsSauvegarde
{
    const string Menu = "Jeu/Sauvegarde/";

    static string Chemin(string fichier)
    {
        return Path.Combine(Application.persistentDataPath, fichier);
    }

    [MenuItem(Menu + "Réinitialiser la progression (cartes, défis, position)")]
    static void ReinitialiserProgression()
    {
        if (!EditorUtility.DisplayDialog("Réinitialiser la progression ?",
            "Toutes les cartes, tous les défis et la position du joueur seront effacés.\n" +
            "Les réglages d'accessibilité sont conservés.", "Oui, effacer", "Annuler"))
            return;

        if (Application.isPlaying && ProgressManager.Instance != null)
        {
            // Le jeu tourne : on passe par le gestionnaire. Supprimer le fichier ne suffirait pas,
            // car le jeu le réécrirait en quittant.
            ProgressManager.Instance.ResetProgress();
            Debug.Log("[Sauvegarde] Progression effacée. Relance la scène pour retrouver les cartes de départ.");
            return;
        }

        Supprimer("sauvegarde.json");
    }

    [MenuItem(Menu + "Réinitialiser les réglages (volumes, texte...)")]
    static void ReinitialiserReglages()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Mode Play actif",
                "Quitte le mode Play avant de réinitialiser les réglages.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Réinitialiser les réglages ?",
            "Les volumes, la taille du texte et les autres réglages reviendront à leurs valeurs par défaut.",
            "Oui, effacer", "Annuler"))
            return;

        Supprimer("reglages.json");
    }

    [MenuItem(Menu + "Tout réinitialiser (progression + réglages)")]
    static void ToutReinitialiser()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Mode Play actif",
                "Quitte le mode Play avant de tout réinitialiser.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Tout réinitialiser ?",
            "La progression ET les réglages seront effacés.", "Oui, tout effacer", "Annuler"))
            return;

        Supprimer("sauvegarde.json");
        Supprimer("reglages.json");
    }

    [MenuItem(Menu + "Ouvrir le dossier de sauvegarde")]
    static void OuvrirDossier()
    {
        string cible = File.Exists(Chemin("sauvegarde.json"))
            ? Chemin("sauvegarde.json")
            : Application.persistentDataPath;

        EditorUtility.RevealInFinder(cible);
        Debug.Log("[Sauvegarde] Dossier : " + Application.persistentDataPath);
    }

    static void Supprimer(string fichier)
    {
        string chemin = Chemin(fichier);
        bool supprime = false;

        // Le fichier temporaire est celui que ProgressManager écrit avant de le déplacer
        foreach (string f in new[] { chemin, chemin + ".tmp" })
        {
            if (File.Exists(f))
            {
                File.Delete(f);
                supprime = true;
            }
        }

        Debug.Log(supprime
            ? "[Sauvegarde] " + fichier + " supprimé : le prochain lancement repart de zéro."
            : "[Sauvegarde] " + fichier + " n'existe pas : rien à supprimer.");
    }
}
#endif
