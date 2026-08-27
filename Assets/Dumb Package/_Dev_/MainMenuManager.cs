using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _chooseLevelPanel;
    [SerializeField] private GameObject _optionPanel;
    [SerializeField] private GameObject _creditPanel;
    [SerializeField] private GameObject _tutorialPanel;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Play Main Menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("OSTMainMenu");
        }

        // Default: hanya Main Menu yang terbuka
        ShowPanel(_mainMenuPanel, false);
        ShowPanel(_chooseLevelPanel, false);
        ShowPanel(_optionPanel, false);
        ShowPanel(_creditPanel, false);
        ShowPanel(_tutorialPanel, false);

        ShowPanel(_mainMenuPanel, true);
    }

    // =========================================================
    // PANEL SYSTEM
    // =========================================================

    private void CloseAllPanels()
    {
        ShowPanel(_mainMenuPanel, false);
        ShowPanel(_chooseLevelPanel, false);
        ShowPanel(_optionPanel, false);
        ShowPanel(_creditPanel, false);
        ShowPanel(_tutorialPanel, false);
    }

    private void ShowPanel(
        GameObject panel,
        bool active
    )
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void OpenPanel(
        GameObject panel
    )
    {
        if (panel == null)
        {
            Debug.LogWarning(
                "Panel reference is missing."
            );

            return;
        }

        CloseAllPanels();

        panel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                "UI Pop Up"
            );
        }
    }

    // =========================================================
    // MAIN MENU
    // =========================================================

    public void OpenMainMenu()
    {
        OpenPanel(_mainMenuPanel);
    }

    // =========================================================
    // CHOOSE LEVEL
    // =========================================================

    public void OpenChooseLevel()
    {
        OpenPanel(_chooseLevelPanel);
    }

    // =========================================================
    // OPTIONS
    // =========================================================

    public void OpenOptions()
    {
        OpenPanel(_optionPanel);
    }

    // =========================================================
    // CREDITS
    // =========================================================

    public void OpenCredits()
    {
        OpenPanel(_creditPanel);
    }

    // =========================================================
    // TUTORIAL
    // =========================================================

    public void OpenTutorial()
    {
        OpenPanel(_tutorialPanel);
    }

    // =========================================================
    // PLAY GAME
    // =========================================================

    public void PlayGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                "UI Button"
            );
        }

        Debug.Log(
            "Play Game button pressed."
        );

        // TODO:
        // Load gameplay scene here.
    }

    // =========================================================
    // EXIT
    // =========================================================

    public void ExitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                "UI Button"
            );
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}