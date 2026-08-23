using UnityEngine;
using UnityEngine.UI;

public class BodyCellSide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;

    private Vector2Int direction;
    private BodyCell owner;
    private BodyCell targetCell;

    public void Setup(
        BodyCell owner,
        Vector2Int direction
    )
    {
        this.owner = owner;
        this.direction = direction;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        // Default tidak aktif
        gameObject.SetActive(false);
    }

    public void SetAvailable(
        bool available,
        BodyCell target = null
    )
    {
        targetCell = target;

        gameObject.SetActive(available);
    }

    private void OnClicked()
    {
        if (owner == null)
            return;

        if (targetCell == null)
            return;

        Debug.Log(
            $"Attach clicked: " +
            $"{owner.name} -> {targetCell.name}"
        );

        owner.RequestAttach(
            direction,
            targetCell
        );
    }
}