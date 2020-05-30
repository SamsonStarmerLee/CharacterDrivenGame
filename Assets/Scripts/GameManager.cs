using Assets.Scripts.Actions;
using Assets.Scripts.Characters;
using Assets.Scripts.Notifications;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public const string ScoreChangedNotification = "ScoreChanged.Notification";
        public const string HealthChangedNotification = "HealthChanged.Notification";
        public const string GameOverNotification = "GameOver.Notification";
        public const string SubmitTurnNotification = "SubmitTurn.Notification";
        public const string OpenExitNotification = "OpenExit.Notification";

        public const int MaxHealth = 3;

        public int Health { get; private set; }

        public int Score { get; private set; }

        public int DoorCountdown { get; private set; } = 10;

        private void Awake()
        {
            Health = MaxHealth;
        }

        private void OnEnable()
        {
            this.AddObserver(OnAttack, Notify.Action<DamagePlayerAction>());
            this.AddObserver(OnScore, Notify.Action<ScoreAction>());
            this.AddObserver(OnSubmitTurn, SubmitTurnNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnAttack, Notify.Action<DamagePlayerAction>());
            this.RemoveObserver(OnScore, Notify.Action<ScoreAction>());
            this.RemoveObserver(OnSubmitTurn, SubmitTurnNotification);
        }

        private void OnAttack(object sender, object args)
        {
            var action = args as DamagePlayerAction;
            
            Health -= action.Damage;
            this.PostNotification(HealthChangedNotification, this);

            if (Health == 0)
            {
                Debug.Log("YOU DIED!");
                
                StartCoroutine(KillCharacter(action));

                this.PostNotification(GameOverNotification, action.Damaged);
            }
        }

        private static IEnumerator KillCharacter(DamagePlayerAction action)
        {
            var character = action.Damaged as ICharacter;

            // Shake damaged character
            var shaker = (character as MonoBehaviour).GetComponent<Shaker>();
            shaker.AddTrauma(1f);

            yield return new WaitForSeconds(2f);

            character.Destroy();
        }

        private void OnScore(object sender, object args)
        {
            var action = args as ScoreAction;
            Score += action.ScoreChange;

            Debug.Log($"Score {action.ScoreChange}! Total score is now {Score}.");

            this.PostNotification(ScoreChangedNotification, this);
        }

        private void OnSubmitTurn(object sender, object args)
        {
            DoorCountdown--;

            if (DoorCountdown == 0)
            {
                this.PostNotification(OpenExitNotification);
            }
            else if (DoorCountdown > 0)
            {
                Debug.Log($"Turns until door opens: {DoorCountdown}");
            }
        }
    }
}
