using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    [Min(1)]
    [SerializeField] private int width = 30;

    [Min(1)]
    [SerializeField] private int height = 20;

    [Header("Cell")]
    [Min(0.01f)]
    [SerializeField] private float cellSize = 1f;

    [Header("Grid Visual")]
    [SerializeField] private GameObject gridLinePrefab;

    [SerializeField] private Transform gridVisualParent;

    // =========================================================
    // PUBLIC
    // =========================================================

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    public Vector2 GridSize =>
        new Vector2(
            width,
            height
        );

    public Vector2 WorldSize =>
        new Vector2(
            width * cellSize,
            height * cellSize
        );

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        GenerateGridVisual();
    }

    // =========================================================
    // GRID TO WORLD
    // =========================================================

    public Vector3 GridToWorld(
        Vector2Int gridPosition)
    {
        return transform.position +
               new Vector3(
                   (gridPosition.x + 0.5f) * cellSize,
                   (gridPosition.y + 0.5f) * cellSize,
                   0f
               );
    }

    // =========================================================
    // WORLD TO GRID
    // =========================================================

    public Vector2Int WorldToGrid(
        Vector3 worldPosition)
    {
        Vector3 localPosition =
            worldPosition -
            transform.position;

        return new Vector2Int(
            Mathf.FloorToInt(
                localPosition.x / cellSize
            ),

            Mathf.FloorToInt(
                localPosition.y / cellSize
            )
        );
    }

    // =========================================================
    // GRID BOUNDARY TO WORLD
    // =========================================================

    public Vector3 GridBoundaryToWorld(
        int x,
        int y)
    {
        return transform.position +
               new Vector3(
                   x * cellSize,
                   y * cellSize,
                   0f
               );
    }

    // =========================================================
    // INSIDE GRID
    // =========================================================

    public bool IsInsideGrid(
        Vector2Int position)
    {
        return
            position.x >= 0 &&
            position.x < width &&
            position.y >= 0 &&
            position.y < height;
    }

    // =========================================================
    // CLAMP GRID POSITION
    // =========================================================

    public Vector2Int ClampGridPosition(
        Vector2Int position)
    {
        return new Vector2Int(
            Mathf.Clamp(
                position.x,
                0,
                width - 1
            ),

            Mathf.Clamp(
                position.y,
                0,
                height - 1
            )
        );
    }

    // =========================================================
    // GET GRID BOUNDS
    // =========================================================

    public Bounds GetGridBounds()
    {
        Vector3 center =
            transform.position +
            new Vector3(
                width * cellSize * 0.5f,
                height * cellSize * 0.5f,
                0f
            );

        Vector3 size =
            new Vector3(
                width * cellSize,
                height * cellSize,
                0.1f
            );

        return new Bounds(
            center,
            size
        );
    }

    // =========================================================
    // CHECK WORLD POSITION
    // =========================================================

    public bool IsWorldInsideGrid(
        Vector3 worldPosition)
    {
        return IsInsideGrid(
            WorldToGrid(worldPosition)
        );
    }

    // =========================================================
    // GENERATE GRID VISUAL
    // =========================================================

    private void GenerateGridVisual()
    {
        if (gridLinePrefab == null)
            return;

        ClearGridVisual();

        Transform parent =
            gridVisualParent != null
                ? gridVisualParent
                : transform;

        // =====================================================
        // VERTICAL
        // =====================================================

        for (int x = 0; x <= width; x++)
        {
            GameObject line =
                Instantiate(
                    gridLinePrefab,
                    parent
                );

            line.transform.localPosition =
                new Vector3(
                    x * cellSize,
                    height * cellSize * 0.5f,
                    0f
                );

            line.transform.localScale =
                new Vector3(
                    0.1f,
                    height * cellSize,
                    1f
                );
        }

        // =====================================================
        // HORIZONTAL
        // =====================================================

        for (int y = 0; y <= height; y++)
        {
            GameObject line =
                Instantiate(
                    gridLinePrefab,
                    parent
                );

            line.transform.localPosition =
                new Vector3(
                    width * cellSize * 0.5f,
                    y * cellSize,
                    0f
                );

            line.transform.localScale =
                new Vector3(
                    width * cellSize,
                    0.1f,
                    1f
                );
        }
    }

    // =========================================================
    // CLEAR GRID VISUAL
    // =========================================================

    private void ClearGridVisual()
    {
        Transform parent =
            gridVisualParent != null
                ? gridVisualParent
                : transform;

        for (int i = parent.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                parent.GetChild(i);

            if (child == null)
                continue;

            // Jangan hapus object selain grid visual
            if (child.gameObject == gameObject)
                continue;

            if (Application.isPlaying)
            {
                Destroy(
                    child.gameObject
                );
            }
            else
            {
                DestroyImmediate(
                    child.gameObject
                );
            }
        }
    }

    // =========================================================
    // REFRESH VISUAL
    // =========================================================

    public void RefreshGridVisual()
    {
        GenerateGridVisual();
    }

    // =========================================================
    // VALIDATE
    // =========================================================

    private void OnValidate()
    {
        width =
            Mathf.Max(
                1,
                width
            );

        height =
            Mathf.Max(
                1,
                height
            );

        cellSize =
            Mathf.Max(
                0.01f,
                cellSize
            );
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        if (cellSize <= 0f)
            return;

        Gizmos.color =
            Color.gray;

        // =====================================================
        // VERTICAL
        // =====================================================

        for (int x = 0; x <= width; x++)
        {
            Vector3 start =
                transform.position +
                new Vector3(
                    x * cellSize,
                    0f,
                    0f
                );

            Vector3 end =
                transform.position +
                new Vector3(
                    x * cellSize,
                    height * cellSize,
                    0f
                );

            Gizmos.DrawLine(
                start,
                end
            );
        }

        // =====================================================
        // HORIZONTAL
        // =====================================================

        for (int y = 0; y <= height; y++)
        {
            Vector3 start =
                transform.position +
                new Vector3(
                    0f,
                    y * cellSize,
                    0f
                );

            Vector3 end =
                transform.position +
                new Vector3(
                    width * cellSize,
                    y * cellSize,
                    0f
                );

            Gizmos.DrawLine(
                start,
                end
            );
        }

        // =====================================================
        // BORDER
        // =====================================================

        Gizmos.DrawWireCube(
            GetGridBounds().center,
            GetGridBounds().size
        );
    }
}