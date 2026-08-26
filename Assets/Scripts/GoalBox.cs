using UnityEngine;

public class GoalBox : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    
    [Header("Syarat Menang")]
    [Tooltip("Kotak ini HANYA mau menerima jeli tipe apa?")]
    public BodyCell.TipeBlok syaratTipe = BodyCell.TipeBlok.Polos;

    [Header("Visual Asset (Isi dengan gambarmu)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite gambarPolos;
    [SerializeField] private Sprite gambarMotif;
    [SerializeField] private Sprite gambarKepala;

    private Vector2Int gridPosition;
    public bool IsFilled { get; private set; }

    // Fitur sakti Unity: Berjalan otomatis saat kamu ubah nilai di Inspector
    private void OnValidate()
    {
        if (spriteRenderer == null) return;

        // Otomatis ganti gambar sesuai tipe yang dipilih
        switch (syaratTipe)
        {
            case BodyCell.TipeBlok.Polos:
                spriteRenderer.sprite = gambarPolos;
                break;
            case BodyCell.TipeBlok.Motif:
                spriteRenderer.sprite = gambarMotif;
                break;
            case BodyCell.TipeBlok.Kepala:
                spriteRenderer.sprite = gambarKepala;
                break;
        }
    }

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        gridPosition = gridManager.WorldToGrid(transform.position);
        transform.position = gridManager.GridToWorld(gridPosition);
    }

    private void Update()
    {
        CheckForBodyCell();
    }

    private void CheckForBodyCell()
    {
        IsFilled = false;
        
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);
        
        foreach (BodyCell cell in allCells)
        {
            if (cell.GridPosition == gridPosition && cell.tipeBlok == syaratTipe)
            {
                IsFilled = true;
                break; 
            }
        }
    }
}