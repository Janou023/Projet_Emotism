using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Fenêtre « Nouvelle carte obtenue ! » : affiche l'illustration de la carte
/// (le design fait par l'équipe artistique) et son texte explicatif.
/// - Animation courte et douce : fondu + léger agrandissement, jouée une seule fois.
/// - Aucun clignotement, aucune limite de temps : le joueur ferme avec « Continuer ».
/// À placer sur l'objet PopupCarte, qui doit être DÉSACTIVÉ dans la scène au départ.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CardRewardPopup : MonoBehaviour
{
    [Header("Éléments de la fenêtre")]
    [SerializeField] RectTransform cardTransform;   // la carte qui s'anime
    [SerializeField] Image cardImage;               // reçoit l'illustration de la carte
    [SerializeField] TMP_Text descriptionText;      // texte explicatif (facultatif)
    [SerializeField] Button continueButton;

    [Header("Son (facultatif, court et doux)")]
    [SerializeField] AudioSource audioSource;       // à relier au groupe SFX du mixer
    [SerializeField] AudioClip rewardClip;

    [Header("Animation")]
    [SerializeField] float animationDuration = 0.6f;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    /// <summary>Déclenché quand le joueur ferme la fenêtre : le jeu peut reprendre.</summary>
    public event Action Closed;

    CanvasGroup group;
    Coroutine running;
    bool initialized;
    bool closing;

    void Init()
    {
        if (initialized) return;
        initialized = true;
        group = GetComponent<CanvasGroup>();
        continueButton.onClick.AddListener(Close);
    }

    /// <summary>Affiche la fenêtre pour la carte obtenue.</summary>
    public void Show(CarteEmotion carte)
    {
        if (carte == null)
        {
            Debug.LogWarning("[CardRewardPopup] Aucune carte à afficher.");
            return;
        }

        Init();
        closing = false;
        gameObject.SetActive(true);

        cardImage.sprite = carte.illustration;

        if (descriptionText != null)
        {
            bool hasText = !string.IsNullOrWhiteSpace(carte.texteExplicatif);
            descriptionText.gameObject.SetActive(hasText);
            descriptionText.text = carte.texteExplicatif;
        }

        // Le focus est mis sur « Continuer » : le clavier et la manette fonctionnent tout de suite
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        float duration = animationDuration;
        float startScale = 0.85f;

        group.alpha = 0f;
        cardTransform.localScale = Vector3.one * startScale;

        if (audioSource != null && rewardClip != null)
            audioSource.PlayOneShot(rewardClip);

        // Temps non affecté par la pause (Time.timeScale = 0)
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = ease.Evaluate(Mathf.Clamp01(t / duration));
            group.alpha = p;
            cardTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, p);
            yield return null;
        }

        group.alpha = 1f;
        cardTransform.localScale = Vector3.one;
        running = null;
    }

    /// <summary>Ferme la fenêtre (bouton Continuer). Peut être utilisé pendant l'animation.</summary>
    public void Close()
    {
        if (closing) return;
        closing = true;

        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }

        StartCoroutine(CloseNextFrame());
    }

    IEnumerator CloseNextFrame()
    {
        // On attend une image : si le joueur valide avec Entrée ou Espace, la même touche
        // ne doit pas aussi faire avancer le dialogue qui se trouve derrière.
        yield return null;

        gameObject.SetActive(false);
        closing = false;
        Closed?.Invoke();
    }
}
