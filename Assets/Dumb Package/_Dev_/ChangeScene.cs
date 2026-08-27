using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _sceneName;

    public void LoadScene()
    {
        if (string.IsNullOrWhiteSpace(_sceneName))
        {
            Debug.LogWarning(
                $"{nameof(ChangeScene)}: Scene name is empty."
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(_sceneName);
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;

        string currentScene =
            SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentScene);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning(
                $"{nameof(ChangeScene)}: Scene name is empty."
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneName);
    }
}