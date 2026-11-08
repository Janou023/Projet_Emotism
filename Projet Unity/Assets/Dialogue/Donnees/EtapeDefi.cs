using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum TypeQuestion { Aucune, Choix, CarteEmotion }

public class ChoixReponse
{
    public string texte;
    public int suivant;   // numéro de l'étape où ce choix mène
}

/// <summary>
/// Une étape du JSON (un objet du tableau). Règles de lecture :
///  - phrases : répliques du personnage ; pensees : narration du joueur
///  - question.paragraphe_N : choix qui mène à l'étape N
///  - question.reponse_X    : réplique du joueur, mène à l'étape suivante (num + 1)
///  - cartes non vide       : question « quelle émotion ? », cartes[0] = bonne réponse
///  - fin = true            : le dialogue s'arrête après cette étape
/// </summary>
public class EtapeDefi
{
    public int num;
    public List<string> pensees = new List<string>();
    public List<string> phrases = new List<string>();
    public string personnage = "";
    public bool fin;
    public TypeQuestion type = TypeQuestion.Aucune;
    public string invite = "";                       // texte de la question affiché
    public List<ChoixReponse> choix = new List<ChoixReponse>();
    public string emotionAttendue = "";              // brut : "peur", "colère"...

    public static Dictionary<int, EtapeDefi> Charger(string json)
    {
        Dictionary<int, EtapeDefi> resultat = new Dictionary<int, EtapeDefi>();
        JArray tableau = JArray.Parse(json);

        foreach (JToken token in tableau)
        {
            JObject o = token as JObject;
            if (o == null) continue;

            EtapeDefi e = new EtapeDefi();
            e.num = (int)o["num"];
            e.phrases = Textes(o["phrases"]);
            e.pensees = Textes(o["pensees"]);
            e.personnage = (string)o["personnage"] ?? "";
            JToken fin = o["fin"];
            e.fin = fin != null && (bool)fin;

            List<string> cartes = Textes(o["cartes"]);
            List<ChoixReponse> paragraphes = new List<ChoixReponse>();
            List<ChoixReponse> reponses = new List<ChoixReponse>();

            JObject q = o["question"] as JObject;
            if (q != null)
            {
                foreach (JProperty p in q.Properties())
                {
                    string valeur = (string)p.Value;
                    if (p.Name == "question")
                    {
                        e.invite = valeur;
                    }
                    else if (p.Name.StartsWith("paragraphe_"))
                    {
                        int cible;
                        if (int.TryParse(p.Name.Substring("paragraphe_".Length), out cible))
                            paragraphes.Add(new ChoixReponse { texte = valeur, suivant = cible });
                    }
                    else if (p.Name.StartsWith("reponse_"))
                    {
                        reponses.Add(new ChoixReponse { texte = valeur, suivant = e.num + 1 });
                    }
                }
            }

            if (cartes.Count > 0)
            {
                e.type = TypeQuestion.CarteEmotion;
                e.emotionAttendue = cartes[0];
            }
            else if (paragraphes.Count > 0)
            {
                e.type = TypeQuestion.Choix;
                e.choix = paragraphes;
            }
            else if (reponses.Count > 0)
            {
                e.type = TypeQuestion.Choix;
                e.choix = reponses;
            }

            resultat[e.num] = e;
        }
        return resultat;
    }

    static List<string> Textes(JToken t)
    {
        List<string> liste = new List<string>();
        JArray a = t as JArray;
        if (a != null)
            foreach (JToken x in a) liste.Add((string)x);
        return liste;
    }

    // minuscules + sans accents : "Colère" == "colere"
    public static string Normaliser(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string d = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in d)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool TryParseEmotion(string brut, out Emotion emotion)
    {
        switch (Normaliser(brut))
        {
            case "joie":      emotion = Emotion.Joie;      return true;
            case "colere":    emotion = Emotion.Colere;    return true;
            case "peur":      emotion = Emotion.Peur;      return true;
            case "tristesse": emotion = Emotion.Tristesse; return true;
            case "stresse":   emotion = Emotion.Stresse;   return true;
        }
        emotion = Emotion.Joie;
        return false;
    }
}
