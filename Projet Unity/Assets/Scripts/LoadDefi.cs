using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadDefi : MonoBehaviour
{
    public string sceneName;
    public bool rememberPlayerPositionOnReturn;
    public Vector3 playerReturnPosition;

    [Header("Cartes requises pour entrer")]
    [SerializeField] private string[] requiredEmotions;
    [SerializeField] private string idDefiBloqueApresReussite;
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private ContenuTutoriel messageCartesManquantes;

    private SpriteRenderer spriteRenderer;
    private Collider2D colliderEntree;
    private Color unlockedColor = Color.white;

    private static Vector3 returnPosition;
    private static string returnSceneName;
    private static bool hasReturnPosition;
    private static bool ecouteRetourActive;

    public static void DefinirPositionRetour(string scene, Vector3 position)
    {
        if (string.IsNullOrWhiteSpace(scene)) return;
        returnPosition = position;
        returnSceneName = scene;
        hasReturnPosition = true;
        EcouterChargementScene();
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliderEntree = GetComponent<Collider2D>();
        if (spriteRenderer != null)
            unlockedColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged += ActualiserDisponibilite;
        ActualiserDisponibilite();
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= ActualiserDisponibilite;
    }

    bool DefiAccessible()
    {
        if (DefiTermine()) return false;

        if (requiredEmotions != null && requiredEmotions.Length > 0)
        {
            if (ProgressManager.Instance == null)
                return false;

            if (CartesManquantes().Count > 0)
                return false;
        }

        return true;
    }

    List<string> CartesManquantes()
    {
        List<string> manquantes = new List<string>();
        if (ProgressManager.Instance == null || requiredEmotions == null)
            return manquantes;

        foreach (string idEmotion in requiredEmotions)
        {
            if (!ProgressManager.Instance.IsEmotionUnlocked(idEmotion))
                manquantes.Add(idEmotion);
        }

        return manquantes;
    }

    bool DefiTermine()
    {
        return !string.IsNullOrWhiteSpace(idDefiBloqueApresReussite)
            && ProgressManager.Instance != null
            && ProgressManager.Instance.IsChallengeCompleted(idDefiBloqueApresReussite);
    }

    [ContextMenu("Afficher les cartes requises")]
    void AfficherCartesRequises()
    {
        if (requiredEmotions == null || requiredEmotions.Length == 0)
        {
            Debug.Log("[LoadDefi] Aucun prérequis émotionnel.", this);
            return;
        }

        foreach (string id in requiredEmotions)
        {
            bool possede = ProgressManager.Instance != null &&
                ProgressManager.Instance.IsEmotionUnlocked(id);
            Debug.Log($"[LoadDefi] {id} : {(possede ? "possédée" : "manquante")}", this);
        }
    }

    void ActualiserDisponibilite()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = DefiAccessible() ? unlockedColor : lockedColor;

        // Après réussite, le collider d'entrée devient solide : le joueur ne peut
        // plus traverser l'entrée ni relancer le défi.
        if (colliderEntree != null && !string.IsNullOrWhiteSpace(idDefiBloqueApresReussite))
            colliderEntree.isTrigger = !DefiTermine();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!DefiAccessible())
        {
            if (!DefiTermine() && ProgressManager.Instance != null && CartesManquantes().Count > 0)
                AfficherMessageCartesManquantes();
            return;
        }

        if (rememberPlayerPositionOnReturn)
        {
            DefinirPositionRetour(SceneManager.GetActiveScene().name, other.transform.position);
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.LoadSceneWithFade(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    void AfficherMessageCartesManquantes()
    {
        ContenuTutoriel contenu = messageCartesManquantes != null
            ? messageCartesManquantes
            : Resources.Load<ContenuTutoriel>("Tutoriels/CartesManquantesTram");

        if (contenu == null)
        {
            Debug.LogError("[LoadDefi] Le tutoriel des cartes manquantes est introuvable.", this);
            return;
        }

        GestionnaireTutoriels.Instance.AfficherMessage(contenu);
    }

    static void EcouterChargementScene()
    {
        if (ecouteRetourActive) return;
        SceneManager.sceneLoaded += AppliquerPositionAuRetour;
        ecouteRetourActive = true;
    }

    static void AppliquerPositionAuRetour(Scene scene, LoadSceneMode mode)
    {
        if (!hasReturnPosition || scene.name != returnSceneName) return;

        DeplacementJoueur joueur = Object.FindAnyObjectByType<DeplacementJoueur>();
        if (joueur == null) return;

        joueur.transform.position = returnPosition;
        if (joueur.rigidbody != null)
        {
            joueur.rigidbody.position = returnPosition;
            joueur.rigidbody.linearVelocity = Vector2.zero;
        }

        hasReturnPosition = false;
        SceneManager.sceneLoaded -= AppliquerPositionAuRetour;
        ecouteRetourActive = false;
    }

    public static bool TryGetReturnPosition(string sceneName, out Vector3 position)
    {
        if (hasReturnPosition && returnSceneName == sceneName)
        {
            position = returnPosition;
            return true;
        }

        position = default;
        return false;
    }
}
