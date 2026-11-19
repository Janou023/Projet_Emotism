using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Écran de réussite éditable dans la hiérarchie de la scène Dialogue.</summary>
public class EcranReussiteDefi : MonoBehaviour
{
    [Header("Éléments de l'écran")]
    [SerializeField] TMP_Text titre;
    [SerializeField] Button boutonContinuer;

    [Header("Son")]
    [SerializeField] AudioSource sourceAudio;
    [SerializeField] AudioClip sonPersonnalise;

    Action continuer;
    static AudioClip sonParDefaut;

    void Awake()
    {
        if (sourceAudio == null) sourceAudio = GetComponent<AudioSource>();
        if (boutonContinuer != null)
            boutonContinuer.onClick.AddListener(FermerEtContinuer);
    }

    public void Afficher(Action quandContinuer)
    {
        continuer = quandContinuer;
        gameObject.SetActive(true);

        if (sourceAudio != null)
        {
            float volumeSfx = SettingsManager.Instance != null
                ? SettingsManager.Instance.Data.sfxVolume : 1f;
            if (sonPersonnalise != null)
                sourceAudio.PlayOneShot(sonPersonnalise, volumeSfx);
            else
                JouerSonReussite(sourceAudio);
        }
    }

    void FermerEtContinuer()
    {
        Action callback = continuer;
        continuer = null;
        gameObject.SetActive(false);
        if (callback != null) callback();
    }

    /// <summary>Joue le même jingle sur une bonne réponse carte émotion.</summary>
    public static void JouerSonReussite(AudioSource source = null)
    {
        AudioClip clip = ObtenirSonParDefaut();
        if (clip == null) return;

        bool sourceTemporaire = source == null;
        if (sourceTemporaire)
        {
            GameObject objetSon = new GameObject("SonReussite");
            source = objetSon.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        float volumeSfx = SettingsManager.Instance != null
            ? SettingsManager.Instance.Data.sfxVolume : 1f;
        source.PlayOneShot(clip, volumeSfx);
        if (sourceTemporaire)
            Destroy(source.gameObject, clip.length + 0.1f);
    }

    static AudioClip ObtenirSonParDefaut()
    {
        if (sonParDefaut != null) return sonParDefaut;

        const int frequenceEchantillonnage = 44100;
        const float duree = 0.95f;
        int nombreEchantillons = Mathf.CeilToInt(frequenceEchantillonnage * duree);
        float[] echantillons = new float[nombreEchantillons];
        float[] debuts = { 0f, 0.16f, 0.32f };
        float[] frequences = { 659.25f, 783.99f, 987.77f };
        float[] durees = { 0.34f, 0.42f, 0.62f };

        for (int i = 0; i < nombreEchantillons; i++)
        {
            float temps = i / (float)frequenceEchantillonnage;
            float valeur = 0f;
            for (int note = 0; note < frequences.Length; note++)
            {
                float tempsNote = temps - debuts[note];
                if (tempsNote < 0f || tempsNote > durees[note]) continue;
                float attaque = Mathf.Clamp01(tempsNote / 0.012f);
                float enveloppe = attaque * Mathf.Exp(-4.5f * tempsNote);
                float phase = 2f * Mathf.PI * frequences[note] * tempsNote;
                valeur += enveloppe * (Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 2f) * 0.2f);
            }
            echantillons[i] = valeur * 0.16f;
        }

        sonParDefaut = AudioClip.Create("SonDefiReussi", nombreEchantillons, 1,
            frequenceEchantillonnage, false);
        sonParDefaut.SetData(echantillons, 0);
        return sonParDefaut;
    }
}
