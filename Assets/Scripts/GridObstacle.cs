using UnityEngine;

public class GridObstacle : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GridManager gridManager;

    private Vector2Int gridPosition;

    public Vector2Int GridPosition => gridPosition;

    private void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError(
                $"{name}: GridManager belum di-assign!"
            );

            return;
        }

        SnapToGrid();
    }

    public void SnapToGrid()
    {
        gridPosition =
            gridManager.WorldToGrid(
                transform.position
            );

        transform.position =
            gridManager.GridToWorld(
                gridPosition
            );
    }
}

