using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarteEmotionDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    UIClasseur classeur;
    RectTransform carteRect;
    Transform parentInitial;
    int siblingInitial;
    Vector2 positionInitiale;
    Vector2 ancreMinInitiale;
    Vector2 ancreMaxInitiale;
    Vector2 pivotInitial;
    Vector3 echelleInitiale;
    bool raycastInitial;
    bool glissementActif;

    public int CarteIndex { get; private set; }

    public void Configurer(UIClasseur uiClasseur, int index)
    {
        classeur = uiClasseur;
        CarteIndex = index;
        carteRect = transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (classeur == null || !classeur.CommencerGlisserCarte(CarteIndex, eventData))
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("CarteEmotionDragHandler : aucun Canvas parent pour déplacer la carte.");
            classeur.TerminerGlisserCarte(CarteIndex);
            return;
        }

        parentInitial = transform.parent;
        siblingInitial = transform.GetSiblingIndex();
        positionInitiale = carteRect.anchoredPosition;
        ancreMinInitiale = carteRect.anchorMin;
        ancreMaxInitiale = carteRect.anchorMax;
        pivotInitial = carteRect.pivot;
        echelleInitiale = carteRect.localScale;
        Image image = GetComponent<Image>();
        if (image != null)
        {
            raycastInitial = image.raycastTarget;
            image.raycastTarget = false;
        }

        transform.SetParent(canvas.transform, false);
        carteRect.anchorMin = new Vector2(0.5f, 0.5f);
        carteRect.anchorMax = new Vector2(0.5f, 0.5f);
        carteRect.pivot = new Vector2(0.5f, 0.5f);
        carteRect.localScale = echelleInitiale;
        glissementActif = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (glissementActif && classeur != null)
            classeur.GlisserCarte(CarteIndex, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!glissementActif) return;

        transform.SetParent(parentInitial, false);
        transform.SetSiblingIndex(siblingInitial);
        carteRect.anchorMin = ancreMinInitiale;
        carteRect.anchorMax = ancreMaxInitiale;
        carteRect.pivot = pivotInitial;
        carteRect.localScale = echelleInitiale;
        carteRect.anchoredPosition = positionInitiale;

        Image image = GetComponent<Image>();
        if (image != null) image.raycastTarget = raycastInitial;

        glissementActif = false;
        classeur.TerminerGlisserCarte(CarteIndex);
    }

    public bool AppartientA(UIClasseur uiClasseur)
    {
        return glissementActif && classeur == uiClasseur
            && classeur.EstCarteGlissee(this);
    }
}
