using UnityEngine;

public class BodyAttachment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridBodyMovement body;

    private void Update()
    {
        RefreshAttachmentUI();
    }

    private void RefreshAttachmentUI()
    {
        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        // Semua cell yang sudah menjadi bagian Player
        // boleh melakukan attachment detection.
        foreach (BodyCell cell in allCells)
        {
            if (!IsAttachedCell(cell))
                continue;

            CheckCellSides(cell);
        }
    }

    private bool IsAttachedCell(BodyCell cell)
    {
        if (body == null)
            return false;

        return cell.transform.IsChildOf(
            body.transform
        );
    }

    private void CheckCellSides(BodyCell cell)
    {
        // Reset UI
        cell.HideAllSides();

        Vector2Int cellPosition =
            gridManager.WorldToGrid(
                cell.transform.position
            );

        CheckDirection(
            cell,
            cellPosition,
            Vector2Int.up
        );

        CheckDirection(
            cell,
            cellPosition,
            Vector2Int.down
        );

        CheckDirection(
            cell,
            cellPosition,
            Vector2Int.left
        );

        CheckDirection(
            cell,
            cellPosition,
            Vector2Int.right
        );
    }

    private void CheckDirection(
        BodyCell attachedCell,
        Vector2Int cellPosition,
        Vector2Int direction
    )
    {
        Vector2Int targetPosition =
            cellPosition + direction;

        BodyCell targetCell =
            FindDetachedBodyCell(
                targetPosition
            );

        if (targetCell == null)
            return;

        Debug.Log(
            $"[{attachedCell.name}] menemukan " +
            $"[{targetCell.name}] di {direction}"
        );

        attachedCell.SetSideAvailable(
            direction,
            true,
            targetCell
        );
    }

    private BodyCell FindDetachedBodyCell(
        Vector2Int targetPosition
    )
    {
        BodyCell[] allCells =
            FindObjectsByType<BodyCell>(
                FindObjectsSortMode.None
            );

        foreach (BodyCell cell in allCells)
        {
            // Head tidak pernah menjadi target
            if (cell.IsHead)
                continue;

            // Sudah menjadi bagian tubuh
            if (IsAttachedCell(cell))
                continue;

            Vector2Int cellPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            if (cellPosition == targetPosition)
            {
                return cell;
            }
        }

        return null;
    }
}