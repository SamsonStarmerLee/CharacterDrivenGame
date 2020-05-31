using Assets.Scripts.Notifications;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableUI : MonoBehaviour, IPointerClickHandler
{
    public const string ClickedNotification = "ClickableUI.ClickedNotification";

    public void OnPointerClick(PointerEventData eventData)
    {
        this.PostNotification(ClickedNotification, eventData);
    }
}