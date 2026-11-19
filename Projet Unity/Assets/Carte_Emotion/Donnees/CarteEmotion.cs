using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public class CarteEmotion
{
    [InspectorName("Identifiant")]
    [Tooltip("Identifiant stable utilisé par la sauvegarde, par exemple joie, colere ou peur.")]
    public string id;

    [InspectorName("Émotion")]
    public Emotion emotion;

    [InspectorName("Face de la carte (recto)")]
    [Tooltip("Image affichée quand la carte est débloquée et dans son aperçu agrandi.")]
    public Sprite illustration;

    [InspectorName("Dos de la carte (verrouillée)")]
    [Tooltip("Image affichée tant que la carte n'est pas débloquée.")]
    public Sprite lockedSprite;

    [InspectorName("Illustration affichée au clic")]
    [Tooltip("Illustration affichée dans le zoom quand cette carte est débloquée. Si vide, le recto est affiché.")]
    public Sprite illustrationExplicative;

    [InspectorName("Texte de récompense (facultatif)")]
    [Tooltip("Texte de la fenêtre de récompense. Le zoom du carnet utilise l'illustration ci-dessus.")]
    [TextArea(3, 8)]
    public string texteExplicatif;
}
