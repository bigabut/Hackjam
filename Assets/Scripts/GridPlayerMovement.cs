using UnityEngine;

public class GridPlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Hold Movement")]
    [SerializeField] private float holdDelay = 0.2f;
    [SerializeField] private float repeatRate = 0.1f;

    private Vector2Int currentGridPosition;
    private Vector2Int targetGridPosition;

    private bool isMoving;

    private Vector2Int heldDirection;
    private float holdTimer;

    private void Start()
    {
        currentGridPosition = gridManager.WorldToGrid(transform.position);
        targetGridPosition = currentGridPosition;

        transform.position = gridManager.GridToWorld(currentGridPosition);
    }

    private void Update()
    {
        HandleInput();
        MoveToTarget();
    }

    private void HandleInput()
    {
        Vector2Int inputDirection = GetInputDirection();

        // Kalau tidak ada tombol yang ditekan
        if (inputDirection == Vector2Int.zero)
        {
            heldDirection = Vector2Int.zero;
            holdTimer = 0f;
            return;
        }

        // Kalau baru menekan tombol / mengganti arah
        if (inputDirection != heldDirection)
        {
            heldDirection = inputDirection;
            holdTimer = 0f;

            // Langsung bergerak satu cell
            if (!isMoving)
            {
                TryMove(heldDirection);
            }

            return;
        }

        // Kalau masih hold tombol yang sama
        holdTimer += Time.deltaTime;

        // Tunggu sampai holdDelay
        if (holdTimer < holdDelay)
            return;

        // Setelah delay, ulangi movement berdasarkan repeatRate
        if (!isMoving)
        {
            TryMove(heldDirection);

            holdTimer -= repeatRate;
        }
    }

    private Vector2Int GetInputDirection()
    {
        if (Input.GetKey(KeyCode.W))
            return Vector2Int.up;

        if (Input.GetKey(KeyCode.S))
            return Vector2Int.down;

        if (Input.GetKey(KeyCode.A))
            return Vector2Int.left;

        if (Input.GetKey(KeyCode.D))
            return Vector2Int.right;

        return Vector2Int.zero;
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = currentGridPosition + direction;

        currentGridPosition = newPosition;
        targetGridPosition = currentGridPosition;

        isMoving = true;
    }

    private void MoveToTarget()
    {
        if (!isMoving)
            return;

        Vector3 targetWorldPosition =
            gridManager.GridToWorld(targetGridPosition);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.001f)
        {
            transform.position = targetWorldPosition;
            isMoving = false;
        }
    }

    public Vector2Int GetGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}