using UnityEngine;

public class WinManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject winPanel;

    private bool hasWon = false;
    public static bool IsGameOver = false; 

    private GridBodyMovement playerBody; // Referensi ke player

    private void Start()
    {
        IsGameOver = false; 

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        // Cari player di scene
        playerBody = FindFirstObjectByType<GridBodyMovement>();
    }

    private void Update()
    {
        if (hasWon) return; 

        // KUNCI PERBAIKAN: Kalau player masih animasi gerak (meluncur), jangan cek kemenangan dulu!
        if (playerBody != null && playerBody.IsMoving()) return;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        GoalBox[] activeBoxes = FindObjectsByType<GoalBox>(FindObjectsSortMode.None);
        if (activeBoxes.Length == 0) return;

        bool allFilled = true;
        foreach (GoalBox box in activeBoxes)
        {
            if (!box.IsFilled)
            {
                allFilled = false;
                break; 
            }
        }

        if (allFilled)
        {
            hasWon = true;
            IsGameOver = true; 
            
            Debug.Log("🎉 MENANG! Semua kotak target sudah terisi blok Jelly!");
            
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                AudioManager.Instance.PlaySFX("Win");
            }
        }
    }
}