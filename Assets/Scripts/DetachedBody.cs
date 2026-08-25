using System.Collections.Generic;
using UnityEngine;

public class DetachedBody : MonoBehaviour
{
    private List<BodyCell> cells =
        new List<BodyCell>();

    private bool isAttaching;

    // =========================================================
    // REGISTER CELL
    // =========================================================

    public void RegisterCell(
        BodyCell cell
    )
    {
        if (cell == null)
            return;

        if (cells.Contains(cell))
            return;

        cells.Add(cell);
    }

    // =========================================================
    // REGISTER GROUP
    // =========================================================

    public void RegisterGroup(
        List<BodyCell> group,
        GridManager gridManager
    )
    {
        if (group == null ||
            group.Count == 0)
        {
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError(
                "DetachedBody: GridManager null."
            );

            return;
        }

        cells.Clear();

        // =====================================================
        // CARI ORIGIN
        // =====================================================

        Vector2Int minGrid =
            group[0].GridPosition;

        foreach (BodyCell cell in group)
        {
            if (cell == null)
                continue;

            minGrid.x =
                Mathf.Min(
                    minGrid.x,
                    cell.GridPosition.x
                );

            minGrid.y =
                Mathf.Min(
                    minGrid.y,
                    cell.GridPosition.y
                );
        }

        // =====================================================
        // LEPAS CONTAINER
        // =====================================================

        transform.SetParent(
            null,
            true
        );

        transform.position =
            gridManager.GridToWorld(
                minGrid
            );

        // =====================================================
        // MASUKKAN SEMUA CELL
        // =====================================================

        foreach (BodyCell cell in group)
        {
            if (cell == null)
                continue;

            Vector2Int relative =
                cell.GridPosition -
                minGrid;

            cell.transform.SetParent(
                transform,
                false
            );

            cell.transform.localPosition =
                new Vector3(
                    relative.x *
                    gridManager.CellSize,

                    relative.y *
                    gridManager.CellSize,

                    0f
                );

            cell.SetGridPosition(
                cell.GridPosition
            );

            RegisterCell(cell);
        }

        Debug.Log(
            $"DetachedBody registered " +
            $"{cells.Count} cells."
        );
    }

    // =========================================================
    // ATTACH ENTIRE GROUP
    // =========================================================

    public void AttachToPlayer(
        Transform player
    )
    {
        if (player == null)
            return;

        // =====================================================
        // PREVENT DOUBLE ATTACH
        // =====================================================

        if (isAttaching)
        {
            Debug.Log(
                $"DetachedBody {name} " +
                $"sudah sedang di-attach."
            );

            return;
        }

        isAttaching = true;

        // =====================================================
        // PLAYER BODY
        // =====================================================

        GridBodyMovement bodyMovement =
            player.GetComponent<GridBodyMovement>();

        if (bodyMovement == null)
        {
            Debug.LogError(
                "Player tidak memiliki " +
                "GridBodyMovement."
            );

            isAttaching = false;
            return;
        }

        // =====================================================
        // VALIDASI GROUP
        // =====================================================

        if (cells.Count == 0)
        {
            Debug.LogWarning(
                $"DetachedBody {name} " +
                $"tidak memiliki cell."
            );

            isAttaching = false;
            return;
        }

        // =====================================================
        // COPY GROUP
        // =====================================================

        List<BodyCell> cellsToAttach =
            new List<BodyCell>();

        List<Vector2Int> positions =
            new List<Vector2Int>();

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            cellsToAttach.Add(cell);
            positions.Add(
                cell.GridPosition
            );
        }

        if (cellsToAttach.Count == 0)
        {
            isAttaching = false;
            return;
        }

        Debug.Log(
            $"Attaching DetachedBody {name}. " +
            $"Total group: " +
            $"{cellsToAttach.Count} cells."
        );

        // =====================================================
        // REGISTER SEMUA CELL
        // =====================================================

        foreach (BodyCell cell in cellsToAttach)
        {
            bodyMovement.RegisterAttachedCell(
                cell
            );
        }

        // =====================================================
        // PARENT SEMUA CELL KE PLAYER
        // =====================================================

        for (int i = 0;
             i < cellsToAttach.Count;
             i++)
        {
            BodyCell cell =
                cellsToAttach[i];

            Vector2Int gridPosition =
                positions[i];

            cell.transform.SetParent(
                player,
                true
            );

            cell.transform.position =
                FindGridWorldPosition(
                    gridPosition
                );

            cell.SetGridPosition(
                gridPosition
            );

            cell.HideAllSides();

            Collider2D[] colliders =
                cell.GetComponentsInChildren<Collider2D>(
                    true
                );

            foreach (Collider2D collider in colliders)
            {
                collider.enabled = true;
            }
        }

        // =====================================================
        // CLEAR GROUP
        // =====================================================

        cells.Clear();

        // =====================================================
        // DESTROY CONTAINER
        // =====================================================

        Destroy(gameObject);

        // =====================================================
        // REFRESH PLAYER BODY
        // =====================================================

        bodyMovement.RefreshBodyCells();

        Debug.Log(
            $"DetachedBody {name} berhasil " +
            $"digabungkan sebagai satu group. " +
            $"Cells: {cellsToAttach.Count}"
        );
    }

    // =========================================================
    // GRID POSITION
    // =========================================================

    private Vector3 FindGridWorldPosition(
        Vector2Int gridPosition
    )
    {
        GridManager gridManager =
            FindFirstObjectByType<GridManager>();

        if (gridManager == null)
            return transform.position;

        return gridManager.GridToWorld(
            gridPosition
        );
    }

    // =========================================================
    // DATA
    // =========================================================

    public List<BodyCell> GetCells()
    {
        return cells;
    }

    public int GetCellCount()
    {
        return cells.Count;
    }

    public bool Contains(
        BodyCell cell
    )
    {
        if (cell == null)
            return false;

        return cells.Contains(cell);
    }
}