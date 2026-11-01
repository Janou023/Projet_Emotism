using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Historique de la conversation : panneau défilant qu'on ouvre avec une touche ou un bouton.</summary>
public class DialogueHistory : MonoBehaviour
{
    [Header("UI")]
    public GameObject historyPanel;     // le panneau complet (activé/désactivé)
    public ScrollRect scrollRect;
    public TextMeshProUGUI historyText;

    public KeyCode toggleKey = KeyCode.H;
    public int maxEntries = 100;

    readonly List<string> entries = new List<string>();

    void Start()
    {
        if (historyPanel != null) historyPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) || (IsOpen && Input.GetKeyDown(KeyCode.Escape))) Toggle();
    }

    public bool IsOpen { get { return historyPanel != null && historyPanel.activeSelf; } }

    public void Toggle()
    {
        if (historyPanel == null)
        {
            Debug.LogError("DialogueHistory : History Panel n'est pas assigné.");
            return;
        }
        historyPanel.SetActive(!historyPanel.activeSelf);
        if (!historyPanel.activeSelf) return;

        historyPanel.transform.SetAsLastSibling();
        Transform historyButton = historyPanel.transform.parent.Find("BtnHistorique");
        if (historyButton != null) historyButton.SetAsLastSibling();
        ScrollToBottom();
    }

    public void Add(string speaker, string text)
    {
        if (historyText == null)
        {
            Debug.LogError("DialogueHistory : HistoryText n'est pas assigné.");
            return;
        }
        string entry = string.IsNullOrEmpty(speaker)
            ? text
            : "<b>" + speaker + "</b>\n" + text;

        entries.Add(entry);
        if (entries.Count > maxEntries)
            entries.RemoveAt(0);
        historyText.text = string.Join("\n\n", entries);
        if (IsOpen) ScrollToBottom();
    }

    public void Clear()
    {
        entries.Clear();
        if (historyText != null) historyText.text = "";
    }

    void ScrollToBottom()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
