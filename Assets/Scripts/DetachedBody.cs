using System.Collections.Generic;
using UnityEngine;

public class DetachedBody : MonoBehaviour
{
    private List<BodyCell> cells =
        new List<BodyCell>();

    private bool isAttaching;

    // =========================================================
    // REGISTER
    // =========================================================

    public void RegisterCell(
        BodyCell cell)
    {
        if (cell == null)
            return;

        if (cells.Contains(cell))
            return;

        cells.Add(cell);
    }

    // =========================================================
    // ATTACH
    // =========================================================

    public void AttachToPlayer(
        Transform player)
    {
        if (player == null)
            return;

        if (isAttaching)
            return;

        if (cells.Count == 0)
        {
            Debug.LogWarning(
                $"{name}: Tidak ada cell."
            );

            return;
        }

        GridBodyMovement body =
            player.GetComponent<
                GridBodyMovement
            >();

        if (body == null)
        {
            Debug.LogError(
                $"{player.name}: " +
                $"GridBodyMovement tidak ditemukan."
            );

            return;
        }

        isAttaching = true;

        GridManager gridManager =
            FindFirstObjectByType<GridManager>();

        if (gridManager == null)
        {
            isAttaching = false;
            return;
        }

        // =====================================================
        // COPY
        // =====================================================

        List<BodyCell> group =
            new List<BodyCell>(
                cells
            );

        // =====================================================
        // REGISTER
        // =====================================================

        foreach (BodyCell cell in group)
        {
            if (cell == null)
                continue;

            body.RegisterAttachedCell(
                cell
            );
        }

        // =====================================================
        // PARENT
        // =====================================================

        foreach (BodyCell cell in group)
        {
            if (cell == null)
                continue;

            Vector2Int gridPosition =
                cell.GridPosition;

            cell.transform.SetParent(
                player,
                true
            );

            cell.transform.position =
                gridManager.GridToWorld(
                    gridPosition
                );

            cell.SetGridPosition(
                gridPosition
            );

            cell.HideAllSides();

            Collider2D[] colliders =
                cell.GetComponentsInChildren<
                    Collider2D
                >(true);

            foreach (Collider2D collider
                     in colliders)
            {
                collider.enabled = true;
            }
        }

        // =====================================================
        // REFRESH
        // =====================================================

        body.RefreshBodyCells();

        // =====================================================
        // SOUND
        // =====================================================

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                "Attach"
            );
        }

        // =====================================================
        // CLEAR + DESTROY
        // =====================================================

        cells.Clear();

        Destroy(gameObject);

        Debug.Log(
            $"DetachedBody {name} " +
            $"berhasil diattach. " +
            $"Cells: {group.Count}"
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
        BodyCell cell)
    {
        if (cell == null)
            return false;

        return cells.Contains(cell);
    }
}