using UnityEngine;

public class Pause : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _pauseMenuUI;

    private bool _isPaused;

    private void Start()
    {
        ResumeGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    #region Pause System

    public void TogglePause()
    {
        if (_isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(false);
        }
    }

    #endregion

    private void OnApplicationQuit()
    {
        Time.timeScale = 1f;
    }
}