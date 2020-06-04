using Assets.Scripts.Actions;
using Assets.Scripts.Characters;
using Assets.Scripts.LevelGen;
using Assets.Scripts.Notifications;
using Assets.Scripts.Pooling;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public const string ScoreChangedNotification = "ScoreChanged.Notification";
        public const string HealthChangedNotification = "HealthChanged.Notification";
        public const string FloorChangedNotification = "FloorChanged.Notification";
        public const string SubmitTurnNotification = "SubmitTurn.Notification";

        public const string OpenExitNotification = "OpenExit.Notification";
        public const string ExitFloorNotification = "ExitFloor.Notification";
        public const string DoorCountdownNotification = "DoorCountdown.Notification";

        public const string GameOverNotification = "GameOver.Notification";
        public const string ExitGameNotification = "ExitGame.Notification";
        public const string RestartNotification = "Restart.Notification";

        public const int MaxHealth = 3;

        [SerializeField]
        RoomGenerator roomGenerator;

        [SerializeField]
        Pooler pooler;

        public int Health { get; private set; }

        public int Score { get; private set; }

        public int DoorCountdown { get; private set; } = 9;

        public int Floor { get; private set; } = 1;

        private void Awake()
        {
            Health = MaxHealth;
        }

        private void OnEnable()
        {
            this.AddObserver(OnAttack, Notify.Action<DamagePlayerAction>());
            this.AddObserver(OnScore, Notify.Action<ScoreAction>());
            this.AddObserver(OnSubmitTurn, SubmitTurnNotification);
            this.AddObserver(OnExitFloor, ExitFloorNotification);
            this.AddObserver(OnRestart, RestartNotification);
            this.AddObserver(OnExitGame, ExitGameNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnAttack, Notify.Action<DamagePlayerAction>());
            this.RemoveObserver(OnScore, Notify.Action<ScoreAction>());
            this.RemoveObserver(OnSubmitTurn, SubmitTurnNotification);
            this.RemoveObserver(OnExitFloor, ExitFloorNotification);
            this.RemoveObserver(OnRestart, RestartNotification);
            this.RemoveObserver(OnExitGame, ExitGameNotification);
        }

        private void Start()
        {
            // Clear 'static' data that may be left over from previous plays.
            Board.Instance.ClearFloorReferences();
            pooler.Clear();

            roomGenerator.Generate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                roomGenerator.Generate();
            }
        }

        private void OnAttack(object sender, object args)
        {
            var action = args as DamagePlayerAction;
            
            Health -= action.Damage;
            this.PostNotification(HealthChangedNotification, this);

            if (Health == 0)
            {
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
            this.PostNotification(ScoreChangedNotification, this);
        }

        private void OnSubmitTurn(object sender, object args)
        {
            DoorCountdown--;

            if (DoorCountdown >= 0)
            {
                this.PostNotification(DoorCountdownNotification, this);
            }

            if (DoorCountdown == 0)
            {
                this.PostNotification(OpenExitNotification);
            }
        }

        private void OnExitFloor(object sender, object args)
        {
            StartCoroutine(GenerateNewFloor());
        }

        private IEnumerator GenerateNewFloor()
        {
            roomGenerator.Generate();

            yield return new WaitForSeconds(1f);

            // Notify about the new floor
            Floor++;
            this.PostNotification(FloorChangedNotification, this);

            // Heal 1 health
            Health = Mathf.Clamp(Health + 1, 0, MaxHealth);
            this.PostNotification(HealthChangedNotification, this);

            // 'Refill' door countdown
            DoorCountdown = 9;
            this.PostNotification(DoorCountdownNotification, this);

            roomGenerator.Generate();
        }

        private void OnRestart(object sender, object args)
        {
            SceneManager.LoadScene(0);
        }

        private void OnExitGame(object sender, object args)
        {
            // TODO: Return to main menu
            Application.Quit();
        }
    }
}
