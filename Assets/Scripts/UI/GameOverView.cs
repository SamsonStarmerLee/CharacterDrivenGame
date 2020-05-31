using Assets.Scripts;
using Assets.Scripts.Notifications;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverView : MonoBehaviour
{
    [SerializeField]
    private GameObject retryButton;

    [SerializeField]
    private GameObject exitButton;

    private bool active = true;

    private void OnEnable()
    {
        this.AddObserver(OnGameOver, GameManager.GameOverNotification);
        this.AddObserver(OnUIClick, ClickableUI.ClickedNotification);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnGameOver, GameManager.GameOverNotification);
        this.RemoveObserver(OnUIClick, ClickableUI.ClickedNotification);
    }

    private void OnGameOver(object sender, object args)
    {
        StartCoroutine(ShowGameOverOptions());
    }

    private IEnumerator ShowGameOverOptions()
    {
        yield return null;

        active = true;
    }

    private void OnUIClick(object sender, object args)
    {
        if (!active)
        {
            return;
        }

        var ped = args as PointerEventData;

        switch (ped.pointerPress)
        {
            case var _ when ped.pointerPress == retryButton:
                this.PostNotification(GameManager.RestartNotification);
                break;
            case var _ when ped.pointerPress == exitButton:
                this.PostNotification(GameManager.ExitGameNotification);
                break;
        }

        active = false;
    }
}