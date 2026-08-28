
using UnityEngine;

public class GoalBox : MonoBehaviour
{
    [SerializeField]
    private GridManager gridManager;

    // =========================================================
    // SYARAT MENANG
    // =========================================================

    [Header("Syarat Menang")]
    [Tooltip("Kotak ini HANYA mau menerima jeli tipe apa?")]
    public BodyCell.TipeBlok syaratTipe =
        BodyCell.TipeBlok.Polos;

    // =========================================================
    // VISUAL
    // =========================================================

    [Header("Visual Asset")]

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite gambarPolos;

    [SerializeField]
    private Sprite gambarMotif;

    [SerializeField]
    private Sprite gambarKepala;

    // =========================================================
    // DATA
    // =========================================================

    private Vector2Int gridPosition;

    public bool IsFilled { get; private set; }

    // =========================================================
    // ON VALIDATE
    // =========================================================

    private void OnValidate()
    {
        UpdateGoalSprite();
    }

    // =========================================================
    // UPDATE SPRITE
    // =========================================================

    private void UpdateGoalSprite()
    {
        if (spriteRenderer == null)
            return;

        switch (syaratTipe)
        {
            case BodyCell.TipeBlok.Polos:

                spriteRenderer.sprite =
                    gambarPolos;

                break;

            case BodyCell.TipeBlok.Motif:

                spriteRenderer.sprite =
                    gambarMotif;

                break;

            case BodyCell.TipeBlok.Kepala:

                spriteRenderer.sprite =
                    gambarKepala;

                break;
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
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

        gridPosition =
            gridManager.WorldToGrid(
                transform.position
            );

        transform.position =
            gridManager.GridToWorld(
                gridPosition
            );

        UpdateGoalSprite();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CheckForBodyCell();
    }

    // =========================================================
    // CHECK BODY CELL
    // =========================================================

    private void CheckForBodyCell()
    {
        IsFilled = false;

        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        foreach (BodyCell cell in allCells)
        {
            if (cell == null)
                continue;

            // =================================================
            // CEK POSISI
            // =================================================

            if (cell.GridPosition != gridPosition)
                continue;

            // =================================================
            // CEK TIPE
            // =================================================

            if (cell.TipeBlokSaatIni != syaratTipe)
                continue;

            // =================================================
            // BERHASIL
            // =================================================

            IsFilled = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    "Input Success"
                );
            }

            break;
        }
    }
}

