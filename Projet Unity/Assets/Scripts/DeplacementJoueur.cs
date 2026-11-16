using UnityEngine;
using UnityEngine.SceneManagement;

public class DeplacementJoueur : MonoBehaviour
{
    private const string CleSceneSauvegardee = "SauvegardeJoueur_Scene";
    private static bool verificationSceneInitialeEffectuee;

    public float moveSpeed = 3f;
    public bool freeMovement;
    public Rigidbody2D rigidbody;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Vector2 movement;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserVerificationSceneInitiale()
    {
        verificationSceneInitialeEffectuee = false;
    }

    void Awake()
    {
        if (verificationSceneInitialeEffectuee)
        {
            return;
        }

        verificationSceneInitialeEffectuee = true;
        string sceneSauvegardee = PlayerPrefs.GetString(CleSceneSauvegardee, string.Empty);
        string sceneActuelle = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneSauvegardee) || sceneSauvegardee == sceneActuelle)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneSauvegardee))
        {
            Debug.LogWarning("La scene sauvegardee n'est pas disponible dans les scenes du jeu : " + sceneSauvegardee, this);
            return;
        }

        Debug.Log("Chargement de la scene sauvegardee : " + sceneSauvegardee, this);
        SceneManager.LoadScene(sceneSauvegardee);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SauvegarderProgression();
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (!freeMovement)
        {
            movement.y = 0;
        }

        movement = movement.normalized;

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.sqrMagnitude > 0.1f)
        {
            spriteRenderer.flipX = movement.x < 0;
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
    }

    void FixedUpdate()
    {
        rigidbody.linearVelocity = movement * moveSpeed;
    }

    public void SauvegarderProgression()
    {
        if (DialogueManager.IsActive)
        {
            Debug.LogWarning("Sauvegarde ignoree : impossible de sauvegarder pendant un dialogue.", this);
            return;
        }

        PlayerPrefs.SetString(CleSceneSauvegardee, SceneManager.GetActiveScene().name);
        ClasseurEmotions.SauvegarderProgression();

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.Save();
        }

        PlayerPrefs.Save();
        Debug.Log("Sauvegarde effectuee : scene, cartes, tutoriels et preferences.", this);
    }
}
