using Assets.Scripts.Notifications;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // TODO: Add object-specific observable subscriptions.
    public const string ClickedNotification = "ClickableUI.ClickedNotification";

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

        this.PostNotification(ClickedNotification, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = hovered;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = @default;
    }
}