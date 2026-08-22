using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    public float CellSize => cellSize;

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return transform.position + new Vector3(
            (gridPosition.x + 0.5f) * cellSize,
            (gridPosition.y + 0.5f) * cellSize,
            0f
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;

        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / cellSize),
            Mathf.FloorToInt(localPosition.y / cellSize)
        );
    }

    public Vector3 GetCellCenter(Vector2Int gridPosition)
    {
        return GridToWorld(gridPosition);
    }

    private void OnDrawGizmos()
    {
        if (cellSize <= 0f)
            return;

        Gizmos.color = Color.gray;

        int gridSize = 10;

        // Vertical lines
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = transform.position +
                            new Vector3(x * cellSize, 0f, 0f);

            Vector3 end = transform.position +
                          new Vector3(x * cellSize, gridSize * cellSize, 0f);

            Gizmos.DrawLine(start, end);
        }

        // Horizontal lines
        for (int y = 0; y <= gridSize; y++)
        {
            Vector3 start = transform.position +
                            new Vector3(0f, y * cellSize, 0f);

            Vector3 end = transform.position +
                          new Vector3(gridSize * cellSize, y * cellSize, 0f);

            Gizmos.DrawLine(start, end);
        }
    }
}