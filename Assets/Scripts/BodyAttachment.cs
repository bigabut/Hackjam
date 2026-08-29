using UnityEngine;

public class BodyAttachment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridBodyMovement body;

    private void Update()
    {
        if (gridManager == null || body == null) return;
        RefreshAttachmentUI();
    }

    private void RefreshAttachmentUI()
    {
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);

        foreach (BodyCell cell in allCells)
        {
            if (cell == null) continue;
            if (!IsAttachedCell(cell)) continue;
            
            CheckCellSides(cell);
        }
    }

    private bool IsAttachedCell(BodyCell cell)
    {
        if (cell == null) return false;
        return cell.transform.IsChildOf(body.transform);
    }

    private void CheckCellSides(BodyCell cell)
    {
        if (cell == null) return;

        Vector2Int position = cell.GridPosition;

        CheckDirection(cell, position, Vector2Int.up);
        CheckDirection(cell, position, Vector2Int.down);
        CheckDirection(cell, position, Vector2Int.left);
        CheckDirection(cell, position, Vector2Int.right);
    }

    private void CheckDirection(BodyCell attachedCell, Vector2Int position, Vector2Int direction)
    {
        if (attachedCell == null) return;

        Vector2Int targetPosition = position + direction;
        BodyCell target = FindDetachedBodyCell(targetPosition);

        // KUNCI PERBAIKAN 3: Tentukan status tombol menyala/mati secara akurat di sini
        if (target != null)
        {
            attachedCell.SetSideAvailable(direction, true, target);
        }
        else
        {
            attachedCell.SetSideAvailable(direction, false, null);
        }
    }

    private BodyCell FindDetachedBodyCell(Vector2Int targetPosition)
    {
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);

        foreach (BodyCell cell in allCells)
        {
            if (cell == null) continue;
            if (cell.IsHead) continue;
            if (IsAttachedCell(cell)) continue;

            if (cell.GridPosition == targetPosition) return cell;
        }

        return null;
    }
}