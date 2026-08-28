using UnityEngine;

public class VerticalCuttingLine : CuttingLine
{
    protected override Vector3 GetDragPosition(
        Vector3 mouseWorld)
    {
        return new Vector3(
            mouseWorld.x,
            mouseWorld.y,
            transform.position.z
        );
    }

    protected override void SnapToGrid()
    {
        if (gridManager == null)
            return;

        Vector3 localPosition =
            transform.position -
            gridManager.transform.position;

        float cellSize =
            gridManager.CellSize;

        float snappedX =
            Mathf.Round(
                localPosition.x / cellSize
            ) * cellSize;

        snappedX =
            Mathf.Clamp(
                snappedX,
                0f,
                gridManager.Width * cellSize
            );

        // Y tetap mengikuti posisi cutter saat dilepas
        float snappedY =
            localPosition.y;

        transform.position =
            gridManager.transform.position +
            new Vector3(
                snappedX,
                snappedY,
                transform.position.z
            );
    }

    protected override CutDirection GetCutDirection()
    {
        return CutDirection.Vertical;
    }
}