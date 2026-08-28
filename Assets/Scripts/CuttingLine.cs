using UnityEngine;

public abstract class CuttingLine : MonoBehaviour
{
    public enum CutDirection
    {
        Horizontal,
        Vertical
    }

    [Header("Grid")]
    [SerializeField] protected GridManager gridManager;

    [Header("Cut")]
    [SerializeField] private BodyCutter bodyCutter;

    protected Camera mainCamera;
    protected bool isDragging;

    // =========================================================
    // START
    // =========================================================

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        if (gridManager == null)
        {
            gridManager =
                FindFirstObjectByType<GridManager>();
        }

        SnapToGrid();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    protected virtual void Update()
    {
        if (WinManager.IsGameOver)
            return;

        HandleDrag();
    }

    // =========================================================
    // DRAG
    // =========================================================

    private void HandleDrag()
    {
        if (mainCamera == null)
            return;

        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        mouseWorld.z =
            transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(mouseWorld);
        }

        if (isDragging &&
            Input.GetMouseButton(0))
        {
            Drag(mouseWorld);
        }

        if (isDragging &&
            Input.GetMouseButtonUp(0))
        {
            StopDrag();
        }
    }

    // =========================================================
    // START DRAG
    // =========================================================

    private void TryStartDrag(
        Vector3 mouseWorld)
    {
        Collider2D collider =
            GetComponent<Collider2D>();

        if (collider == null)
        {
            Debug.LogWarning(
                $"{name}: Collider2D tidak ditemukan."
            );

            return;
        }

        if (!collider.OverlapPoint(mouseWorld))
            return;

        isDragging = true;

        OnStartDrag();
    }

    // =========================================================
    // DRAG
    // =========================================================

    private void Drag(
        Vector3 mouseWorld)
    {
        // Cutter sekarang bisa bergerak bebas
        transform.position =
            new Vector3(
                mouseWorld.x,
                mouseWorld.y,
                transform.position.z
            );
    }

    // =========================================================
    // STOP DRAG
    // =========================================================

    private void StopDrag()
    {
        isDragging = false;

        // Snap ke boundary grid terdekat
        SnapToGrid();

        OnStopDrag();

        if (bodyCutter == null)
        {
            Debug.LogWarning(
                $"{name}: BodyCutter belum diassign."
            );

            return;
        }

        bodyCutter.Cut(
            GetCutDirection(),
            transform.position
        );
    }

    // =========================================================
    // ABSTRACT
    // =========================================================

    protected abstract Vector3 GetDragPosition(
        Vector3 mouseWorld
    );

    protected abstract void SnapToGrid();

    protected abstract CutDirection GetCutDirection();

    // =========================================================
    // EVENTS
    // =========================================================

    protected virtual void OnStartDrag()
    {
    }

    protected virtual void OnStopDrag()
    {
    }

    // =========================================================
    // STATE
    // =========================================================

    public bool IsDragging()
    {
        return isDragging;
    }
}