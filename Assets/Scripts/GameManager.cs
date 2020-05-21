using Assets.Scripts.Actions;
using Assets.Scripts.Notifications;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public const string ScoreChangedNotification = "ScoreChanged.Notification";
        public const string HealthChangedNotification = "HealthChanged.Notification";

        public int Health { get; private set; }

        public int Score { get; private set; }

        private void OnEnable()
        {
            this.AddObserver(OnAttack, Notify.Action<DamagePlayerAction>());
            this.AddObserver(OnScore, Notify.Action<ScoreAction>());
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnAttack, Notify.Action<DamagePlayerAction>());
            this.RemoveObserver(OnScore, Notify.Action<ScoreAction>());
        }

        private void OnAttack(object sender, object args)
        {
            var action = args as DamagePlayerAction;
            Health -= action.Damage;

            this.PostNotification(HealthChangedNotification, this);
        }

        private void OnScore(object sender, object args)
        {
            var action = args as ScoreAction;
            Score += action.ScoreChange;

            Debug.Log($"Score {action.ScoreChange}! Total score is now {Score}.");

            this.PostNotification(ScoreChangedNotification, this);
        }
    }
}
