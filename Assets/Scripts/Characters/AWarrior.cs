using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    [SelectionBase]
    public partial class AWarrior : Entity, ICharacter, IScorer
    {
        [SerializeField, Min(0f)]
        float moveTime = 0.25f;

        [SerializeField, Min(1)]
        int throwRange = 3;

        [SerializeField, Min(0f)]
        float throwTime = 0.25f;

        [SerializeField, Min(0)]
        int _movementRange = 6;

        StateMachine machine = new StateMachine();

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
