using UnityEngine;

public class SoundsManager : MonoBehaviour
{
   public static SoundsManager Instance { get; private set; }

   [SerializeField] private AudioSource SoundFxObject;
   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);
      }
      else
      {
         Destroy(gameObject);
      }
   }

   public void PlaySound(AudioClip clip, Vector3 position, float volume )
   {
    AudioSource audioSource = Instantiate(SoundFxObject, position, Quaternion.identity);
    audioSource.clip = clip;
    audioSource.volume = volume;
    audioSource.Play();

    float clipLength = clip.length;
    Destroy(audioSource.gameObject, clipLength);
  }
}
