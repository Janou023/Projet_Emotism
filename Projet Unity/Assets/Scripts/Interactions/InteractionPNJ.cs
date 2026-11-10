using UnityEngine;

public class InteractionPNJ : MonoBehaviour
{
    public string sceneName;
    private bool playerInRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            FadeManager.Instance.LoadSceneWithFade(sceneName);
            playerInRange = false;
        }
    }
}
