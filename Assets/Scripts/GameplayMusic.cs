using UnityEngine;

public class GameplayMusic : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(
                "Ingame Music"
            );
        }
    }
}