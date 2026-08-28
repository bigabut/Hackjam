using System.Collections.Generic;
using UnityEngine;

public class GridBodyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Head")]
    [SerializeField] private BodyCell headCell;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Hold Movement")]
    [SerializeField] private float holdDelay = 0.2f;
    [SerializeField] private float repeatRate = 0.1f;

    private Vector2Int bodyPosition;
    private Vector2Int targetPosition;

    private bool isMoving;

    private Vector2Int heldDirection;
    private float holdTimer;

    // Semua posisi BodyCell relatif terhadap Head.
    private List<Vector2Int> bodyCells =
        new List<Vector2Int>();

    // =========================================================
    // PUBLIC
    // =========================================================

    public Transform GetBodyTransform()
    {
        return transform;
    }

    public Vector2Int GetBodyPosition()
    {
        return bodyPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public BodyCell GetHeadCell()
    {
        return headCell;
    }

    public GridManager GetGridManager()
    {
        return gridManager;
    }

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
        // FIND HEAD
        // -----------------------------------------------------

        FindHeadCell();

        if (headCell == null)
        {
            Debug.LogError(
                $"{name}: Head BodyCell tidak ditemukan."
            );
        }

        // -----------------------------------------------------
        // BODY POSITION
        // -----------------------------------------------------

        bodyPosition =
            gridManager.WorldToGrid(
                transform.position
            );

        // Kalau Head ditemukan, gunakan posisi Head
        // sebagai posisi utama body.
        if (headCell != null)
        {
            bodyPosition =
                gridManager.WorldToGrid(
                    headCell.transform.position
                );

            transform.position =
                gridManager.GridToWorld(
                    bodyPosition
                );
        }
        else
        {
            transform.position =
                gridManager.GridToWorld(
                    bodyPosition
                );
        }

        targetPosition =
            bodyPosition;

        // -----------------------------------------------------
        // INITIALIZE BODY
        // -----------------------------------------------------

        InitializeExistingBody();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (gridManager == null)
            return;

        HandleInput();
        MoveBody();
    }

    // =========================================================
    // FIND HEAD
    // =========================================================

    private void FindHeadCell()
    {
        // Kalau sudah diassign manual dan masih valid
        if (headCell != null)
        {
            if (headCell.transform.IsChildOf(transform) ||
                headCell.transform == transform)
            {
                return;
            }

            headCell = null;
        }

        BodyCell[] cells =
            GetComponentsInChildren<BodyCell>(
                true
            );

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            if (!cell.IsHead)
                continue;

            headCell = cell;

            Debug.Log(
                $"{name}: Head ditemukan -> {cell.name}"
            );

            return;
        }
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeExistingBody()
    {
        bodyCells.Clear();

        BodyCell[] cells =
            GetComponentsInChildren<BodyCell>(
                true
            );

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            // -------------------------------------------------
            // POSITION
            // -------------------------------------------------

            Vector2Int gridPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            // -------------------------------------------------
            // HEAD
            // -------------------------------------------------

            if (cell.IsHead)
            {
                headCell = cell;

                gridPosition =
                    bodyPosition;
            }

            cell.SetGridPosition(
                gridPosition
            );

            // -------------------------------------------------
            // RELATIVE POSITION
            // -------------------------------------------------

            Vector2Int relative =
                gridPosition -
                bodyPosition;

            if (!bodyCells.Contains(relative))
            {
                bodyCells.Add(relative);
            }
        }

        // Head harus selalu berada pada relative (0,0)
        if (!bodyCells.Contains(Vector2Int.zero))
        {
            bodyCells.Add(Vector2Int.zero);
        }

        Debug.Log(
            $"{name}: Initialized " +
            $"{bodyCells.Count} BodyCells. " +
            $"Head = {(headCell != null ? headCell.name : "NULL")}"
        );
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        Vector2Int direction =
            GetInputDirection();

        // -----------------------------------------------------
        // NO INPUT
        // -----------------------------------------------------

        if (direction == Vector2Int.zero)
        {
            heldDirection =
                Vector2Int.zero;

            holdTimer = 0f;

            return;
        }

        // -----------------------------------------------------
        // NEW DIRECTION
        // -----------------------------------------------------

        if (direction != heldDirection)
        {
            heldDirection =
                direction;

            holdTimer = 0f;

            if (!isMoving)
            {
                TryMove(
                    heldDirection
                );
            }

            return;
        }

        // -----------------------------------------------------
        // HOLD
        // -----------------------------------------------------

        holdTimer +=
            Time.deltaTime;

        if (holdTimer < holdDelay)
            return;

        if (!isMoving)
        {
            TryMove(
                heldDirection
            );

            holdTimer =
                Mathf.Max(
                    0f,
                    holdTimer - repeatRate
                );
        }
    }

    private Vector2Int GetInputDirection()
    {
        if (Input.GetKey(KeyCode.W))
            return Vector2Int.up;

        if (Input.GetKey(KeyCode.S))
            return Vector2Int.down;

        if (Input.GetKey(KeyCode.A))
            return Vector2Int.left;

        if (Input.GetKey(KeyCode.D))
            return Vector2Int.right;

        return Vector2Int.zero;
    }

    // =========================================================
    // TRY MOVE
    // =========================================================

    private void TryMove(
        Vector2Int direction)
    {
        if (isMoving)
            return;

        // -----------------------------------------------------
        // COUNTER
        // -----------------------------------------------------

        if (GameCounter.Instance != null)
        {
            if (!GameCounter.Instance.CanMove())
            {
                Debug.Log(
                    "Head movement counter habis."
                );

                return;
            }
        }

        // -----------------------------------------------------
        // NEW BODY POSITION
        // -----------------------------------------------------

        Vector2Int newPosition =
            bodyPosition +
            direction;

        // -----------------------------------------------------
        // GRID BOUNDARY
        // -----------------------------------------------------

        foreach (Vector2Int relativeCell
                 in bodyCells)
        {
            Vector2Int newCellPosition =
                newPosition +
                relativeCell;

            if (!gridManager.IsInsideGrid(
                    newCellPosition))
            {
                Debug.Log(
                    $"Movement blocked: " +
                    $"{newCellPosition} di luar grid."
                );

                return;
            }
        }

        // -----------------------------------------------------
        // OBSTACLE
        // -----------------------------------------------------

        foreach (Vector2Int relativeCell
                 in bodyCells)
        {
            Vector2Int newCellPosition =
                newPosition +
                relativeCell;

            if (IsObstacleAt(
                    newCellPosition))
            {
                Debug.Log(
                    $"Movement blocked by obstacle at " +
                    $"{newCellPosition}"
                );

                return;
            }
        }

        // -----------------------------------------------------
        // DETACHED BODY
        // -----------------------------------------------------

        if (IsBlockedByDetachedBody(
                newPosition))
        {
            return;
        }

        // -----------------------------------------------------
        // MOVE
        // -----------------------------------------------------

        bodyPosition =
            newPosition;

        targetPosition =
            newPosition;

        isMoving = true;

        if (GameCounter.Instance != null)
        {
            GameCounter.Instance.AddHeadMove();
        }
    }

    // =========================================================
    // DETACHED BODY CHECK
    // =========================================================

    private bool IsBlockedByDetachedBody(
        Vector2Int newBodyPosition)
    {
        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        foreach (Vector2Int relativeCell
                 in bodyCells)
        {
            Vector2Int newCellPosition =
                newBodyPosition +
                relativeCell;

            foreach (BodyCell otherCell
                     in allCells)
            {
                if (otherCell == null)
                    continue;

                // -------------------------------------------------
                // BODY SENDIRI
                // -------------------------------------------------

                if (otherCell.transform.IsChildOf(
                        transform))
                {
                    continue;
                }

                // -------------------------------------------------
                // HEAD
                // -------------------------------------------------

                if (otherCell.IsHead)
                {
                    continue;
                }

                // -------------------------------------------------
                // POSITION
                // -------------------------------------------------

                Vector2Int otherPosition =
                    otherCell.GridPosition;

                if (newCellPosition !=
                    otherPosition)
                {
                    continue;
                }

                Debug.Log(
                    $"Movement blocked by detached body: " +
                    $"{otherCell.name} at " +
                    $"{otherPosition}"
                );

                return true;
            }
        }

        return false;
    }

    // =========================================================
    // OBSTACLE
    // =========================================================

    private bool IsObstacleAt(
        Vector2Int gridPosition)
    {
        Vector3 worldPosition =
            gridManager.GridToWorld(
                gridPosition
            );

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                worldPosition,
                Vector2.one *
                gridManager.CellSize *
                0.8f,
                0f
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            // -------------------------------------------------
            // BODY CELL
            // -------------------------------------------------

            BodyCell bodyCell =
                hit.GetComponentInParent<BodyCell>();

            if (bodyCell != null)
            {
                continue;
            }

            // -------------------------------------------------
            // PLAYER
            // -------------------------------------------------

            if (hit.transform.IsChildOf(
                    transform))
            {
                continue;
            }

            // -------------------------------------------------
            // CUTTING LINE
            // -------------------------------------------------

            CuttingLine cuttingLine =
                hit.GetComponentInParent<CuttingLine>();

            if (cuttingLine != null)
            {
                continue;
            }

            // -------------------------------------------------
            // DETACHED BODY CONTAINER
            // -------------------------------------------------

            DetachedBody detachedBody =
                hit.GetComponentInParent<DetachedBody>();

            if (detachedBody != null)
            {
                continue;
            }

            // -------------------------------------------------
            // REAL OBSTACLE
            // -------------------------------------------------

            return true;
        }

        return false;
    }

    // =========================================================
    // MOVE BODY
    // =========================================================

    private void MoveBody()
    {
        if (!isMoving)
            return;

        Vector3 targetWorld =
            gridManager.GridToWorld(
                targetPosition
            );

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetWorld,
                moveSpeed *
                Time.deltaTime
            );

        if (Vector3.Distance(
                transform.position,
                targetWorld
            ) < 0.001f)
        {
            transform.position =
                targetWorld;

            isMoving = false;

            UpdateAllCellGridPositions();
        }
    }

    // =========================================================
    // UPDATE CELL POSITIONS
    // =========================================================

    private void UpdateAllCellGridPositions()
    {
        BodyCell[] cells =
            GetComponentsInChildren<BodyCell>(
                true
            );

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            Vector2Int gridPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            cell.SetGridPosition(
                gridPosition
            );
        }

        // Head selalu berada pada posisi body
        if (headCell != null)
        {
            headCell.SetGridPosition(
                bodyPosition
            );
        }
    }

    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshBodyCells()
    {
        if (gridManager == null)
        {
            gridManager =
                FindFirstObjectByType<GridManager>();
        }

        if (gridManager == null)
            return;

        // -----------------------------------------------------
        // FIND HEAD AGAIN
        // -----------------------------------------------------

        FindHeadCell();

        // -----------------------------------------------------
        // IF HEAD EXISTS
        // -----------------------------------------------------

        if (headCell != null)
        {
            bodyPosition =
                gridManager.WorldToGrid(
                    headCell.transform.position
                );
        }
        else
        {
            bodyPosition =
                gridManager.WorldToGrid(
                    transform.position
                );
        }

        targetPosition =
            bodyPosition;

        // -----------------------------------------------------
        // BODY CELLS
        // -----------------------------------------------------

        bodyCells.Clear();

        BodyCell[] cells =
            GetComponentsInChildren<BodyCell>(
                true
            );

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            Vector2Int gridPosition;

            // Head adalah anchor
            if (cell == headCell ||
                cell.IsHead)
            {
                headCell = cell;

                gridPosition =
                    bodyPosition;
            }
            else
            {
                gridPosition =
                    gridManager.WorldToGrid(
                        cell.transform.position
                    );
            }

            cell.SetGridPosition(
                gridPosition
            );

            Vector2Int relative =
                gridPosition -
                bodyPosition;

            if (!bodyCells.Contains(relative))
            {
                bodyCells.Add(relative);
            }
        }

        // -----------------------------------------------------
        // MAKE SURE HEAD EXISTS IN BODY DATA
        // -----------------------------------------------------

        if (!bodyCells.Contains(
                Vector2Int.zero))
        {
            bodyCells.Add(
                Vector2Int.zero
            );
        }

        Debug.Log(
            $"{name}: Body refreshed. " +
            $"Cells = {bodyCells.Count}, " +
            $"Head = {(headCell != null ? headCell.name : "NULL")}"
        );
    }

    // =========================================================
    // REGISTER ATTACHED CELL
    // =========================================================

    public bool RegisterAttachedCell(
        BodyCell cell)
    {
        if (cell == null)
            return false;

        if (gridManager == null)
        {
            gridManager =
                FindFirstObjectByType<GridManager>();
        }

        if (gridManager == null)
            return false;

        // -----------------------------------------------------
        // HEAD
        // -----------------------------------------------------

        if (cell.IsHead)
        {
            headCell = cell;

            cell.SetGridPosition(
                bodyPosition
            );

            if (!bodyCells.Contains(
                    Vector2Int.zero))
            {
                bodyCells.Add(
                    Vector2Int.zero
                );
            }

            return true;
        }

        // -----------------------------------------------------
        // NORMAL CELL
        // -----------------------------------------------------

        Vector2Int gridPosition =
            cell.GridPosition;

        Vector2Int relative =
            gridPosition -
            bodyPosition;

        if (bodyCells.Contains(
                relative))
        {
            return false;
        }

        bodyCells.Add(
            relative
        );

        cell.SetGridPosition(
            gridPosition
        );

        return true;
    }

    // =========================================================
    // GET BODY CELLS
    // =========================================================

    public List<Vector2Int> GetBodyCells()
    {
        List<Vector2Int> result =
            new List<Vector2Int>();

        foreach (Vector2Int relative
                 in bodyCells)
        {
            result.Add(
                bodyPosition +
                relative
            );
        }

        return result;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (gridManager == null)
            return;

        Gizmos.color = Color.green;

        foreach (Vector2Int relative
                 in bodyCells)
        {
            Vector2Int position =
                bodyPosition +
                relative;

            Gizmos.DrawWireCube(
                gridManager.GridToWorld(
                    position
                ),
                Vector3.one *
                gridManager.CellSize *
                0.8f
            );
        }
    }
}