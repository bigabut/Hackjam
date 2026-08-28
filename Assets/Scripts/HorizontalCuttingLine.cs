using UnityEngine;

public class HorizontalCuttingLine : CuttingLine
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

        float snappedY =
            Mathf.Round(
                localPosition.y / cellSize
            ) * cellSize;

        snappedY =
            Mathf.Clamp(
                snappedY,
                0f,
                gridManager.Height * cellSize
            );

        // X tetap mengikuti posisi cutter saat dilepas
        float snappedX =
            localPosition.x;

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
        return CutDirection.Horizontal;
    }
}