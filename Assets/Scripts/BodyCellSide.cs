using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BodyCellSide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    public void Setup(BodyCell owner, Vector2Int direction)
    {
        this.owner = owner;
        this.direction = direction;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        SetAlpha(hiddenAlpha);
    }

    public void SetAvailable(bool available, BodyCell target = null)
    {
        // PENGAMAN: Cegah pemuatan berulang jika status tidak berubah
        if (isAvailable == available && targetCell == target) return;

        isAvailable = available;
        targetCell = target;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (!isAvailable)
        {
            SetAlpha(hiddenAlpha);
            if (button != null) button.interactable = false;
            
            // KUNCI PERBAIKAN 1: Matikan fisik penahan kursor agar tidak menutupi UI lain
            if (sideImage != null) sideImage.raycastTarget = false; 
            return;
        }

        if (button != null) button.interactable = true;
        
        // KUNCI PERBAIKAN 2: Nyalakan kembali fisik penahan kursor
        if (sideImage != null) sideImage.raycastTarget = true; 

        if (isHovered) SetAlpha(hoverAlpha);
        else SetAlpha(availableAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (sideImage == null) return;
        Color color = sideImage.color;
        color.a = alpha;
        sideImage.color = color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isAvailable) return;
        isHovered = true;
        RefreshVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RefreshVisual();
    }

    private void OnClicked()
    {
        if (!isAvailable || owner == null || targetCell == null) return;
        owner.RequestAttach(direction, targetCell);
    }

    public bool IsAvailable() => isAvailable;
    public BodyCell GetTargetCell() => targetCell;
}