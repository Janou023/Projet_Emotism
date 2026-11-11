using UnityEngine;

public class LoadDefi : MonoBehaviour
{
    public string sceneName;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FadeManager.Instance.LoadSceneWithFade(sceneName);
        }
    }
}