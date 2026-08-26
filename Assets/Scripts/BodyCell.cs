using UnityEngine;

public class BodyCell : MonoBehaviour
{
    // Bikin daftar tipe blok
    public enum TipeBlok { Polos, Motif, Kepala }

    [Header("Tipe Jeli")]
    [Tooltip("Pilih jeli ini jenisnya apa")]
    public TipeBlok tipeBlok = TipeBlok.Polos;
    
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Body")]
    [SerializeField] private bool isHead;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sides")]
    [SerializeField] private BodyCellSide upSide;
    [SerializeField] private BodyCellSide downSide;
    [SerializeField] private BodyCellSide leftSide;
    [SerializeField] private BodyCellSide rightSide;

    private Vector2Int gridPosition;

    public bool IsHead => isHead;
    public Vector2Int GridPosition => gridPosition;

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

        if (gridManager != null)
        {
            gridPosition =
                gridManager.WorldToGrid(
                    transform.position
                );

            transform.position =
                gridManager.GridToWorld(
                    gridPosition
                );
        }

        SetupSides();
    }

    // =========================================================
    // SETUP SIDES
    // =========================================================

    private void SetupSides()
    {
        if (upSide != null)
            upSide.Setup(
                this,
                Vector2Int.up
            );

        if (downSide != null)
            downSide.Setup(
                this,
                Vector2Int.down
            );

        if (leftSide != null)
            leftSide.Setup(
                this,
                Vector2Int.left
            );

        if (rightSide != null)
            rightSide.Setup(
                this,
                Vector2Int.right
            );

        HideAllSides();
    }

    // =========================================================
    // HEAD
    // =========================================================

    public void SetAsHead(bool value)
    {
        isHead = value;

        if (spriteRenderer == null)
            return;

        if (isHead)
        {
            spriteRenderer.transform.localScale =
                Vector3.one * 0.8f;
        }
        else
        {
            spriteRenderer.transform.localScale =
                Vector3.one;
        }
    }

    // =========================================================
    // GRID
    // =========================================================

    public void UpdateGridPosition()
    {
        if (gridManager == null)
            return;

        gridPosition =
            gridManager.WorldToGrid(
                transform.position
            );
    }

    public void SetGridPosition(
        Vector2Int position)
    {
        gridPosition = position;
    }

    // =========================================================
    // SIDE AVAILABLE
    // =========================================================

    public void SetSideAvailable(
        Vector2Int direction,
        bool available,
        BodyCell targetCell = null
    )
    {
        if (direction == Vector2Int.up)
        {
            if (upSide != null)
            {
                upSide.SetAvailable(
                    available,
                    targetCell
                );
            }
        }
        else if (direction == Vector2Int.down)
        {
            if (downSide != null)
            {
                downSide.SetAvailable(
                    available,
                    targetCell
                );
            }
        }
        else if (direction == Vector2Int.left)
        {
            if (leftSide != null)
            {
                leftSide.SetAvailable(
                    available,
                    targetCell
                );
            }
        }
        else if (direction == Vector2Int.right)
        {
            if (rightSide != null)
            {
                rightSide.SetAvailable(
                    available,
                    targetCell
                );
            }
        }
    }

    // =========================================================
    // HIDE ALL SIDES
    // =========================================================

    public void HideAllSides()
    {
        SetSideAvailable(
            Vector2Int.up,
            false,
            null
        );

        SetSideAvailable(
            Vector2Int.down,
            false,
            null
        );

        SetSideAvailable(
            Vector2Int.left,
            false,
            null
        );

        SetSideAvailable(
            Vector2Int.right,
            false,
            null
        );
    }

    // =========================================================
    // REQUEST ATTACH
    // =========================================================

    public void RequestAttach(
        Vector2Int direction,
        BodyCell targetCell
    )
    {
        if (targetCell == null)
        {
            Debug.LogWarning(
                $"{name}: targetCell null."
            );

            return;
        }

        Debug.Log(
            $"Attach requested: " +
            $"{name} -> {targetCell.name}"
        );

        // =====================================================
        // CARI PLAYER
        // =====================================================

        Transform player =
            transform.parent;

        if (player == null)
        {
            Debug.LogError(
                $"{name} tidak memiliki Player parent."
            );

            return;
        }

        // =====================================================
        // TARGET ADALAH BAGIAN DARI DETACHED BODY
        // =====================================================

        DetachedBody detachedBody =
            targetCell.GetComponentInParent<DetachedBody>();

        if (detachedBody != null)
        {
            Debug.Log(
                $"Target {targetCell.name} " +
                $"berada di DetachedBody. " +
                $"Mengattach seluruh group."
            );

            detachedBody.AttachToPlayer(
                player
            );

            return;
        }

        // =====================================================
        // TARGET STANDALONE
        // =====================================================

        targetCell.AttachToBody(
            player
        );
    }

    // =========================================================
    // ATTACH TO BODY
    // =========================================================

    public void AttachToBody(
        Transform player
    )
    {
        if (player == null)
            return;

        GridBodyMovement bodyMovement =
            player.GetComponent<GridBodyMovement>();

        if (bodyMovement == null)
        {
            Debug.LogError(
                $"Player {player.name} " +
                $"tidak memiliki GridBodyMovement."
            );

            return;
        }

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

        // =====================================================
        // SAFETY CHECK
        // =====================================================

        DetachedBody detachedBody =
            GetComponentInParent<DetachedBody>();

        if (detachedBody != null)
        {
            Debug.Log(
                $"{name} adalah bagian dari DetachedBody. " +
                $"Mengattach seluruh group."
            );

            detachedBody.AttachToPlayer(
                player
            );

            return;
        }

        // =====================================================
        // SIMPAN POSISI
        // =====================================================

        Vector2Int attachGridPosition =
            gridManager.WorldToGrid(
                transform.position
            );

        // =====================================================
        // REGISTER
        // =====================================================

        bool registered =
            bodyMovement.RegisterAttachedCell(
                this
            );

        if (!registered)
            return;

        // =====================================================
        // PARENT
        // =====================================================

        transform.SetParent(
            player,
            true
        );

        // =====================================================
        // SNAP
        // =====================================================

        transform.position =
            gridManager.GridToWorld(
                attachGridPosition
            );

        SetGridPosition(
            attachGridPosition
        );

        // =====================================================
        // COLLIDER
        // =====================================================

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>(
                true
            );

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }

        // =====================================================
        // HIDE SIDE
        // =====================================================

        HideAllSides();

        Debug.Log(
            $"Body Cell {name} attached ke " +
            $"{player.name} at grid " +
            $"{attachGridPosition}"
        );
    }
}