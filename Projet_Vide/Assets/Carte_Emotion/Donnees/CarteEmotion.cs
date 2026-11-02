using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public class CarteEmotion
{
    public string id;                  // ex: "peur", "colere", "degout"
    public Emotion emotion;            // Joie, Colere, Peur, Tristesse, Stresse
    public Sprite illustration;        // Carte débloquée
    public Sprite lockedSprite;        // Dos avec cadenas
    [TextArea(3, 5)]
    public string texteExplicatif;    // Description affichée lors du zoom
}