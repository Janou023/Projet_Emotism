using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadDefi : MonoBehaviour
{
    public string sceneName;
    public bool rememberPlayerPositionOnReturn;
    public Vector3 playerReturnPosition;

    [Header("Cartes requises pour entrer")]
    [SerializeField] private string[] requiredEmotions;
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private string requiredChallenge;

    private SpriteRenderer spriteRenderer;
    private Color unlockedColor = Color.white;

    private static Vector3 returnPosition;
    private static string returnSceneName;
    private static bool hasReturnPosition;
    private static bool ecouteRetourActive;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        if (requiredEmotions != null && requiredEmotions.Length > 0)
        {
            if (ProgressManager.Instance == null ||
                !ProgressManager.Instance.HasAllEmotions(requiredEmotions))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(requiredChallenge))
        {
            if (ProgressManager.Instance == null ||
                ProgressManager.Instance.IsChallengeCompleted(requiredChallenge))
                return false;
        }

        return true;
    }

    void ActualiserDisponibilite()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = DefiAccessible() ? unlockedColor : lockedColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && DefiAccessible())
        {
            if (rememberPlayerPositionOnReturn)
            {
                returnPosition = playerReturnPosition;
                returnSceneName = SceneManager.GetActiveScene().name;
                hasReturnPosition = true;
                EcouterChargementScene();
            }

            if (FadeManager.Instance != null)
                FadeManager.Instance.LoadSceneWithFade(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }
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
