using UnityEngine;

public class WinManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject winPanel;

    private bool hasWon = false;

    // [KODE BARU] Saklar utama! "static" berarti variabel ini berlaku global untuk seluruh game
    public static bool IsGameOver = false; 

    private void Start()
    {
        // Pastikan saklar selalu ker-reset jadi false setiap kali level baru di-play
        IsGameOver = false; 

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (hasWon) return; 

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
            
            // [KODE BARU] Matikan saklar utama game!
            IsGameOver = true; 
            
            Debug.Log("🎉 MENANG! Semua kotak target sudah terisi blok Jelly!");
            
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
            
            // (Hapus kode playerMovement.enabled = false yang kemarin, kita udah nggak butuh itu lagi)
        }
    }
}