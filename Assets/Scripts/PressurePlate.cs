using UnityEngine;
using System.Collections.Generic; // Jangan lupa tambahkan ini untuk List

public class PressurePlate : MonoBehaviour
{
    // BIKIN MENU PILIHAN TIPE PINTU DI INSPECTOR
    public enum PintuTipe { Putar, Geser } 

    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private GridManager gridManager; // Tarik GridManager dari scene ke sini

    [Header("Settings Utama")]
    [Tooltip("Pilih injakan ini nyambung ke pintu tipe apa?")]
    [SerializeField] private PintuTipe tipePintu = PintuTipe.Putar; // Defaultnya pintu putar
    [SerializeField] private bool stayOpen = true; 

    [Header("Settings Pintu Putar")]
    [SerializeField] private float closedRotationZ = -90f; 
    [SerializeField] private float openRotationZ = -180f;

    [Header("Settings Pintu Geser")]
    [Tooltip("Isi X = 3 buat geser kanan, X = -3 buat kiri")]
    [SerializeField] private Vector3 geserOffset; 
    [SerializeField] private float kecepatanGeser = 5f;

    private Vector2Int plateGridPosition;
    private bool isDoorOpen = false;

    // Variabel pembantu untuk pintu geser
    private Vector3 posisiAwalTertutup;

    private void Start()
    {
        // Ingat posisi grid dari plat injak ini
        plateGridPosition = gridManager.WorldToGrid(transform.position);

        // Kalau pintunya ada, simpan posisi awalnya buat patokan kalau nanti mau digeser
        if (door != null)
        {
            posisiAwalTertutup = door.transform.position;
        }
    }

    private void Update()
    {
        CheckForPlayer();

        // LOGIKA PINTU GESER (dieksekusi perlahan setiap frame)
        if (door != null && tipePintu == PintuTipe.Geser)
        {
            // Tentukan target tujuan: terbuka atau kembali ke awal
            Vector3 targetPosisi = isDoorOpen ? (posisiAwalTertutup + geserOffset) : posisiAwalTertutup;
            // Gerakkan pintu mulus ke target
            door.transform.position = Vector3.MoveTowards(door.transform.position, targetPosisi, kecepatanGeser * Time.deltaTime);
        }
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
        
        // LOGIKA PINTU PUTAR (langsung pindah sudut)
        if (tipePintu == PintuTipe.Putar && door != null)
        {
            door.transform.rotation = Quaternion.Euler(0, 0, openRotationZ);
        }
    }

    private void CloseDoor()
    {
        isDoorOpen = false;

        // LOGIKA PINTU PUTAR (kembali ke sudut awal)
        if (tipePintu == PintuTipe.Putar && door != null)
        {
            door.transform.rotation = Quaternion.Euler(0, 0, closedRotationZ);
        }
    }
}