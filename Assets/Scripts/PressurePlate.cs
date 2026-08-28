using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public enum PintuTipe { Putar, Geser } 

    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private GridManager gridManager;

    [Header("Settings Utama")]
    [SerializeField] private PintuTipe tipePintu = PintuTipe.Putar;
    [SerializeField] private bool stayOpen = false; 

    [Header("Visual Injakan (Ganti Gambar)")]
    [Tooltip("Komponen visual plat. (Otomatis terisi jika dibiarkan kosong)")]
    [SerializeField] private SpriteRenderer plateVisual;
    [Tooltip("Gambar saat plat NGANGGUR (belum diinjak)")]
    [SerializeField] private Sprite spriteNormal;
    [Tooltip("Gambar saat plat DITEKAN")]
    [SerializeField] private Sprite spriteDitekan;

    [Header("Audio (SFX)")]
    [SerializeField] private string openSFX = "Plate Door";
    [SerializeField] private string closeSFX = ""; 

    [Header("Settings Pintu Putar")]
    [SerializeField] private float openRotationAmount = 90f; 

    [Header("Settings Pintu Geser")]
    [SerializeField] private Vector3 geserOffset; 
    [SerializeField] private float kecepatanGeser = 5f;

    private Vector2Int plateGridPosition;
    private bool isDoorOpen = false;

    private Vector3 posisiAwalTertutup;
    private Quaternion rotasiAwalTertutup; 

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        plateGridPosition = gridManager.WorldToGrid(transform.position);

        if (door != null)
        {
            posisiAwalTertutup = door.transform.position;
            rotasiAwalTertutup = door.transform.rotation; 
        }

        // Otomatis mencari SpriteRenderer di objek ini jika belum dimasukkan
        if (plateVisual == null) plateVisual = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        CheckForPlayer();

        if (door != null && tipePintu == PintuTipe.Geser)
        {
            Vector3 targetPosisi = isDoorOpen ? (posisiAwalTertutup + geserOffset) : posisiAwalTertutup;
            door.transform.position = Vector3.MoveTowards(door.transform.position, targetPosisi, kecepatanGeser * Time.deltaTime);
        }
    }

    private void CheckForPlayer()
    {
        bool someoneIsOnPlate = false;
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);
        
        foreach (BodyCell cell in allCells)
        {
            Vector2Int cellPos = gridManager.WorldToGrid(cell.transform.position);
            if (cellPos == plateGridPosition)
            {
                someoneIsOnPlate = true;
                break; 
            }
        }

        if (someoneIsOnPlate && !isDoorOpen) OpenDoor();
        else if (!someoneIsOnPlate && isDoorOpen && !stayOpen) CloseDoor();
    }

    private void OpenDoor()
    {
        isDoorOpen = true;

        // --- GANTI GAMBAR JADI DITEKAN ---
        if (plateVisual != null && spriteDitekan != null)
        {
            plateVisual.sprite = spriteDitekan;
        }

        if (!string.IsNullOrEmpty(openSFX))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(openSFX);
            }
            else
            {
                Debug.LogWarning("Pintu terbuka, tapi AudioManager belum terpasang di Scene!");
            }
        }

        if (tipePintu == PintuTipe.Putar && door != null) 
        {
            door.transform.rotation = rotasiAwalTertutup * Quaternion.Euler(0, 0, openRotationAmount);
        }
    }

    private void CloseDoor()
    {
        isDoorOpen = false;

        // --- KEMBALIKAN GAMBAR KE NORMAL ---
        if (plateVisual != null && spriteNormal != null)
        {
            plateVisual.sprite = spriteNormal;
        }

        if (!string.IsNullOrEmpty(closeSFX))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(closeSFX);
            }
        }

        if (tipePintu == PintuTipe.Putar && door != null) door.transform.rotation = rotasiAwalTertutup; 
    }
}