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

    [Header("Drag")]
    [SerializeField] protected float dragHeight = 0f;

    [Header("Cut")]
    [SerializeField] private BodyCutter bodyCutter;

    protected Camera mainCamera;

    protected bool isDragging;

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        SnapToGrid();
    }

    protected virtual void Update()
    {
        if (WinManager.IsGameOver) return;
        
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

        mouseWorld.z = transform.position.z;

        // =====================================================
        // LEFT CLICK DOWN
        // =====================================================

        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(mouseWorld);
        }

        // =====================================================
        // DRAGGING
        // =====================================================

        if (isDragging &&
            Input.GetMouseButton(0))
        {
            Drag(mouseWorld);
        }

        // =====================================================
        // LEFT CLICK UP
        // =====================================================

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
                $"{name} tidak memiliki Collider2D."
            );

            return;
        }

        if (!collider.OverlapPoint(mouseWorld))
            return;

        isDragging = true;

        OnStartDrag();

        Debug.Log(
            $"Started dragging {name}"
        );
    }

    // =========================================================
    // DRAG
    // =========================================================

    private void Drag(
        Vector3 mouseWorld)
    {
        Vector3 targetPosition =
            GetDragPosition(mouseWorld);

        transform.position =
            targetPosition;
    }

    // =========================================================
    // STOP DRAG
    // =========================================================

    private void StopDrag()
    {
        isDragging = false;

        // Snap ke grid dulu
        SnapToGrid();

        OnStopDrag();

        // =====================================================
        // CUT
        // =====================================================

        if (bodyCutter == null)
        {
            Debug.LogWarning(
                $"{name} tidak memiliki BodyCutter."
            );

            return;
        }

        bodyCutter.Cut(
            GetCutDirection(),
            transform.position
        );

        Debug.Log(
            $"Cut executed by {name} " +
            $"at {transform.position}"
        );
    }

    // =========================================================
    // ABSTRACT
    // =========================================================

    protected abstract Vector3 GetDragPosition(
        Vector3 mouseWorld
    );

    protected abstract void SnapToGrid();

    // =========================================================
    // CUT DIRECTION
    // =========================================================

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