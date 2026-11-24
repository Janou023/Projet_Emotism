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
    [SerializeField] private AudioClip clipDeplacement;
    [SerializeField] private float intervalleEntrePas = 0.4f;
    [SerializeField] private float volumePas = 0.1f;
    private float prochainPas;

    void Awake()
    {
        if (LoadDefi.TryGetReturnPosition(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, out Vector3 position))
        {
            transform.position = position;
            if (rigidbody != null)
                rigidbody.position = position;
            return;
        }

        if (ProgressManager.Instance != null && ProgressManager.Instance.Data.position != null
            && ProgressManager.Instance.Data.position.carte == SceneManager.GetActiveScene().name)
        {
            Vector2 positionSauvegardee = new Vector2(
                ProgressManager.Instance.Data.position.x,
                ProgressManager.Instance.Data.position.y);
            transform.position = positionSauvegardee;
            if (rigidbody != null) rigidbody.position = positionSauvegardee;
        }
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

        movement = Vector2.ClampMagnitude(movement, 1f);

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.sqrMagnitude > 0.01f)
        {
            spriteRenderer.flipX = movement.x < 0;
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);

            if (Time.time >= prochainPas)
            {
                if (clipDeplacement != null && SoundsManager.Instance != null)
                    SoundsManager.Instance.PlaySound(clipDeplacement, transform.position, volumePas);

                prochainPas = Time.time + Mathf.Max(0.05f, intervalleEntrePas);
            }
        }
        else
        {
            prochainPas = 0f;
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
        SauvegarderPosition();
        ClasseurEmotions.SauvegarderProgression();

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.Save();
        }

        PlayerPrefs.Save();
        Debug.Log("Sauvegarde effectuee : scene, cartes, tutoriels et preferences.", this);
    }

    void OnApplicationPause(bool enPause)
    {
        if (enPause) SauvegarderPosition();
    }

    void OnApplicationQuit()
    {
        SauvegarderPosition();
    }

    void OnDestroy()
    {
        if (Application.isPlaying) SauvegarderPosition();
    }

    void SauvegarderPosition()
    {
        if (ProgressManager.Instance == null) return;
        Vector3 position = transform.position;
        string sceneJoueur = gameObject.scene.name;
        ProgressManager.Instance.SavePosition(sceneJoueur, position.x, position.y);
        PlayerPrefs.SetString(CleSceneSauvegardee, sceneJoueur);
        PlayerPrefs.Save();
    }
}
