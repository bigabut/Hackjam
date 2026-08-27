using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Variables

    private static GameManager instance;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    #region Public Methods

    public static void GameOver()
    {
        Debug.Log("[GameManager] Game Over!");
        Time.timeScale = 0f;
    }

    #endregion
}