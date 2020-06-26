using Assets.Scripts.Notifications;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // TODO: Add object-specific observable subscriptions.
    public const string ClickableClickNotification = "ClickableUI.ClickableClickNotification";
    public const string ClickableEnterNotification = "ClickableUI.ClickableEnterNotification";
    public const string ClickableExitNotification = "ClickableUI.ClickableExitNotification";

    [SerializeField]
    private Sprite @default, hovered, clicked;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        image.sprite = clicked;
        this.PostNotification(ClickableClickNotification, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = hovered;
        this.PostNotification(ClickableEnterNotification, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = @default;
        this.PostNotification(ClickableExitNotification, eventData);
    }
}