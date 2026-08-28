using System.Collections.Generic;
using UnityEngine;

public class BodyCutter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridBodyMovement body;

    [Header("Detached Body")]
    [SerializeField] private GameObject detachedBodyPrefab;

    [Header("Cut Detection")]
    [SerializeField] private float lineTolerance = 0.15f;

    // =========================================================
    // CUT
    // =========================================================

    public void Cut(
        CuttingLine.CutDirection direction,
        Vector3 lineWorldPosition)
    {
        // -----------------------------------------------------
        // VALIDASI
        // -----------------------------------------------------

        if (gridManager == null)
        {
            Debug.LogError(
                "BodyCutter: GridManager belum diassign."
            );

            return;
        }

        if (body == null)
        {
            Debug.LogError(
                "BodyCutter: GridBodyMovement belum diassign."
            );

            return;
        }

        if (body.IsMoving())
        {
            Debug.Log(
                "Tidak bisa cutting saat body sedang bergerak."
            );

            return;
        }

        // -----------------------------------------------------
        // GET BODY
        // -----------------------------------------------------

        List<BodyCell> cells =
            GetBodyCells();

        if (cells.Count <= 1)
        {
            Debug.Log(
                "Body terlalu kecil untuk dipotong."
            );

            return;
        }

        // Pastikan posisi grid setiap cell benar
        RefreshCellPositions(cells);

        // -----------------------------------------------------
        // FIND CONNECTION YANG DILALUI CUTTER
        // -----------------------------------------------------

        HashSet<Connection> cutConnections =
            FindCutConnections(
                cells,
                direction,
                lineWorldPosition
            );

        if (cutConnections.Count == 0)
        {
            Debug.Log(
                "Cutter tidak memotong connection BodyCell."
            );

            return;
        }

        Debug.Log(
            $"Cutter memutus {cutConnections.Count} connection."
        );

        // -----------------------------------------------------
        // COUNTER
        // -----------------------------------------------------

        if (GameCounter.Instance != null)
        {
            if (!GameCounter.Instance.CanCut())
            {
                Debug.Log(
                    "Cut counter sudah habis."
                );

                return;
            }

            GameCounter.Instance.AddCut();
        }

        // -----------------------------------------------------
        // CARI CELL YANG MASIH TERHUBUNG KE HEAD
        // -----------------------------------------------------

        HashSet<BodyCell> connectedToHead =
            FindConnectedToHead(
                cells,
                cutConnections
            );

        // -----------------------------------------------------
        // CARI GROUP YANG DETACHED
        // -----------------------------------------------------

        List<List<BodyCell>> detachedGroups =
            FindDetachedGroups(
                cells,
                connectedToHead,
                cutConnections
            );

        // -----------------------------------------------------
        // CREATE DETACHED
        // -----------------------------------------------------

        foreach (List<BodyCell> group
                 in detachedGroups)
        {
            if (group == null ||
                group.Count == 0)
            {
                continue;
            }

            // SAFETY:
            // Jangan pernah detach group yang berisi Head.
            bool containsHead = false;

            foreach (BodyCell cell in group)
            {
                if (cell != null &&
                    cell.IsHead)
                {
                    containsHead = true;
                    break;
                }
            }

            if (containsHead)
            {
                Debug.LogWarning(
                    "Group mengandung Head. " +
                    "Group tidak akan detached."
                );

                continue;
            }

            CreateDetachedBody(group);
        }

        // -----------------------------------------------------
        // REFRESH PLAYER
        // -----------------------------------------------------

        body.RefreshBodyCells();

        Debug.Log(
            $"Cut selesai. " +
            $"Detached groups = {detachedGroups.Count}"
        );
    }

    // =========================================================
    // GET BODY CELLS
    // =========================================================

    private List<BodyCell> GetBodyCells()
    {
        List<BodyCell> result =
            new List<BodyCell>();

        BodyCell[] cells =
            body.GetComponentsInChildren<BodyCell>(
                true
            );

        foreach (BodyCell cell in cells)
        {
            if (cell != null)
            {
                result.Add(cell);
            }
        }

        return result;
    }

    // =========================================================
    // REFRESH GRID POSITION
    // =========================================================

    private void RefreshCellPositions(
        List<BodyCell> cells)
    {
        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            Vector2Int gridPosition =
                gridManager.WorldToGrid(
                    cell.transform.position
                );

            cell.SetGridPosition(
                gridPosition
            );
        }
    }

    // =========================================================
    // CONNECTION
    // =========================================================

    private struct Connection
    {
        public BodyCell a;
        public BodyCell b;

        public Connection(
            BodyCell a,
            BodyCell b)
        {
            this.a = a;
            this.b = b;
        }

        public bool Matches(
            BodyCell first,
            BodyCell second)
        {
            return
                (a == first && b == second) ||
                (a == second && b == first);
        }
    }

    // =========================================================
    // FIND CUT CONNECTIONS
    // =========================================================

    private HashSet<Connection> FindCutConnections(
        List<BodyCell> cells,
        CuttingLine.CutDirection direction,
        Vector3 lineWorldPosition)
    {
        HashSet<Connection> result =
            new HashSet<Connection>();

        Vector3 gridOrigin =
            gridManager.transform.position;

        float cellSize =
            gridManager.CellSize;

        Vector3 localLine =
            lineWorldPosition -
            gridOrigin;

        // =====================================================
        // HORIZONTAL
        // =====================================================

        if (direction ==
            CuttingLine.CutDirection.Horizontal)
        {
            float boundaryFloat =
                localLine.y / cellSize;

            int boundary =
                Mathf.RoundToInt(
                    boundaryFloat
                );

            if (boundary < 0 ||
                boundary > gridManager.Height)
            {
                return result;
            }

            foreach (BodyCell cell in cells)
            {
                if (cell == null)
                    continue;

                Vector2Int position =
                    cell.GridPosition;

                // Cell harus berada tepat di bawah garis
                if (position.y != boundary - 1)
                    continue;

                Vector2Int abovePosition =
                    position + Vector2Int.up;

                BodyCell above =
                    FindCellAt(
                        cells,
                        abovePosition
                    );

                if (above == null)
                    continue;

                // Pastikan benar-benar adjacent
                if (above.GridPosition.y !=
                    position.y + 1)
                {
                    continue;
                }

                result.Add(
                    new Connection(
                        cell,
                        above
                    )
                );
            }
        }

        // =====================================================
        // VERTICAL
        // =====================================================

        else
        {
            float boundaryFloat =
                localLine.x / cellSize;

            int boundary =
                Mathf.RoundToInt(
                    boundaryFloat
                );

            if (boundary < 0 ||
                boundary > gridManager.Width)
            {
                return result;
            }

            foreach (BodyCell cell in cells)
            {
                if (cell == null)
                    continue;

                Vector2Int position =
                    cell.GridPosition;

                // Cell harus berada tepat di kiri garis
                if (position.x != boundary - 1)
                    continue;

                Vector2Int rightPosition =
                    position + Vector2Int.right;

                BodyCell right =
                    FindCellAt(
                        cells,
                        rightPosition
                    );

                if (right == null)
                    continue;

                if (right.GridPosition.x !=
                    position.x + 1)
                {
                    continue;
                }

                result.Add(
                    new Connection(
                        cell,
                        right
                    )
                );
            }
        }

        return result;
    }

    // =========================================================
    // FIND CONNECTED TO HEAD
    // =========================================================

    private HashSet<BodyCell> FindConnectedToHead(
        List<BodyCell> cells,
        HashSet<Connection> cutConnections)
    {
        HashSet<BodyCell> visited =
            new HashSet<BodyCell>();

        BodyCell head = null;

        // Cari Head
        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            if (cell.IsHead)
            {
                head = cell;
                break;
            }
        }

        if (head == null)
        {
            Debug.LogError(
                "BodyCutter: Head tidak ditemukan " +
                "di dalam BodyCell player."
            );

            return visited;
        }

        Queue<BodyCell> queue =
            new Queue<BodyCell>();

        queue.Enqueue(head);
        visited.Add(head);

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            BodyCell current =
                queue.Dequeue();

            foreach (Vector2Int direction
                     in directions)
            {
                Vector2Int neighbourPosition =
                    current.GridPosition +
                    direction;

                BodyCell neighbour =
                    FindCellAt(
                        cells,
                        neighbourPosition
                    );

                if (neighbour == null)
                    continue;

                // Connection diputus oleh cutter
                if (IsConnectionCut(
                        current,
                        neighbour,
                        cutConnections))
                {
                    continue;
                }

                if (visited.Contains(neighbour))
                    continue;

                visited.Add(neighbour);
                queue.Enqueue(neighbour);
            }
        }

        return visited;
    }

    // =========================================================
    // FIND DETACHED GROUPS
    // =========================================================

    private List<List<BodyCell>> FindDetachedGroups(
        List<BodyCell> allCells,
        HashSet<BodyCell> connectedToHead,
        HashSet<Connection> cutConnections)
    {
        List<List<BodyCell>> groups =
            new List<List<BodyCell>>();

        HashSet<BodyCell> alreadyGrouped =
            new HashSet<BodyCell>();

        foreach (BodyCell startingCell in allCells)
        {
            if (startingCell == null)
                continue;

            // Cell yang masih terhubung ke Head
            // bukan detached.
            if (connectedToHead.Contains(
                    startingCell))
            {
                continue;
            }

            if (alreadyGrouped.Contains(
                    startingCell))
            {
                continue;
            }

            // =================================================
            // HEAD TIDAK BOLEH MASUK DETACHED
            // =================================================

            if (startingCell.IsHead)
            {
                continue;
            }

            List<BodyCell> group =
                new List<BodyCell>();

            Queue<BodyCell> queue =
                new Queue<BodyCell>();

            queue.Enqueue(startingCell);
            alreadyGrouped.Add(startingCell);

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            while (queue.Count > 0)
            {
                BodyCell current =
                    queue.Dequeue();

                // Head tidak boleh dimasukkan
                if (!current.IsHead)
                {
                    group.Add(current);
                }

                foreach (Vector2Int direction
                         in directions)
                {
                    Vector2Int neighbourPosition =
                        current.GridPosition +
                        direction;

                    BodyCell neighbour =
                        FindCellAt(
                            allCells,
                            neighbourPosition
                        );

                    if (neighbour == null)
                        continue;

                    if (neighbour.IsHead)
                        continue;

                    if (IsConnectionCut(
                            current,
                            neighbour,
                            cutConnections))
                    {
                        continue;
                    }

                    if (connectedToHead.Contains(
                            neighbour))
                    {
                        continue;
                    }

                    if (alreadyGrouped.Contains(
                            neighbour))
                    {
                        continue;
                    }

                    alreadyGrouped.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }

            if (group.Count > 0)
            {
                groups.Add(group);
            }
        }

        return groups;
    }

    // =========================================================
    // CREATE DETACHED BODY
    // =========================================================

    private void CreateDetachedBody(
        List<BodyCell> cells)
    {
        if (cells == null ||
            cells.Count == 0)
        {
            return;
        }

        GameObject detachedObject;

        if (detachedBodyPrefab != null)
        {
            detachedObject =
                Instantiate(
                    detachedBodyPrefab
                );
        }
        else
        {
            detachedObject =
                new GameObject(
                    "DetachedBody"
                );
        }

        DetachedBody detachedBody =
            detachedObject.GetComponent<DetachedBody>();

        if (detachedBody == null)
        {
            detachedBody =
                detachedObject.AddComponent<DetachedBody>();
        }

        // -----------------------------------------------------
        // MOVE CELLS
        // -----------------------------------------------------

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            Vector2Int gridPosition =
                cell.GridPosition;

            Vector3 worldPosition =
                gridManager.GridToWorld(
                    gridPosition
                );

            cell.transform.SetParent(
                detachedObject.transform,
                true
            );

            cell.transform.position =
                worldPosition;

            cell.SetGridPosition(
                gridPosition
            );

            cell.HideAllSides();

            // Collider tetap aktif
            Collider2D[] colliders =
                cell.GetComponentsInChildren<Collider2D>(
                    true
                );

            foreach (Collider2D collider in colliders)
            {
                collider.enabled = true;
            }

            detachedBody.RegisterCell(cell);
        }

        detachedObject.transform.SetParent(
            null,
            true
        );

        Debug.Log(
            $"Detached group dibuat: " +
            $"{cells.Count} cells."
        );
    }

    // =========================================================
    // FIND CELL
    // =========================================================

    private BodyCell FindCellAt(
        List<BodyCell> cells,
        Vector2Int position)
    {
        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            if (cell.GridPosition ==
                position)
            {
                return cell;
            }
        }

        return null;
    }

    // =========================================================
    // CHECK CONNECTION CUT
    // =========================================================

    private bool IsConnectionCut(
        BodyCell a,
        BodyCell b,
        HashSet<Connection> cutConnections)
    {
        foreach (Connection connection
                 in cutConnections)
        {
            if (connection.Matches(a, b))
            {
                return true;
            }
        }

        return false;
    }
}