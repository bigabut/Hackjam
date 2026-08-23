using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int width = 30;
    [SerializeField] private int height = 20;

    [Header("Cell")]
    [SerializeField] private float cellSize = 1f;

    [Header("Grid Visual")]
    [SerializeField] private GameObject gridLinePrefab;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Start()
    {
        GenerateGridVisual();
    }

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

    public bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < Width &&
               position.y >= 0 &&
               position.y < Height;
    }

    private void GenerateGridVisual()
    {
        if (gridLinePrefab == null)
            return;

        // Vertical lines
        for (int x = 0; x <= width; x++)
        {
            GameObject line = Instantiate(
                gridLinePrefab,
                transform
            );

            line.transform.localPosition = new Vector3(
                x * cellSize,
                (height * cellSize) / 2f,
                0f
            );

            line.transform.localScale = new Vector3(
                0.1f,
                height * cellSize,
                1f
            );
        }

        // Horizontal lines
        for (int y = 0; y <= height; y++)
        {
            GameObject line = Instantiate(
                gridLinePrefab,
                transform
            );

            line.transform.localPosition = new Vector3(
                (width * cellSize) / 2f,
                y * cellSize,
                0f
            );

            line.transform.localScale = new Vector3(
                width * cellSize,
                0.1f,
                1f
            );
        }
    }

    private void OnDrawGizmos()
    {
        if (cellSize <= 0f)
            return;

        Gizmos.color = Color.gray;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = transform.position +
                            new Vector3(x * cellSize, 0f, 0f);

            Vector3 end = transform.position +
                          new Vector3(x * cellSize, height * cellSize, 0f);

            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = transform.position +
                            new Vector3(0f, y * cellSize, 0f);

            Vector3 end = transform.position +
                          new Vector3(width * cellSize, y * cellSize, 0f);

            Gizmos.DrawLine(start, end);
        }
    }
}