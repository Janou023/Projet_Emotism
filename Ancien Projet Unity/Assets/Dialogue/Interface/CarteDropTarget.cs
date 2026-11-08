using UnityEngine;
using UnityEngine.EventSystems;

public class CarteDropTarget : MonoBehaviour, IDropHandler
{
    UIClasseur classeur;

    public void Configurer(UIClasseur uiClasseur)
    {
        classeur = uiClasseur;
    }

    public void OnDrop(PointerEventData eventData)
    {
        CarteEmotionDragHandler carte = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<CarteEmotionDragHandler>()
            : null;

        if (carte != null && carte.AppartientA(classeur) && classeur != null)
            classeur.AccepterDepotCarte();
    }
}
