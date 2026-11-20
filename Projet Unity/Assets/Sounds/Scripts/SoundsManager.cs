using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

[System.Serializable]
public class MusiqueParScene
{
   public string nomScene;
   public AudioClip clip;
   [Range(0f, 1f)] public float volume = 1f;
}

public class SoundsManager : MonoBehaviour
{
   public static SoundsManager Instance { get; private set; }

   [SerializeField] private AudioSource SoundFxObject;
   [Header("Groupes du mixer")]
   [SerializeField] private AudioMixerGroup groupeMusique;
   [SerializeField] private AudioMixerGroup groupeAmbiance;
   [SerializeField] private AudioMixerGroup groupeSfx;
   [Header("Musiques par scène")]
   [SerializeField] private MusiqueParScene[] musiquesParScene = new MusiqueParScene[0];
   [SerializeField] private string sceneParametres = "MenuParametres";
   [SerializeField] private AudioClip clipAmbiance;
   [SerializeField] private string sceneAmbiance = "Monde";
   [SerializeField, Range(0f, 1f)] private float volumeAmbiance = 1f;
   [SerializeField] private AudioClip clipClavier;
   [SerializeField, Range(0f, 1f)] private float volumeClavier = 0.25f;
   [SerializeField] private AudioClip clipEntreePolePeur;
   [SerializeField, Range(0f, 1f)] private float volumeEntreePolePeur = 1f;
   [SerializeField] private string scenePolePeur = "PolePeur";
   [Header("Sons d'interface")]
   [SerializeField] private AudioClip sonCarteDeposee;
   [SerializeField, Range(0f, 1f)] private float volumeCarteDeposee = 1f;
   [SerializeField] private AudioClip sonClasseurOuvert;
   [SerializeField, Range(0f, 1f)] private float volumeClasseurOuvert = 1f;
   [SerializeField] private AudioClip sonClasseurFerme;
   [SerializeField, Range(0f, 1f)] private float volumeClasseurFerme = 1f;
   [SerializeField] private AudioClip sonCarteCorrecte;
   [SerializeField, Range(0f, 1f)] private float volumeCarteCorrecte = 1f;
   [SerializeField] private AudioClip sonCarteIncorrecte;
   [SerializeField, Range(0f, 1f)] private float volumeCarteIncorrecte = 1f;
   [SerializeField] private AudioClip sonDefiReussi;
   [SerializeField, Range(0f, 1f)] private float volumeDefiReussi = 1f;
   [SerializeField] private AudioClip sonSurvolBouton;
   [SerializeField, Range(0f, 1f)] private float volumeSurvolBouton = 1f;
   [SerializeField] private AudioClip sonClicBouton;
   [SerializeField, Range(0f, 1f)] private float volumeClicBouton = 1f;
   [SerializeField] private AudioClip sonNouvelleCarte;
   [SerializeField, Range(0f, 1f)] private float volumeNouvelleCarte = 1f;
   private AudioSource sourceAmbiance;
   private AudioSource sourceMusique;
   private AudioSource sourceClavier;
   private AudioSource sourceEntreePolePeur;
   private AudioSource sourceClicBouton;
   private int frameSonClasseur = -1;
   private readonly HashSet<Button> boutonsAvecSons = new HashSet<Button>();

   public bool HasChallengeSuccessSound => sonDefiReussi != null;
   public bool HasNewCardSound => sonNouvelleCarte != null;

   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);
         sourceAmbiance = gameObject.AddComponent<AudioSource>();
         sourceAmbiance.playOnAwake = false;
         sourceAmbiance.loop = true;
         sourceAmbiance.spatialBlend = 0f;
         sourceAmbiance.outputAudioMixerGroup = groupeAmbiance;
         sourceMusique = gameObject.AddComponent<AudioSource>();
         sourceMusique.playOnAwake = false;
         sourceMusique.loop = true;
         sourceMusique.spatialBlend = 0f;
         sourceMusique.outputAudioMixerGroup = groupeMusique;
         sourceClavier = gameObject.AddComponent<AudioSource>();
         sourceClavier.playOnAwake = false;
         sourceClavier.loop = true;
         sourceClavier.spatialBlend = 0f;
         sourceClavier.outputAudioMixerGroup = groupeSfx;
         sourceEntreePolePeur = gameObject.AddComponent<AudioSource>();
         sourceEntreePolePeur.playOnAwake = false;
         sourceEntreePolePeur.spatialBlend = 0f;
         sourceEntreePolePeur.outputAudioMixerGroup = groupeSfx;
         SceneManager.sceneLoaded += GererAmbianceScene;
         MettreAJourAmbiance(SceneManager.GetActiveScene());
         MettreAJourMusique(SceneManager.GetActiveScene());
         ConfigurerSonsBoutons();
      }
      else
      {
         Destroy(gameObject);
      }
   }

   private void OnDestroy()
   {
      if (Instance == this)
      {
         SceneManager.sceneLoaded -= GererAmbianceScene;
         Instance = null;
      }
   }

   private void GererAmbianceScene(Scene scene, LoadSceneMode mode)
   {
      MettreAJourAmbiance(scene);
      MettreAJourMusique(scene);
      ConfigurerSonsBoutons();

      if (scene.name == scenePolePeur)
      {
         if (clipEntreePolePeur == null)
         {
            Debug.LogWarning("SoundsManager : assigne un clip au champ Clip Entree Pole Peur.", this);
            return;
         }

         sourceEntreePolePeur.Stop();
         sourceEntreePolePeur.volume = volumeEntreePolePeur;
         sourceEntreePolePeur.PlayOneShot(clipEntreePolePeur);
      }
   }

   private void MettreAJourAmbiance(Scene scene)
   {
      if (sourceAmbiance == null || clipAmbiance == null) return;

      if (scene.name == sceneAmbiance)
      {
         sourceAmbiance.clip = clipAmbiance;
         sourceAmbiance.volume = volumeAmbiance;
         if (!sourceAmbiance.isPlaying)
            sourceAmbiance.Play();
      }
      else
      {
         sourceAmbiance.Stop();
      }
   }

   private void MettreAJourMusique(Scene scene)
   {
      if (sourceMusique == null) return;
      if (scene.name == sceneParametres) return;

      MusiqueParScene musique = null;
      if (musiquesParScene != null)
      {
         foreach (MusiqueParScene entree in musiquesParScene)
         {
            if (entree != null && entree.nomScene == scene.name)
            {
               musique = entree;
               break;
            }
         }
      }

      if (musique == null || musique.clip == null)
      {
         sourceMusique.Stop();
         sourceMusique.clip = null;
         return;
      }

      if (sourceMusique.clip != musique.clip)
      {
         sourceMusique.Stop();
         sourceMusique.clip = musique.clip;
      }

      sourceMusique.volume = musique.volume;
      if (!sourceMusique.isPlaying)
         sourceMusique.Play();
   }

   public void StartTypingSound()
   {
      if (clipClavier == null || sourceClavier == null) return;

      sourceClavier.clip = clipClavier;
      sourceClavier.volume = volumeClavier;
      if (!sourceClavier.isPlaying)
         sourceClavier.Play();
   }

   public void StopTypingSound()
   {
      if (sourceClavier != null && sourceClavier.isPlaying)
         sourceClavier.Stop();
   }

   public void PlayCardPlacedSound() => PlayInterfaceSound(sonCarteDeposee, volumeCarteDeposee);
   public void PlayBinderOpenSound() => PlayBinderSound(sonClasseurOuvert, volumeClasseurOuvert);
   public void PlayBinderCloseSound() => PlayBinderSound(sonClasseurFerme, volumeClasseurFerme);
   public bool PlayCorrectCardSound() => PlayInterfaceSound(sonCarteCorrecte, volumeCarteCorrecte) != null;
   public void PlayWrongCardSound() => PlayInterfaceSound(sonCarteIncorrecte, volumeCarteIncorrecte);
   public bool PlayChallengeSuccessSound() => PlayInterfaceSound(sonDefiReussi, volumeDefiReussi) != null;
   public void PlayButtonHoverSound() => PlayInterfaceSound(sonSurvolBouton, volumeSurvolBouton);
   public void PlayButtonClickSound()
   {
      if (Time.frameCount == frameSonClasseur) return;
      sourceClicBouton = PlayInterfaceSound(sonClicBouton, volumeClicBouton);
   }
   public bool PlayNewCardSound() => PlayInterfaceSound(sonNouvelleCarte, volumeNouvelleCarte) != null;

   void PlayBinderSound(AudioClip clip, float volume)
   {
      frameSonClasseur = Time.frameCount;
      if (sourceClicBouton != null)
      {
         sourceClicBouton.Stop();
         Destroy(sourceClicBouton.gameObject);
         sourceClicBouton = null;
      }

      PlayInterfaceSound(clip, volume);
   }

   AudioSource PlayInterfaceSound(AudioClip clip, float volume)
   {
      if (clip == null) return null;

      AudioSource source = new GameObject("SonInterface").AddComponent<AudioSource>();
      source.playOnAwake = false;
      source.spatialBlend = 0f;
      source.outputAudioMixerGroup = groupeSfx;
      source.clip = clip;
      source.volume = volume;
      source.Play();
      Destroy(source.gameObject, clip.length);
      return source;
   }

   void ConfigurerSonsBoutons()
   {
      boutonsAvecSons.RemoveWhere(bouton => bouton == null);
      Button[] boutons = FindObjectsByType<Button>(
         FindObjectsInactive.Include, FindObjectsSortMode.None);

      foreach (Button bouton in boutons)
      {
         if (bouton == null || !boutonsAvecSons.Add(bouton)) continue;

         EventTrigger trigger = bouton.GetComponent<EventTrigger>();
         if (trigger == null) trigger = bouton.gameObject.AddComponent<EventTrigger>();
         if (trigger.triggers == null) trigger.triggers = new List<EventTrigger.Entry>();

         AjouterEvenementBouton(trigger, bouton, EventTriggerType.PointerEnter, true);
         AjouterEvenementBouton(trigger, bouton, EventTriggerType.PointerClick, false);
      }
   }

   void AjouterEvenementBouton(
      EventTrigger trigger, Button bouton, EventTriggerType type, bool survol)
   {
      if (!survol && EstDansZoneTexteDialogue(bouton)) return;

      EventTrigger.Entry entree = new EventTrigger.Entry { eventID = type };
      entree.callback.AddListener(_ =>
      {
         if (bouton == null || !bouton.IsInteractable()) return;
         if (survol) PlayButtonHoverSound();
         else PlayButtonClickSound();
      });
      trigger.triggers.Add(entree);
   }

   static bool EstDansZoneTexteDialogue(Button bouton)
   {
      DialogueManager[] dialogues = FindObjectsByType<DialogueManager>(
         FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (DialogueManager dialogue in dialogues)
      {
         if (dialogue.dialoguePanel == null) continue;

         Transform panneau = dialogue.dialoguePanel.transform;
         if (bouton.transform == panneau || bouton.transform.IsChildOf(panneau))
            return true;
      }

      return false;
   }

   public void PlaySound(AudioClip clip, Vector3 position, float volume )
   {
      if (clip == null || SoundFxObject == null) return;

    AudioSource audioSource = Instantiate(SoundFxObject, position, Quaternion.identity);
    audioSource.clip = clip;
    audioSource.volume = volume;
   audioSource.outputAudioMixerGroup = groupeSfx;
    audioSource.Play();

    float clipLength = clip.length;
    Destroy(audioSource.gameObject, clipLength);
  }
}
