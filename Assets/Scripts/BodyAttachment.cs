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
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);

        // 1. Matikan semua UI ceklis di SEMUA blok (reset)
        foreach (BodyCell cell in allCells)
        {
            cell.HideAllSides();
        }

        // 2. Cek deteksi dari blok yang menempel ke Player
        foreach (BodyCell cell in allCells)
        {
            if (!IsAttachedCell(cell)) continue;
            CheckCellSides(cell);
        }
    }

    private bool IsAttachedCell(BodyCell cell)
    {
        if (body == null) return false;
        return cell.transform.IsChildOf(body.transform);
    }

    private void CheckCellSides(BodyCell cell)
    {
        Vector2Int cellPosition = cell.GridPosition;

        CheckDirection(cell, cellPosition, Vector2Int.up);
        CheckDirection(cell, cellPosition, Vector2Int.down);
        CheckDirection(cell, cellPosition, Vector2Int.left);
        CheckDirection(cell, cellPosition, Vector2Int.right);
    }

    private void CheckDirection(BodyCell attachedCell, Vector2Int cellPosition, Vector2Int direction)
    {
        Vector2Int targetPosition = cellPosition + direction;
        BodyCell targetCell = FindDetachedBodyCell(targetPosition);

        if (targetCell == null) return;

        // Aktifkan ceklis di blok Player
        attachedCell.SetSideAvailable(direction, true, targetCell);

        // Aktifkan juga ceklis di blok sasaran yang lepas biar kamu bisa klik dari sana!
        targetCell.SetSideAvailable(-direction, true, attachedCell);
    }

    private BodyCell FindDetachedBodyCell(Vector2Int targetPosition)
    {
        BodyCell[] allCells = FindObjectsByType<BodyCell>(FindObjectsSortMode.None);
        foreach (BodyCell cell in allCells)
        {
            if (cell.IsHead) continue;
            if (IsAttachedCell(cell)) continue;
            
            if (cell.GridPosition == targetPosition)
            {
                return cell;
            }
        }
        return null;
    }
}