using UnityEngine;

namespace Assets.Scripts.Characters
{
    [SelectionBase]
    public partial class AWarrior : Entity, ICharacter, IScorer
    {
        [SerializeField]
        float moveTime = 0.25f;

        [SerializeField]
        int throwRange = 3;

        [SerializeField]
        float throwTime = 0.25f;

        [SerializeField]
        int _movementRange = 6;

        StateMachine machine = new StateMachine();

        public int MovementRange => _movementRange;

        public override void Init()
        {
            base.Init();

            Board.Instance.RegisterScorer(this);

            machine.ChangeState(new IdleState
            {
                Owner = this
            });
        }

        public override void Destroy()
        {
            base.Destroy();

            Board.Instance.DeregisterScorer(this);
        }

        void Update()
        {
            machine.Execute();
        }
    }
}
