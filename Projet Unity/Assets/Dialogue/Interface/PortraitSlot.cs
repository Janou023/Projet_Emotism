using UnityEngine;
using UnityEngine.UI;

/// <summary>Un emplacement de sprite (gauche ou droite) : glisse pour entrer/sortir et s'assombrit quand le perso ne parle pas.</summary>
[RequireComponent(typeof(Image))]
public class PortraitSlot : MonoBehaviour
{
    public Vector2 hiddenOffset = new Vector2(-200f, 0f); // d'où il arrive (gauche : -200, droite : +200)
    public float speed = 10f;
    public float hopHeight = 25f;                          // petit saut quand le perso prend la parole
    public Color speakingColor = Color.white;
    public Color silentColor = new Color(0.55f, 0.55f, 0.6f, 1f);

    Image image;
    RectTransform rt;
    Vector2 restPos;
    Vector2 targetPos;
    Color targetColor;
    Vector2 layoutOffset;
    bool visible;
    bool speaking;

    void Awake()
    {
        image = GetComponent<Image>();
        rt = (RectTransform)transform;
        restPos = rt.anchoredPosition;
        HideInstant();
    }

    void HideInstant()
    {
        visible = false;
        speaking = false;
        targetPos = restPos + layoutOffset + hiddenOffset;
        rt.anchoredPosition = targetPos;
        targetColor = new Color(1f, 1f, 1f, 0f);
        image.color = targetColor;
    }

    public void Show(Sprite sprite, bool speaking)
    {
        if (sprite == null) { SetSpeaking(false); return; }

        if (!visible)
        {
            rt.anchoredPosition = restPos + layoutOffset + hiddenOffset;
            Color c = silentColor; c.a = 0f;
            image.color = c;
        }
        else if (speaking && image.sprite != sprite)
        {
            // changement d'expression : petit saut
            rt.anchoredPosition = restPos + layoutOffset + Vector2.up * hopHeight;
        }

        image.sprite = sprite;
        image.preserveAspect = true;
        visible = true;
        targetPos = restPos;
        SetSpeaking(speaking);
    }

    public void SetSpeaking(bool speaking)
    {
        this.speaking = speaking;
        if (!visible) return;
        targetColor = speaking ? speakingColor : silentColor;
        targetPos = restPos + layoutOffset + (speaking ? Vector2.up * hopHeight * 0.5f : Vector2.zero);
    }

    public void SetLayoutOffset(Vector2 offset)
    {
        layoutOffset = offset;
        targetPos = restPos + layoutOffset;
        if (visible && speaking)
            targetPos += Vector2.up * hopHeight * 0.5f;
        else if (!visible)
            targetPos += hiddenOffset;
    }

    public void Hide()
    {
        visible = false;
        speaking = false;
        targetPos = restPos + layoutOffset + hiddenOffset;
        targetColor = new Color(1f, 1f, 1f, 0f);
    }

    void Update()
    {
        float k = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, k);
        image.color = Color.Lerp(image.color, targetColor, k);
    }
}
