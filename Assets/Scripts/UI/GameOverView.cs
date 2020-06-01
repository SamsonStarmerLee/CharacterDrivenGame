using Assets.Scripts;
using Assets.Scripts.Notifications;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverView : MonoBehaviour
{
    [SerializeField]
    private Image gameOverLogo;

    [SerializeField]
    private Image retryButton;

    [SerializeField]
    private Image quitButton;

    [SerializeField]
    private float logoRevealDelay, optionRevealDelay, fadeInTime;

    private bool active;

    private void Start()
    {
        gameOverLogo.color = new Color(1, 1, 1, 0);
        retryButton.color = new Color(1, 1, 1, 0);
        quitButton.color = new Color(1, 1, 1, 0);
    }

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
        var fadeIn = DOTween.Sequence()
            .Insert(logoRevealDelay, gameOverLogo.DOColor(Color.white, fadeInTime))
            .Insert(optionRevealDelay, quitButton.DOColor(Color.white, fadeInTime))
            .Insert(optionRevealDelay, retryButton.DOColor(Color.white, fadeInTime));

        if (!fadeIn.IsComplete())
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
            case var _ when ped.pointerPress == retryButton.gameObject:
                this.PostNotification(GameManager.RestartNotification);
                break;
            case var _ when ped.pointerPress == quitButton.gameObject:
                this.PostNotification(GameManager.ExitGameNotification);
                break;
        }

        active = false;
    }
}