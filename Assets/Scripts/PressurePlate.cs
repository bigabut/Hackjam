using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public enum PintuTipe
    {
        Putar,
        Geser
    }

    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private GridManager gridManager;

    [Header("Settings Utama")]
    [SerializeField] private PintuTipe tipePintu = PintuTipe.Putar;
    [SerializeField] private bool stayOpen = false;

    [Header("Visual Injakan")]
    [Tooltip("Komponen visual plat.")]
    [SerializeField] private SpriteRenderer plateVisual;

    [Tooltip("Gambar saat plat belum diinjak.")]
    [SerializeField] private Sprite spriteNormal;

    [Tooltip("Gambar saat plat ditekan.")]
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

    private bool isDoorOpen;

    private Vector3 posisiAwalTertutup;
    private Quaternion rotasiAwalTertutup;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // GRID MANAGER
        // -----------------------------------------------------

        if (gridManager == null)
        {
            gridManager =
                FindFirstObjectByType<GridManager>();
        }

        if (gridManager == null)
        {
            Debug.LogError(
                $"{name}: GridManager tidak ditemukan."
            );

            return;
        }

        // -----------------------------------------------------
        // PLATE POSITION
        // -----------------------------------------------------

        plateGridPosition =
            gridManager.WorldToGrid(
                transform.position
            );

        // -----------------------------------------------------
        // DOOR INITIAL POSITION
        // -----------------------------------------------------

        if (door != null)
        {
            posisiAwalTertutup =
                door.transform.position;

            rotasiAwalTertutup =
                door.transform.rotation;
        }

        // -----------------------------------------------------
        // VISUAL
        // -----------------------------------------------------

        if (plateVisual == null)
        {
            plateVisual =
                GetComponent<SpriteRenderer>();
        }

        // Pastikan visual mulai dari kondisi normal.
        if (plateVisual != null &&
            spriteNormal != null)
        {
            plateVisual.sprite =
                spriteNormal;
        }

        isDoorOpen = false;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CheckForPlayer();

        UpdateSlidingDoor();
    }

    // =========================================================
    // CHECK PLAYER
    // =========================================================

    // =========================================================
    // CHECK PLAYER
    // =========================================================

    private void CheckForPlayer()
    {
        if (gridManager == null)
            return;

        // KUNCI PERBAIKAN: Cari semua BodyCell yang ada di arena, 
        // baik yang menempel di Player maupun yang sudah terpotong.
        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        bool someoneIsOnPlate = false;

        foreach (BodyCell cell in allCells)
        {
            if (cell == null)
                continue;

            Vector2Int cellPosition =
                cell.GridPosition;

            // -------------------------------------------------
            // FALLBACK
            // -------------------------------------------------
            if (cellPosition !=
                plateGridPosition)
            {
                cellPosition =
                    gridManager.WorldToGrid(
                        cell.transform.position
                    );
            }

            if (cellPosition ==
                plateGridPosition)
            {
                someoneIsOnPlate = true;
                break;
            }
        }

        // =====================================================
        // OPEN
        // =====================================================

        if (someoneIsOnPlate &&
            !isDoorOpen)
        {
            OpenDoor();
        }

        // =====================================================
        // CLOSE
        // =====================================================

        else if (!someoneIsOnPlate &&
                 isDoorOpen &&
                 !stayOpen)
        {
            CloseDoor();
        }
    }

    // =========================================================
    // SLIDING DOOR
    // =========================================================

    private void UpdateSlidingDoor()
    {
        if (door == null)
            return;

        if (tipePintu != PintuTipe.Geser)
            return;

        Vector3 targetPosition =
            isDoorOpen
                ? posisiAwalTertutup +
                  geserOffset
                : posisiAwalTertutup;

        door.transform.position =
            Vector3.MoveTowards(
                door.transform.position,
                targetPosition,
                kecepatanGeser *
                Time.deltaTime
            );
    }

    // =========================================================
    // OPEN DOOR
    // =========================================================

    private void OpenDoor()
    {
        isDoorOpen = true;

        // -----------------------------------------------------
        // VISUAL
        // -----------------------------------------------------

        if (plateVisual != null &&
            spriteDitekan != null)
        {
            plateVisual.sprite =
                spriteDitekan;
        }

        // -----------------------------------------------------
        // AUDIO
        // -----------------------------------------------------

        if (!string.IsNullOrEmpty(openSFX))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    openSFX
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{name}: Pintu terbuka, " +
                    $"tetapi AudioManager belum tersedia."
                );
            }
        }

        // -----------------------------------------------------
        // ROTATING DOOR
        // -----------------------------------------------------

        if (tipePintu ==
                PintuTipe.Putar &&
            door != null)
        {
            door.transform.rotation =
                rotasiAwalTertutup *
                Quaternion.Euler(
                    0f,
                    0f,
                    openRotationAmount
                );
        }
    }

    // =========================================================
    // CLOSE DOOR
    // =========================================================

    private void CloseDoor()
    {
        isDoorOpen = false;

        // -----------------------------------------------------
        // VISUAL
        // -----------------------------------------------------

        if (plateVisual != null &&
            spriteNormal != null)
        {
            plateVisual.sprite =
                spriteNormal;
        }

        // -----------------------------------------------------
        // AUDIO
        // -----------------------------------------------------

        if (!string.IsNullOrEmpty(closeSFX))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    closeSFX
                );
            }
        }

        // -----------------------------------------------------
        // ROTATING DOOR
        // -----------------------------------------------------

        if (tipePintu ==
                PintuTipe.Putar &&
            door != null)
        {
            door.transform.rotation =
                rotasiAwalTertutup;
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public bool IsDoorOpen()
    {
        return isDoorOpen;
    }

    public Vector2Int GetPlateGridPosition()
    {
        return plateGridPosition;
    }
}