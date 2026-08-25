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
        Vector3 localPosition =
            transform.position -
            gridManager.transform.position;

        float cellSize =
            gridManager.CellSize;

        float snappedX =
            Mathf.Round(
                localPosition.x / cellSize
            ) * cellSize;

        float snappedY =
            Mathf.Round(
                localPosition.y / cellSize
            ) * cellSize;

        snappedX = Mathf.Clamp(
            snappedX,
            0f,
            gridManager.Width * cellSize
        );

        snappedY = Mathf.Clamp(
            snappedY,
            0f,
            gridManager.Height * cellSize
        );

        transform.position =
            gridManager.transform.position +
            new Vector3(
                snappedX,
                snappedY,
                0f
            );
    }

    protected override CutDirection GetCutDirection()
    {
        return CutDirection.Vertical;
    }
}