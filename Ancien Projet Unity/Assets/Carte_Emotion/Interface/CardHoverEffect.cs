using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public bool isUnlocked = false; // Géré automatiquement par EmotionBinder
    [SerializeField] private float hoverScaleFactor = 1.06f;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // L'effet ne se déclenche que si la carte est débloquée
        if (isUnlocked)
        {
            transform.localScale = originalScale * hoverScaleFactor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    void OnDisable()
    {
        transform.localScale = originalScale;
    }
}