using Assets.Scripts.Characters;
using Assets.Scripts.InputManagement;
using Assets.Scripts.Notifications;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Controllers
{
    public partial class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private InputSource input;

        [SerializeField]
        private LayerMask movementLayerMask;

        [SerializeField]
        private ClickableUI buyLetterButton;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] placeCharacterSfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] placeCharacterOverlaySfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] grabCharacterSfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] dragCharacterSfx;

        [SerializeField, BoxGroup("Audio")]
        private AudioClip[] submitTurnSfx;

        private StateMachine machine = new StateMachine();
        private List<DragMovement> movement = new List<DragMovement>();
        private ICharacter activeCharacter;

        private Dictionary<ICharacter, List<Vector2Int>> paths = new Dictionary<ICharacter, List<Vector2Int>>();
        private HashSet<Vector2Int> inRangeTiles = new HashSet<Vector2Int>();

        private AudioSource audioSource;

        private void Awake()
        {
            input.Unlock();
            machine.ChangeState(new IdleState(this));

            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            this.AddObserver(OnGameOver, GameManager.GameOverNotification);
            this.AddObserver(OnUIClick, ClickableUI.ClickableClickNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnGameOver, GameManager.GameOverNotification);
            this.RemoveObserver(OnUIClick, ClickableUI.ClickableClickNotification);
        }

        private void OnGameOver(object sender, object args)
        {
            input.Lock();
        }

        private void OnUIClick(object sender, object args)
        {
            var state = machine.CurrentState;

            if (args  is PointerEventData ped && 
                state is IClickableHandlerState handler)
            {
                handler.OnClick(ped);
            }
        }

        private void Update()
        {
            input.Prime();
            machine.Execute();
        }

        private void PlaySfx(AudioClip[] clips, float pitchMin, float pitchMax, float volume = 1f)
        {
            // Play drag sfx for each tile traversed
            var sfx = clips[Random.Range(0, clips.Length)];
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.PlayOneShot(sfx, volume);
        }
    }

    internal class DragMovement
    {
        public ICharacter Character;
        public List<Vector2Int> Path;

        public Vector2Int From => Path[Path.Count - 1];

        public Vector2Int To => Path[0];
    }
}