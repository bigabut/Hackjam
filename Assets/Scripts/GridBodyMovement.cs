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

    // Semua cell tubuh disimpan sebagai posisi RELATIF
    // terhadap bodyPosition.
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

    public Transform GetBodyTransform()
    {
        return transform;
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

        // Pastikan semua BodyCell langsung
        // memiliki GridPosition yang benar.
        UpdateAllBodyCellGridPositions();
    }

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

        if (holdTimer <
            holdDelay)
        {
            return;
        }

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
        Vector2Int newPosition =
            bodyPosition +
            direction;

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
                // BodyCell milik kita sendiri
                // tidak dianggap obstacle.
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
        // 2.5 CEK TEMBOK / PINTU / OBSTACLE (PAKAI OVERLAP BOX)
        // =====================================================
        foreach (Vector2Int cell in bodyCells)
        {
            Vector2Int newCellPosition = newPosition + cell;
            Vector3 worldPosToCheck = gridManager.GridToWorld(newCellPosition);

            // Bikin area kotak deteksi sedikit lebih kecil dari ukuran grid asli (0.8x)
            // Biar nggak salah deteksi tembok di kotak sebelah
            Vector2 boxSize = new Vector2(gridManager.CellSize * 0.8f, gridManager.CellSize * 0.8f);

            // Cek semua objek di dalam area kotak tersebut
            Collider2D[] hits = Physics2D.OverlapBoxAll(worldPosToCheck, boxSize, 0f);

            foreach (Collider2D hit in hits)
            {
                // Abaikan kalau yang ketabrak adalah diri sendiri / body-nya sendiri
                if (hit.transform.IsChildOf(this.transform) || hit.gameObject == this.gameObject)
                {
                    continue; 
                }

                // Kalau nabrak collider yang solid (bukan trigger) dan bukan BodyCell
                if (!hit.isTrigger && hit.GetComponent<BodyCell>() == null)
                {
                    Debug.Log($"Nabrak tembok/rintangan bernama: {hit.gameObject.name}");
                    return; // Batalkan pergerakan!
                }
            }
        }

        // =====================================================
        // 3. MOVE
        // =====================================================

        bodyPosition =
            newPosition;

        targetPosition =
            bodyPosition;

        isMoving = true;
    }

    // =========================================================
    // ACTUAL MOVEMENT
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

        // Movement selesai
        if (Vector3.Distance(
                transform.position,
                targetWorldPosition
            ) < 0.001f)
        {
            transform.position =
                targetWorldPosition;

            // =================================================
            // PENTING
            // =================================================
            // Setelah player selesai bergerak,
            // update posisi grid semua BodyCell.
            UpdateAllBodyCellGridPositions();

            isMoving = false;
        }
    }

    // =========================================================
    // UPDATE BODY CELL GRID POSITION
    // =========================================================

    public void UpdateAllBodyCellGridPositions()
    {
        BodyCell[] cells =
            GetComponentsInChildren<BodyCell>();

        foreach (BodyCell cell
                 in cells)
        {
            Vector2Int gridPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            cell.SetGridPosition(
                gridPosition
            );
        }
    }

    // =========================================================
    // BODY CREATION
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
                    bodyPosition +
                    cell
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

        // Jangan masukkan dua kali
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

        foreach (BodyCell cell
                 in cells)
        {
            // Pastikan GridPosition cell
            // sudah sesuai posisi world-nya.
            Vector2Int gridPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            cell.SetGridPosition(
                gridPosition
            );

            Vector2Int relativePosition =
                gridPosition -
                bodyPosition;

            if (!bodyCells.Contains(
                    relativePosition))
            {
                bodyCells.Add(
                    relativePosition
                );
            }
        }
    }

    // =========================================================
    // DATA
    // =========================================================

    public List<Vector2Int> GetBodyCells()
    {
        List<Vector2Int> actualCells =
            new List<Vector2Int>();

        foreach (Vector2Int cell
                 in bodyCells)
        {
            actualCells.Add(
                bodyPosition +
                cell
            );
        }

        return actualCells;
    }

    public Vector2Int GetBodyPosition()
    {
        return bodyPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}