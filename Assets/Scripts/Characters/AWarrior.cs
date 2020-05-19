using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    [SelectionBase]
    public partial class AWarrior : Entity, ICharacter, IScorer
    {
        [SerializeField, Min(0f)]
        private float moveTime = 0.25f;

        [SerializeField, Min(1)]
        private int throwRange = 3;

        [SerializeField, Min(0f)]
        private float throwTime = 0.25f;

        [SerializeField, Min(0)]
        private int _movementRange = 6;
        private StateMachine machine = new StateMachine();

        public int MovementRange => _movementRange;

        public bool HasActed { get; set; }

        public IMovementCallbacks MovementCallbacks { get; } = new LetterMovementCallbacks();

        public override void Init()
        {
            base.Init();

            machine.ChangeState(new IdleState
            {
                Owner = this
            });
        }

        public void Tick()
        {
            machine.Execute();
        }
    }
}
