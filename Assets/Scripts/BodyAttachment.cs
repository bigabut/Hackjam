using UnityEngine;

public class BodyAttachment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridBodyMovement body;



private void Update()
    {
        if (gridManager == null ||
            body == null)
        {
            return;
        }

        RefreshAttachmentUI();
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void RefreshAttachmentUI()
    {
        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        // Cek semua BodyCell yang sudah menjadi
        // bagian dari Player.
        foreach (BodyCell cell in allCells)
        {
            if (cell == null)
                continue;

            if (!IsAttachedCell(cell))
                continue;

            CheckCellSides(
                cell
            );
        }
    }

    // =========================================================
    // ATTACHED?
    // =========================================================

    private bool IsAttachedCell(
        BodyCell cell)
    {
        if (cell == null)
            return false;

        return cell.transform.IsChildOf(
            body.transform
        );
    }

    // =========================================================
    // CHECK SIDES
    // =========================================================

    private void CheckCellSides(
        BodyCell cell)
    {
        if (cell == null)
            return;

        // Reset semua sisi terlebih dahulu.
        cell.HideAllSides();

        Vector2Int position =
            cell.GridPosition;

        CheckDirection(
            cell,
            position,
            Vector2Int.up
        );

        CheckDirection(
            cell,
            position,
            Vector2Int.down
        );

        CheckDirection(
            cell,
            position,
            Vector2Int.left
        );

        CheckDirection(
            cell,
            position,
            Vector2Int.right
        );
    }

    // =========================================================
    // CHECK DIRECTION
    // =========================================================

    private void CheckDirection(
        BodyCell attachedCell,
        Vector2Int position,
        Vector2Int direction)
    {
        if (attachedCell == null)
            return;

        Vector2Int targetPosition =
            position + direction;

        BodyCell target =
            FindDetachedBodyCell(
                targetPosition
            );

        if (target == null)
            return;

        attachedCell.SetSideAvailable(
            direction,
            true,
            target
        );
    }

    // =========================================================
    // FIND DETACHED CELL
    // =========================================================

    private BodyCell FindDetachedBodyCell(
        Vector2Int targetPosition)
    {
        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        foreach (BodyCell cell in allCells)
        {
            if (cell == null)
                continue;

            // Head tidak boleh menjadi target.
            if (cell.IsHead)
                continue;

            // Cell yang sudah menjadi bagian Player
            // bukan detached cell.
            if (IsAttachedCell(cell))
                continue;

            if (cell.GridPosition ==
                targetPosition)
            {
                return cell;
            }
        }

        return null;
    }


}
