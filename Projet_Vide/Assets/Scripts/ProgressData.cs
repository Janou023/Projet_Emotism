using System;
using System.Collections.Generic;

/// <summary>
/// Position du joueur sur la carte.
/// </summary>
[Serializable]
public class PlayerPosition
{
    public string carte = "";
    public float x;
    public float y;
}

/// <summary>
/// Progression du joueur, stockée dans sauvegarde.json.
/// Les noms des champs sont les clés du fichier : ils reprennent volontairement
/// ceux du fichier de l'équipe (emotions_terminees, defis_termines).
/// IMPORTANT : tout ce qui est dans le fichier doit exister ici, sinon
/// la sauvegarde effacerait les champs inconnus.
/// </summary>
[Serializable]
public class ProgressData
{
    public int version = 1;

    // Identifiants des cartes émotion débloquées (ex. "joie", "colere")
    public List<string> emotions_terminees = new List<string>();

    // Identifiants des défis terminés (ex. "tram", "boulangerie")
    public List<string> defis_termines = new List<string>();

    public PlayerPosition position = new PlayerPosition();

    /// <summary>Nettoie les listes (doublons, valeurs vides) après lecture du fichier.</summary>
    public void Sanitize()
    {
        emotions_terminees = Clean(emotions_terminees);
        defis_termines = Clean(defis_termines);
        if (position == null) position = new PlayerPosition();
    }

    static List<string> Clean(List<string> source)
    {
        var result = new List<string>();
        if (source == null) return result;

        var seen = new HashSet<string>();
        foreach (var s in source)
        {
            if (!string.IsNullOrWhiteSpace(s) && seen.Add(s))
                result.Add(s);
        }
        return result;
    }
}
