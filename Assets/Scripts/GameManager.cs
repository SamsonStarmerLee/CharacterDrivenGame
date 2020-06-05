using Assets.Scripts.Actions;
using Assets.Scripts.Characters;
using Assets.Scripts.LevelGen;
using Assets.Scripts.Notifications;
using Assets.Scripts.Pooling;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
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

        public const string PlayerSpawnedNotification = "PlayerSpawned.Notification";
        public const string OpenExitNotification = "OpenExit.Notification";
        public const string ExitFloorNotification = "ExitFloor.Notification";
        public const string DoorCountdownNotification = "DoorCountdown.Notification";

        public const string GameOverNotification = "GameOver.Notification";
        public const string ExitGameNotification = "ExitGame.Notification";
        public const string RestartNotification = "Restart.Notification";

        public const int MaxHealth = 3;

        [SerializeField]
        private RoomGenerator roomGenerator;

        [SerializeField]
        private Pooler pooler;

        [SerializeField]
        private Animator crossfade;

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] damageSfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip deathSfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] matchSfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] matchOverlaySfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip stairsSfx;

        private int matchPosition;
        private AudioSource audioSource;

        public int Health { get; private set; }

        public int Score { get; private set; }

        public int DoorCountdown { get; private set; } = 9;

        public int Floor { get; private set; } = 1;

        private void Awake()
        {
            Health = MaxHealth;
            audioSource = GetComponent<AudioSource>();
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

            // Spawn player characters
            {
                // TEMP
                var spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
                var spawned = new List<Transform>();

                var vowels = new[] { 'A', 'E', 'I', 'O', 'U' };
                Utility.Shuffle(vowels);

                for (var i = 0; i < 3; i++)
                {
                    var character = Instantiate(
                        playerPrefab,
                        spawnPoints[i].transform.position,
                        Quaternion.identity);
                    var aWar = character.GetComponent<AWarrior>();
                    aWar.SetLetter(vowels[i]);

                    spawned.Add(character.transform);
                }

                var tf = spawned[0];
                this.PostNotification(PlayerSpawnedNotification, tf);
            }
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

            var sfx = damageSfx[UnityEngine.Random.Range(0, damageSfx.Length)];
            audioSource.PlayOneShot(sfx, 0.8f);

            if (Health == 0)
            {
                StartCoroutine(KillCharacter(action));
                this.PostNotification(GameOverNotification, action.Damaged);
            }
        }

        private IEnumerator KillCharacter(DamagePlayerAction action)
        {
            var character = action.Damaged as ICharacter;

            // Shake damaged character
            var shaker = (character as MonoBehaviour).GetComponent<Shaker>();
            shaker.AddTrauma(1f);

            // Sfx
            audioSource.PlayOneShot(deathSfx);

            yield return new WaitForSeconds(2.5f);

            character.Destroy();
        }

        private void OnScore(object sender, object args)
        {
            var action = args as ScoreAction;

            if (!string.IsNullOrWhiteSpace(action.Word))
            {
                // Play layered sfx for matching a word, pitched up based on score.
                matchPosition = (matchPosition += UnityEngine.Random.Range(0, 3)) % matchSfx.Length;
                var sfx = matchSfx[matchPosition];
                audioSource.pitch = UnityEngine.Random.Range(0.7f, 0.9f) + (action.Word.Length * 0.25f);
                audioSource.PlayOneShot(sfx, 0.8f);

                sfx = matchOverlaySfx[UnityEngine.Random.Range(0, matchOverlaySfx.Length)];
                audioSource.PlayOneShot(sfx, 1.25f);
            }

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
            crossfade.SetTrigger("FadeOut");
            audioSource.PlayOneShot(stairsSfx);

            yield return new WaitForSeconds(1f);

            Board.Instance.ClearFloorReferences();
            roomGenerator.Generate();

            yield return new WaitForEndOfFrame();

            // Reposition player characters
            var characters = GameObject.FindObjectsOfType<AWarrior>();
            var spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

            for (var i = 0; i < characters.Length; i++)
            {
                characters[i].WorldPosition = spawnPoints[i].transform.position;
                (characters[i] as Entity).Init();
            }
            this.PostNotification(PlayerSpawnedNotification, characters[0].transform);

            // Notify about the new floor
            Floor++;
            this.PostNotification(FloorChangedNotification, this);

            // Heal 1 health
            Health = Mathf.Clamp(Health + 1, 0, MaxHealth);
            this.PostNotification(HealthChangedNotification, this);

            // 'Refill' door countdown
            DoorCountdown = 9;
            this.PostNotification(DoorCountdownNotification, this);

            crossfade.SetTrigger("FadeIn");
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
