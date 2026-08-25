using System.Collections.Generic;
using UnityEngine;

public class BodyCutter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridBodyMovement body;

    [Header("Detached Body")]
    [SerializeField] private GameObject detachedBodyPrefab;

    // =========================================================
    // CUT
    // =========================================================

    public void Cut(
        CuttingLine.CutDirection direction,
        Vector3 lineWorldPosition)
    {
        if (gridManager == null)
        {
            Debug.LogWarning(
                "BodyCutter: GridManager belum diisi."
            );
            return;
        }

        if (body == null)
        {
            Debug.LogWarning(
                "BodyCutter: Body belum diisi."
            );
            return;
        }

        List<BodyCell> cells = GetBodyCells();

        if (cells.Count <= 1)
            return;

        // =====================================================
        // 1. DAPATKAN POSISI GARIS
        // =====================================================

        Vector2Int linePosition =
            GetGridLinePosition(lineWorldPosition);

        // =====================================================
        // 2. CARI CONNECTION YANG TERPOTONG
        // =====================================================

        HashSet<Connection> cutConnections =
            FindCutConnections(
                cells,
                direction,
                linePosition
            );

        if (cutConnections.Count == 0)
        {
            Debug.Log(
                "Cut tidak mengenai connection body."
            );

            return;
        }

        Debug.Log(
            $"Cutting {cutConnections.Count} connection."
        );

        // =====================================================
        // 3. CARI SEMUA CELL YANG MASIH TERHUBUNG HEAD
        // =====================================================

        HashSet<BodyCell> connectedToHead =
            FindConnectedToHead(
                cells,
                cutConnections
            );

        // =====================================================
        // 4. CARI SEMUA GROUP YANG TERPISAH
        // =====================================================

        List<List<BodyCell>> detachedGroups =
            FindDetachedGroups(
                cells,
                connectedToHead,
                cutConnections
            );

        // =====================================================
        // 5. PINDAHKAN GROUP
        // =====================================================

        foreach (List<BodyCell> group in detachedGroups)
        {
            if (group == null || group.Count == 0)
                continue;

            CreateDetachedBody(group);
        }

        // =====================================================
        // 6. REFRESH PLAYER
        // =====================================================

        body.RefreshBodyCells();

        Debug.Log(
            $"Cut selesai. " +
            $"Detached groups: {detachedGroups.Count}"
        );
    }

    // =========================================================
    // GET PLAYER BODY CELLS
    // =========================================================

    private List<BodyCell> GetBodyCells()
    {
        List<BodyCell> result =
            new List<BodyCell>();

        BodyCell[] cells =
            body.GetComponentsInChildren<BodyCell>();

        foreach (BodyCell cell in cells)
        {
            if (cell != null)
                result.Add(cell);
        }

        return result;
    }

    // =========================================================
    // GRID LINE POSITION
    // =========================================================

    private Vector2Int GetGridLinePosition(
        Vector3 worldPosition)
    {
        Vector3 localPosition =
            worldPosition -
            gridManager.transform.position;

        return new Vector2Int(
            Mathf.RoundToInt(
                localPosition.x /
                gridManager.CellSize
            ),

            Mathf.RoundToInt(
                localPosition.y /
                gridManager.CellSize
            )
        );
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
        Vector2Int linePosition)
    {
        HashSet<Connection> result =
            new HashSet<Connection>();

        foreach (BodyCell cell in cells)
        {
            Vector2Int position =
                cell.GridPosition;

            Vector2Int neighbourPosition;

            // =================================================
            // HORIZONTAL CUT
            // =================================================

            if (direction ==
                CuttingLine.CutDirection.Horizontal)
            {
                /*
                 *      1 2
                 * ------------
                 *      3 4 5
                 *
                 * Cut:
                 * 1 - 3
                 * 2 - 4
                 *
                 * Tetap:
                 * 1 - 2
                 * 3 - 4
                 * 4 - 5
                 */

                if (position.y !=
                    linePosition.y - 1)
                {
                    continue;
                }

                neighbourPosition =
                    position +
                    Vector2Int.up;
            }

            // =================================================
            // VERTICAL CUT
            // =================================================

            else
            {
                /*
                 *   1 | 2
                 *   3 | 4
                 *
                 * Cut:
                 * 1 - 2
                 * 3 - 4
                 */

                if (position.x !=
                    linePosition.x - 1)
                {
                    continue;
                }

                neighbourPosition =
                    position +
                    Vector2Int.right;
            }

            BodyCell neighbour =
                FindCellAt(
                    cells,
                    neighbourPosition
                );

            if (neighbour == null)
                continue;

            result.Add(
                new Connection(
                    cell,
                    neighbour
                )
            );
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

        foreach (BodyCell cell in cells)
        {
            if (cell.IsHead)
            {
                head = cell;
                break;
            }
        }

        if (head == null)
        {
            Debug.LogWarning(
                "BodyCutter: Head tidak ditemukan."
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

                // Jangan melewati connection
                // yang terkena cutter.
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
            // Cell ini masih menjadi Player.
            if (connectedToHead.Contains(
                    startingCell))
            {
                continue;
            }

            // Sudah dimasukkan ke group.
            if (alreadyGrouped.Contains(
                    startingCell))
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

                group.Add(current);

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

                    // Connection dipotong.
                    if (IsConnectionCut(
                            current,
                            neighbour,
                            cutConnections))
                    {
                        continue;
                    }

                    // Jangan mengambil cell Player.
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

        // =====================================================
        // PINDAHKAN SEMUA CELL DALAM GROUP
        // =====================================================

        foreach (BodyCell cell in cells)
        {
            if (cell == null)
                continue;

            // Simpan grid position SEBELUM parent berubah.
            Vector2Int gridPosition =
                cell.GridPosition;

            // Pertahankan world position.
            cell.transform.SetParent(
                detachedObject.transform,
                true
            );

            // Pastikan kembali tepat di grid.
            cell.transform.position =
                gridManager.GridToWorld(
                    gridPosition
                );

            cell.SetGridPosition(
                gridPosition
            );

            detachedBody.RegisterCell(
                cell
            );
        }

        // =====================================================
        // PASTIKAN DETACHED BODY TIDAK IKUT PLAYER
        // =====================================================

        detachedObject.transform.SetParent(
            null,
            true
        );

        Debug.Log(
            $"Detached group dibuat: " +
            $"{cells.Count} cells."
        );

        // Debug isi group.
        string groupInfo = "";

        foreach (BodyCell cell in cells)
        {
            groupInfo +=
                $"{cell.name} " +
                $"[{cell.GridPosition}] | ";
        }

        Debug.Log(
            $"Group contents: {groupInfo}"
        );
    }

    // =========================================================
    // FIND CELL AT POSITION
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
    // CHECK CUT CONNECTION
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