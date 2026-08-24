using UnityEngine;
using System.Collections.Generic; // Jangan lupa tambahkan ini untuk List

public class PressurePlate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private GridManager gridManager; // Tarik GridManager dari scene ke sini

    [Header("Settings")]
    [SerializeField] private bool stayOpen = true; 

    [SerializeField] private float closedRotationZ = -90f; 
    [SerializeField] private float openRotationZ = -180f;

    private Vector2Int plateGridPosition;
    private bool isDoorOpen = false;

    private void Start()
    {
        // Ingat posisi grid dari plat injak ini
        plateGridPosition = gridManager.WorldToGrid(transform.position);
    }

    private void Update()
    {
        CheckForPlayer();
    }

    private void CheckForPlayer()
    {
        bool someoneIsOnPlate = false;

        // Cari SEMUA potongan jelly (BodyCell) di scene. 
        // Ini akan mendeteksi Head, Body yang nempel, maupun blok kecil yang terpotong.
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);
        
        foreach (BodyCell cell in allCells)
        {
            // Cek posisinya berdasarkan posisi asli di dunia (WorldToGrid)
            Vector2Int cellPos = gridManager.WorldToGrid(cell.transform.position);

            // Kalau posisi grid blok jelly sama persis dengan plat injak
            if (cellPos == plateGridPosition)
            {
                someoneIsOnPlate = true;
                break; // Keluar dari loop karena udah ketemu minimal 1 yang nginjak
            }
        }

        // Buka pintu kalau ada yang nginjak (dan pintu belum terbuka)
        if (someoneIsOnPlate && !isDoorOpen)
        {
            OpenDoor();
        }
        // Tutup pintu kalau nggak ada yang nginjak (dan settingnya boleh nutup)
        else if (!someoneIsOnPlate && isDoorOpen && !stayOpen)
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        isDoorOpen = true;
        // Memaksa pintu langsung ke rotasi -180 (pasti ke atas)
        door.transform.rotation = Quaternion.Euler(0, 0, openRotationZ);
    }

    private void CloseDoor()
    {
        isDoorOpen = false;
        // Memaksa pintu kembali ke rotasi -90 (pasti tertutup horizontal)
        door.transform.rotation = Quaternion.Euler(0, 0, closedRotationZ);
    }
}    