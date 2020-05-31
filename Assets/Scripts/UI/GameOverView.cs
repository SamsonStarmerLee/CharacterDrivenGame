using Assets.Scripts;
using Assets.Scripts.Notifications;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text gameOverLogo;

    [SerializeField]
    private TMP_Text retryButton;

    [SerializeField]
    private TMP_Text exitButton;

    [SerializeField]
    private float logoRevealDelay, optionRevealDelay, fadeInTime;

    private bool active;

    private void Start()
    {
        gameOverLogo.alpha = 0;
        retryButton.alpha = 0;
        exitButton.alpha = 0;
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
        DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>
            FadeInText(TMP_Text text) =>
                DOTween.To(() => text.alpha, x => text.alpha = x, 1f, fadeInTime);

        var fadeIn = DOTween.Sequence()
            .Insert(logoRevealDelay, FadeInText(gameOverLogo))
            .Insert(optionRevealDelay, FadeInText(exitButton))
            .Insert(optionRevealDelay, FadeInText(retryButton));

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
            case var _ when ped.pointerPress == exitButton.gameObject:
                this.PostNotification(GameManager.ExitGameNotification);
                break;
        }

        active = false;
    }
}