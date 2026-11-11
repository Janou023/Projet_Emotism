using System;
using System.Collections.Generic;
using UnityEngine;

// --- CLASSES CONFORMES AU DIAGRAMME UML ---

public enum PortraitSide { Left, Right }

[Serializable]
public class Personnage
{
    public string nom;                               // doit correspondre au champ "personnage" du JSON
    public PortraitSide cote = PortraitSide.Right;   // côté de l'écran où il apparaît
    public Sprite portraitNeutre;

    [Tooltip("Dans l'ordre de l'enum Emotion : Joie, Colere, Peur, Tristesse, Stresse. Une case vide = portrait neutre.")]
    public List<Sprite> portraits = new List<Sprite>();

    public Sprite GetPortrait(Emotion emotion, bool utiliserExpression)
    {
        if (utiliserExpression)
        {
            int i = (int)emotion;
            if (portraits != null && i >= 0 && i < portraits.Count && portraits[i] != null)
                return portraits[i];
        }
        return portraitNeutre;
    }
}

[Serializable]
public class Dialogue
{
    public Personnage personnage;            // null = narration / pensée du joueur
    [TextArea(2, 5)] public string texte;
    public bool utiliserExpression;          // faux = portrait neutre
    public Emotion expression;               // émotion du personnage pendant cette ligne
    public bool pensee;                      // vrai = texte en italique, sans sprite
}

/// <summary>Réponse du joueur à une question « quelle émotion ? ».</summary>
[Serializable]
public class ReponseJoueur
{
    public CarteEmotion carteChoisie;
    public Emotion emotionAttendue;
    public bool estCorrecte;
}
