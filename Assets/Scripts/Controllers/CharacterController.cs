using Assets.Scripts.Characters;
using Assets.Scripts.InputManagement;
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

        private void Awake()
        {
            machine.ChangeState(new IdleState
            {
                Owner = this
            });
        }

        private void Update()
        {
            DrawMovement();

            input.Prime();
            machine.Execute();
            activeCharacter?.Tick();
        }

        private void DrawMovement()
        {
            foreach (var move in movement)
            {
                DrawPath(move.Path, Color.grey);
            }
        }

        private static void DrawPath(IReadOnlyList<Vector2Int> path, Color color)
        {
            for (var i = 0; i < path.Count - 1; i++)
            {
                var p0 = path[i];
                var p1 = path[i + 1];
                var v0 = new Vector3(p0.x, 0f, p0.y);
                var v1 = new Vector3(p1.x, 0f, p1.y);
                Debug.DrawLine(v0, v1, color);
            }
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