using UnityEngine;

public class BodyCell : MonoBehaviour
{
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

    private void Start()
    {
        if (gridManager != null)
        {
            gridPosition =
                gridManager.WorldToGrid(transform.position);

            transform.position =
                gridManager.GridToWorld(gridPosition);
        }

        if (upSide != null)
            upSide.Setup(this, Vector2Int.up);

        if (downSide != null)
            downSide.Setup(this, Vector2Int.down);

        if (leftSide != null)
            leftSide.Setup(this, Vector2Int.left);

        if (rightSide != null)
            rightSide.Setup(this, Vector2Int.right);
    }

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

    public void UpdateGridPosition()
    {
        if (gridManager == null)
            return;

        gridPosition =
            gridManager.WorldToGrid(transform.position);
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    public void SetSideAvailable(
        Vector2Int direction,
        bool available,
        BodyCell targetCell = null
    )
    {
        if (direction == Vector2Int.up)
        {
            if (upSide != null)
                upSide.SetAvailable(
                    available,
                    targetCell
                );
        }
        else if (direction == Vector2Int.down)
        {
            if (downSide != null)
                downSide.SetAvailable(
                    available,
                    targetCell
                );
        }
        else if (direction == Vector2Int.left)
        {
            if (leftSide != null)
                leftSide.SetAvailable(
                    available,
                    targetCell
                );
        }
        else if (direction == Vector2Int.right)
        {
            if (rightSide != null)
                rightSide.SetAvailable(
                    available,
                    targetCell
                );
        }
    }

    public void HideAllSides()
    {
        if (upSide != null)
            upSide.SetAvailable(false);

        if (downSide != null)
            downSide.SetAvailable(false);

        if (leftSide != null)
            leftSide.SetAvailable(false);

        if (rightSide != null)
            rightSide.SetAvailable(false);
    }

    // =========================================================
    // ATTACHMENT
    // =========================================================

    public void RequestAttach(
        Vector2Int direction,
        BodyCell targetCell
    )
    {
        if (targetCell == null)
            return;

        Debug.Log(
            $"Attach requested: " +
            $"{name} -> {targetCell.name}"
        );

        // Head/body cell harus berada di dalam Player
        Transform player = transform.parent;

        if (player == null)
        {
            Debug.LogError(
                $"{name} tidak memiliki Player parent."
            );

            return;
        }

        targetCell.AttachToBody(player);
    }

    public void AttachToBody(Transform player)
    {
        if (player == null)
            return;

        GridBodyMovement bodyMovement =
            player.GetComponent<GridBodyMovement>();

        if (bodyMovement == null)
        {
            Debug.LogError(
                "Player tidak memiliki " +
                "GridBodyMovement."
            );

            return;
        }

        // Simpan posisi grid sebelum parent berubah
        Vector2Int gridPositionBeforeAttach =
            gridManager.WorldToGrid(
                transform.position
            );

        // Masukkan ke data body
        bool registered =
            bodyMovement.RegisterAttachedCell(
                this
            );

        if (!registered)
            return;

        // Tetap pertahankan posisi world
        transform.SetParent(
            player,
            true
        );

        SetGridPosition(
            gridPositionBeforeAttach
        );

        // Pastikan collider aktif
        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>(
                true
            );

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }

        Debug.Log(
            $"{name} attached ke {player.name}"
        );
    }
}