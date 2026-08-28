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

    [Header("Collision")]
    [SerializeField] private float collisionSize = 0.8f;

    private Vector2Int bodyPosition;
    private Vector2Int targetPosition;

    private bool isMoving;

    private Vector2Int heldDirection;
    private float holdTimer;

    private readonly List<Vector2Int> bodyCells =
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

            enabled = false;
            return;
        }

        FindHeadCell();

        if (headCell == null)
        {
            Debug.LogError(
                $"{name}: Head BodyCell tidak ditemukan."
            );

            return;
        }

        bodyPosition =
            gridManager.WorldToGrid(
                headCell.transform.position
            );

        transform.position =
            gridManager.GridToWorld(
                bodyPosition
            );

        targetPosition =
            bodyPosition;

        InitializeExistingBody();

        UpdateAllCellGridPositions();
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
        if (headCell != null)
        {
            GridBodyMovement parentBody =
                headCell.GetComponentInParent<
                    GridBodyMovement
                >();

            if (parentBody == this)
                return;

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

            Vector2Int gridPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            if (cell == headCell ||
                cell.IsHead)
            {
                headCell = cell;
                gridPosition =
                    bodyPosition;
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

        if (!bodyCells.Contains(
                Vector2Int.zero))
        {
            bodyCells.Add(
                Vector2Int.zero
            );
        }
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        Vector2Int direction =
            GetInputDirection();

        if (direction == Vector2Int.zero)
        {
            heldDirection =
                Vector2Int.zero;

            holdTimer = 0f;

            return;
        }

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

        Vector2Int newPosition =
            bodyPosition +
            direction;

        // =====================================================
        // GRID BOUNDARY
        // =====================================================

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

        // =====================================================
        // COLLISION
        // =====================================================

        if (IsMovementBlocked(
                newPosition))
        {
            return;
        }

        // =====================================================
        // MOVE
        // =====================================================

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
    // MOVEMENT COLLISION
    // =========================================================

    private bool IsMovementBlocked(
        Vector2Int newBodyPosition)
    {
        foreach (Vector2Int relativeCell
                 in bodyCells)
        {
            Vector2Int newCellPosition =
                newBodyPosition +
                relativeCell;

            if (IsSolidAt(
                    newCellPosition))
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // CHECK SOLID OBJECT
    // =========================================================

    private bool IsSolidAt(
        Vector2Int gridPosition)
    {
        Vector3 worldPosition =
            gridManager.GridToWorld(
                gridPosition
            );

        float size =
            gridManager.CellSize *
            collisionSize;

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                worldPosition,
                Vector2.one * size,
                0f
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            // =================================================
            // TRIGGER
            // =================================================

            if (hit.isTrigger)
            {
                continue;
            }

            // =================================================
            // BODY CELL
            // =================================================

            BodyCell bodyCell =
                hit.GetComponentInParent<
                    BodyCell
                >();

            if (bodyCell != null)
            {
                // Body sendiri tidak menghalangi
                if (bodyCell.transform.IsChildOf(
                        transform))
                {
                    continue;
                }

                // Detached body memang menghalangi
                DetachedBody detachedBody =
                    bodyCell.GetComponentInParent<
                        DetachedBody
                    >();

                if (detachedBody != null)
                {
                    Debug.Log(
                        $"Movement blocked by " +
                        $"DetachedBody: {bodyCell.name}"
                    );

                    return true;
                }

                // BodyCell standalone juga menghalangi
                Debug.Log(
                    $"Movement blocked by BodyCell: " +
                    $"{bodyCell.name}"
                );

                return true;
            }

            // =================================================
            // DETACHED BODY
            // =================================================

            DetachedBody detached =
                hit.GetComponentInParent<
                    DetachedBody
                >();

            if (detached != null)
            {
                Debug.Log(
                    $"Movement blocked by " +
                    $"DetachedBody: {detached.name}"
                );

                return true;
            }

            // =================================================
            // PRESSURE PLATE
            // =================================================

            PressurePlate pressurePlate =
                hit.GetComponentInParent<
                    PressurePlate
                >();

            if (pressurePlate != null)
            {
                continue;
            }

            // =================================================
            // CUTTING LINE
            // =================================================

            CuttingLine cuttingLine =
                hit.GetComponentInParent<
                    CuttingLine
                >();

            if (cuttingLine != null)
            {
                continue;
            }

            // =================================================
            // PLAYER
            // =================================================

            if (hit.transform.IsChildOf(
                    transform))
            {
                continue;
            }

            // =================================================
            // OTHER NON-SOLID OBJECT
            // =================================================

            if (hit.GetComponentInParent<
                    BodyAttachment
                >() != null)
            {
                continue;
            }

            // =================================================
            // REAL OBSTACLE
            // =================================================

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
    // UPDATE CELL GRID POSITIONS
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

        if (headCell != null)
        {
            headCell.SetGridPosition(
                bodyPosition
            );
        }
    }

    // =========================================================
    // REFRESH BODY
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

        FindHeadCell();

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

        isMoving = false;

        bodyCells.Clear();

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

            if (cell == headCell ||
                cell.IsHead)
            {
                headCell = cell;
                gridPosition =
                    bodyPosition;
            }

            cell.SetGridPosition(
                gridPosition
            );

            Vector2Int relative =
                gridPosition -
                bodyPosition;

            if (!bodyCells.Contains(
                    relative))
            {
                bodyCells.Add(
                    relative
                );
            }
        }

        if (!bodyCells.Contains(
                Vector2Int.zero))
        {
            bodyCells.Add(
                Vector2Int.zero
            );
        }
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

        Vector2Int gridPosition =
            gridManager.WorldToGrid(
                cell.transform.position
            );

        // =====================================================
        // HEAD
        // =====================================================

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

        // =====================================================
        // NORMAL CELL
        // =====================================================

        Vector2Int relative =
            gridPosition -
            bodyPosition;

        if (bodyCells.Contains(
                relative))
        {
            return false;
        }

        if (!gridManager.IsInsideGrid(
                gridPosition))
        {
            Debug.LogWarning(
                $"Tidak bisa attach {cell.name}. " +
                $"Posisi {gridPosition} di luar grid."
            );

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

        Gizmos.color =
            Color.green;

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