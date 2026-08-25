using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BodyCellSide : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image sideImage;
    [SerializeField] private Button button;

    [Header("Alpha")]
    [SerializeField] private float hiddenAlpha = 0f;
    [SerializeField] private float availableAlpha = 1f;
    [SerializeField] private float hoverAlpha = 1f;

    private BodyCell owner;
    private Vector2Int direction;
    private BodyCell targetCell;

    private bool isAvailable;
    private bool isHovered;

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        BodyCell owner,
        Vector2Int direction)
    {
        this.owner = owner;
        this.direction = direction;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        SetAlpha(hiddenAlpha);

        Debug.Log(
            $"BodyCellSide Setup: {owner.name} / {direction}"
        );
    }

    // =========================================================
    // AVAILABLE
    // =========================================================

    public void SetAvailable(
        bool available,
        BodyCell target = null)
    {
        isAvailable = available;
        targetCell = target;

        Debug.Log(
            $"Side {direction} " +
            $"Available = {available} " +
            $"Target = {(target != null ? target.name : "NULL")}"
        );

        RefreshVisual();
    }

    // =========================================================
    // VISUAL
    // =========================================================

    private void RefreshVisual()
    {
        if (!isAvailable)
        {
            SetAlpha(hiddenAlpha);

            if (button != null)
                button.interactable = false;

            return;
        }

        if (button != null)
            button.interactable = true;

        if (isHovered)
        {
            SetAlpha(hoverAlpha);
        }
        else
        {
            SetAlpha(availableAlpha);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (sideImage == null)
        {
            Debug.LogWarning(
                $"{name}: Side Image belum diassign."
            );

            return;
        }

        Color color =
            sideImage.color;

        color.a = alpha;

        sideImage.color =
            color;
    }

    // =========================================================
    // HOVER
    // =========================================================

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (!isAvailable)
            return;

        isHovered = true;

        Debug.Log(
            $"Mouse ENTER side {direction}"
        );

        RefreshVisual();
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        isHovered = false;

        Debug.Log(
            $"Mouse EXIT side {direction}"
        );

        RefreshVisual();
    }

    // =========================================================
    // CLICK
    // =========================================================

    private void OnClicked()
    {
        if (!isAvailable)
            return;

        if (owner == null)
        {
            Debug.LogWarning(
                $"{name}: Owner null."
            );

            return;
        }

        if (targetCell == null)
        {
            Debug.LogWarning(
                $"{name}: Target Cell null."
            );

            return;
        }

        Debug.Log(
            $"Attach clicked: " +
            $"{owner.name} -> " +
            $"{targetCell.name}"
        );

        owner.RequestAttach(
            direction,
            targetCell
        );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsAvailable()
    {
        return isAvailable;
    }

    public BodyCell GetTargetCell()
    {
        return targetCell;
    }
}