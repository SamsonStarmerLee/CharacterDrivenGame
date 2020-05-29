using Assets.Scripts.Characters;
using Assets.Scripts.InputManagement;
using Assets.Scripts.Visuals;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public partial class CharacterController : MonoBehaviour
    {
        [SerializeField]
        InputSource input;

        [SerializeField]
        private LayerMask movementLayerMask;

        private StateMachine machine = new StateMachine();
        private List<DragMovement> movement = new List<DragMovement>();
        private ICharacter activeCharacter;

        private Dictionary<ICharacter, List<Vector2Int>> paths = new Dictionary<ICharacter, List<Vector2Int>>();
        private HashSet<Vector2Int> inRangeTiles = new HashSet<Vector2Int>();

        private void Awake()
        {
            machine.ChangeState(new IdleState { Owner = this });
        }

        private void Update()
        {
            input.Prime();
            machine.Execute();
            activeCharacter?.Tick();
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