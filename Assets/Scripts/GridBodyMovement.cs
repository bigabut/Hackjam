using System.Collections.Generic;
using UnityEngine;

public class GridBodyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Hold Movement")]
    [SerializeField] private float holdDelay = 0.2f;
    [SerializeField] private float repeatRate = 0.1f;

    [Header("Body")]
    [SerializeField] private GameObject bodyCellPrefab;

    private Vector2Int bodyPosition;
    private Vector2Int targetPosition;

    private bool isMoving;

    private Vector2Int heldDirection;
    private float holdTimer;

    private List<Vector2Int> bodyCells =
        new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };

    private List<Transform> cellVisuals =
        new List<Transform>();

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

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        bodyPosition =
            gridManager.WorldToGrid(
                transform.position
            );

        targetPosition =
            bodyPosition;

        transform.position =
            gridManager.GridToWorld(
                bodyPosition
            );

        CreateBodyVisual();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (WinManager.IsGameOver) return;
        
        HandleInput();
        MoveBody();
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        Vector2Int inputDirection =
            GetInputDirection();

        if (inputDirection ==
            Vector2Int.zero)
        {
            heldDirection =
                Vector2Int.zero;

            holdTimer = 0f;

            return;
        }

        // Tombol baru / arah berubah
        if (inputDirection !=
            heldDirection)
        {
            heldDirection =
                inputDirection;

            holdTimer = 0f;

            if (!isMoving)
            {
                TryMove(
                    heldDirection
                );
            }

            return;
        }

        // Tombol masih ditahan
        holdTimer +=
            Time.deltaTime;

        if (holdTimer < holdDelay)
            return;

        if (!isMoving)
        {
            TryMove(
                heldDirection
            );

            holdTimer -=
                repeatRate;
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
    // MOVEMENT
    // =========================================================

    private void TryMove(
        Vector2Int direction)
    {
        // =====================================================
        // CHECK MOVE COUNTER
        // =====================================================

        if (GameCounter.Instance != null)
        {
            if (!GameCounter.Instance.CanMove())
            {
                Debug.Log(
                    "Move counter sudah habis."
                );

                return;
            }
        }

        Vector2Int newPosition =
            bodyPosition + direction;

        // =====================================================
        // 1. CEK BATAS GRID
        // =====================================================

        foreach (Vector2Int cell
                 in bodyCells)
        {
            Vector2Int newCellPosition =
                newPosition + cell;

            if (!gridManager.IsInsideGrid(
                    newCellPosition))
            {
                return;
            }
        }

        // =====================================================
        // 2. CEK BODY CELL LAIN
        // =====================================================

        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        foreach (Vector2Int cell
                 in bodyCells)
        {
            Vector2Int newCellPosition =
                newPosition + cell;

            foreach (BodyCell otherCell
                     in allCells)
            {
                // Abaikan cell milik tubuh sendiri
                if (otherCell.transform.IsChildOf(
                        transform))
                {
                    continue;
                }

                Vector2Int otherPosition =
                    gridManager.WorldToGrid(
                        otherCell.transform.position
                    );

                if (newCellPosition ==
                    otherPosition)
                {
                    Debug.Log(
                        $"Movement blocked by " +
                        $"{otherCell.name} at " +
                        $"{otherPosition}"
                    );

                    return;
                }
            }
        }

        // =====================================================
        // OBSTACLE CHECK (SWEEP TEST / BOXCAST)
        // =====================================================
        foreach (Vector2Int cell in bodyCells)
        {
            Vector2Int currentCellPosition = bodyPosition + cell;
            Vector2Int newCellPosition = newPosition + cell;
            
            Vector3 startPos = gridManager.GridToWorld(currentCellPosition);
            Vector3 targetPos = gridManager.GridToWorld(newCellPosition);

            // NAMA VARIABEL DIGANTI JADI moveDir AGAR TIDAK ERROR
            Vector2 moveDir = (targetPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, targetPos);
            
            Vector2 boxSize = new Vector2(gridManager.CellSize * 0.5f, gridManager.CellSize * 0.5f);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(startPos, boxSize, 0f, moveDir, distance);
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

                if (hit.collider.GetComponent<BodyCell>() != null || 
                    hit.collider.GetComponent<CuttingLine>() != null ||
                    hit.collider.GetComponent<GoalBox>() != null || 
                    hit.collider.GetComponent<PressurePlate>() != null)
                {
                    continue;
                }

                Debug.Log($"Nabrak: {hit.collider.name}");
                return; 
            }
        }

        // =====================================================
        // 3. MOVE BERHASIL
        // =====================================================

        bodyPosition =
            newPosition;

        targetPosition =
            bodyPosition;

        isMoving = true;

        // --- [KODE BARU: Paksa seluruh data anak sinkron instan] ---
        BodyCell[] cells = GetComponentsInChildren<BodyCell>();
        foreach (BodyCell cell in cells)
        {
            cell.SetGridPosition(cell.GridPosition + direction);
        }
        
        RefreshBodyCells();
        
        // =====================================================
        // 4. TAMBAH MOVE COUNTER
        // =====================================================

        if (GameCounter.Instance != null)
        {
            GameCounter.Instance.AddHeadMove();
        }
    }

    // =========================================================
    // MOVE VISUAL
    // =========================================================

    private void MoveBody()
    {
        if (!isMoving)
            return;

        Vector3 targetWorldPosition =
            gridManager.GridToWorld(
                targetPosition
            );

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetWorldPosition,
                moveSpeed *
                Time.deltaTime
            );

        if (Vector3.Distance(
                transform.position,
                targetWorldPosition
            ) < 0.001f)
        {
            transform.position =
                targetWorldPosition;

            isMoving = false;

            // --- [KODE TAMBAHAN FIX] ---
            // Update posisi grid semua BodyCell setelah selesai bergerak
            BodyCell[] cells = GetComponentsInChildren<BodyCell>();
            foreach (BodyCell cell in cells)
            {
                cell.UpdateGridPosition();
            }
            // ---------------------------
        }
    }

    // =========================================================
    // CREATE BODY
    // =========================================================

    private void CreateBodyVisual()
    {
        if (bodyCellPrefab == null)
            return;

        for (int i = 0;
             i < bodyCells.Count;
             i++)
        {
            Vector2Int cell =
                bodyCells[i];

            GameObject visual =
                Instantiate(
                    bodyCellPrefab,
                    transform
                );

            visual.transform.localPosition =
                new Vector3(
                    cell.x *
                    gridManager.CellSize,

                    cell.y *
                    gridManager.CellSize,

                    0f
                );

            BodyCell bodyCell =
                visual.GetComponent<BodyCell>();

            if (bodyCell != null)
            {
                bodyCell.SetAsHead(
                    i == 0
                );

                bodyCell.SetGridPosition(
                    bodyPosition + cell
                );
            }

            cellVisuals.Add(
                visual.transform
            );
        }
    }

    // =========================================================
    // ATTACHMENT
    // =========================================================

    public bool RegisterAttachedCell(
        BodyCell cell)
    {
        if (cell == null)
            return false;

        Vector2Int cellGridPosition =
            gridManager.WorldToGrid(
                cell.transform.position
            );

        Vector2Int relativePosition =
            cellGridPosition -
            bodyPosition;

        if (bodyCells.Contains(
                relativePosition))
        {
            Debug.LogWarning(
                $"BodyCell {cell.name} " +
                $"sudah terdaftar."
            );

            return false;
        }

        bodyCells.Add(
            relativePosition
        );

        cell.SetGridPosition(
            cellGridPosition
        );

        Debug.Log(
            $"BodyCell {cell.name} " +
            $"registered ke body. " +
            $"Relative position: " +
            $"{relativePosition}"
        );

        return true;
    }

    // =========================================================
    // REFRESH BODY
    // =========================================================

    public void RefreshBodyCells()
    {
        bodyCells.Clear();

        BodyCell[] cells =
            GetComponentsInChildren<BodyCell>();

        foreach (BodyCell cell in cells)
        {
            Vector2Int relativePosition =
                cell.GridPosition -
                bodyPosition;

            bodyCells.Add(
                relativePosition
            );
        }
    }

    // =========================================================
    // GET BODY CELLS
    // =========================================================

    public List<Vector2Int> GetBodyCells()
    {
        List<Vector2Int> actualCells =
            new List<Vector2Int>();

        foreach (Vector2Int cell
                 in bodyCells)
        {
            actualCells.Add(
                bodyPosition + cell
            );
        }

        return actualCells;
    }
}