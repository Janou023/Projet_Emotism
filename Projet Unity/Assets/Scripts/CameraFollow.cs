using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer backgroundBounds;
    public string sceneQuandJoueurHorsChamp;
    public float timeOffset = 0.2f;
    public Vector3 posOffset;
    private Vector3 velocity;
    private bool changementSceneEnCours;
    public Collider2D cameraBounds;

    void Awake()
    {
        if (player != null)
            transform.position = ClampToBackground(player.position + posOffset);
    }

    void Update()
    {
        if (player == null) return;

        if (JoueurHorsChamp())
        {
            ChargerSceneHorsChamp();
            return;
        }

        Vector3 cible = ClampToBackground(player.position + posOffset);
        Vector3 positionLisse = Vector3.SmoothDamp(transform.position, cible, ref velocity, timeOffset);
        Vector3 positionBornee = ClampToBackground(positionLisse);
        if (positionBornee.x != positionLisse.x) velocity.x = 0f;
        if (positionBornee.y != positionLisse.y) velocity.y = 0f;
        transform.position = positionBornee;
    }

    bool JoueurHorsChamp()
    {
        if (string.IsNullOrEmpty(sceneQuandJoueurHorsChamp)) return false;

        Camera camera = GetComponent<Camera>();
        SpriteRenderer spriteJoueur = player.GetComponent<SpriteRenderer>();
        if (camera == null || spriteJoueur == null) return false;

        Bounds limites = spriteJoueur.bounds;
        Vector3 basGauche = camera.WorldToViewportPoint(limites.min);
        Vector3 hautDroite = camera.WorldToViewportPoint(limites.max);
        return hautDroite.x < 0f || basGauche.x > 1f || hautDroite.y < 0f || basGauche.y > 1f;
    }

    void ChargerSceneHorsChamp()
    {
        if (changementSceneEnCours) return;
        changementSceneEnCours = true;

        if (!Application.CanStreamedLevelBeLoaded(sceneQuandJoueurHorsChamp))
        {
            Debug.LogError("CameraFollow : scène de sortie absente des scènes de build : " + sceneQuandJoueurHorsChamp);
            return;
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.LoadSceneWithFade(sceneQuandJoueurHorsChamp);
        else
            SceneManager.LoadScene(sceneQuandJoueurHorsChamp);
    }

    Vector3 ClampToBackground(Vector3 position)
    {
        Bounds limites;
        if (cameraBounds != null)
            limites = cameraBounds.bounds;
        else if (backgroundBounds != null)
            limites = backgroundBounds.bounds;
        else
            return position;

        Camera camera = GetComponent<Camera>();
        if (camera == null || !camera.orthographic) return position;

        float demiHauteurVue = camera.orthographicSize;
        float demiLargeurVue = demiHauteurVue * camera.aspect;

        position.x = ClampDansLimites(position.x, limites.min.x + demiLargeurVue,
            limites.max.x - demiLargeurVue, limites.center.x);
        position.y = ClampDansLimites(position.y, limites.min.y + demiHauteurVue,
            limites.max.y - demiHauteurVue, limites.center.y);
        return position;
    }

    static float ClampDansLimites(float valeur, float minimum, float maximum, float centre)
    {
        if (minimum > maximum) return centre;
        return Mathf.Clamp(valeur, minimum, maximum);
    }
}
