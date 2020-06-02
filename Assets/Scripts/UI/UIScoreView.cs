using Assets.Scripts.Notifications;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIScoreView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text flText;

        [SerializeField]
        private TMP_Text gpText;

        private void OnEnable()
        {
            this.AddObserver(OnScoreChanged, GameManager.ScoreChangedNotification);
            this.AddObserver(OnFloorChanged, GameManager.FloorChangedNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnScoreChanged, GameManager.ScoreChangedNotification);
            this.RemoveObserver(OnFloorChanged, GameManager.FloorChangedNotification);
        }

        private void OnScoreChanged(object sender, object args)
        {
            var gm = args as GameManager;
            gpText.text = gm.Score.ToString();
        }

        private void OnFloorChanged(object sender, object args)
        {
            var gm = args as GameManager;
            flText.text = gm.Floor.ToString();
        }
    }
}
