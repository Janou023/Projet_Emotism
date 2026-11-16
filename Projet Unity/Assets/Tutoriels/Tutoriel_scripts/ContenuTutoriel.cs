using UnityEngine;

[CreateAssetMenu(fileName = "NouveauTutoriel", menuName = "Tutoriels/Contenu")]
public class ContenuTutoriel : ScriptableObject
{
    public string id;
    public string titre;

    [TextArea(3, 8)]
    public string texte;
}
