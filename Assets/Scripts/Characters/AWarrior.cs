using Assets.Scripts.Pathfinding;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    [SelectionBase]
    public partial class AWarrior : Entity, ICharacter, IScorer
    {
        [SerializeField]
        private Mesh[] meshAlphabet;

        [SerializeField, Min(0f)]
        private float moveTime = 0.25f;

        [SerializeField, Min(0)]
        private int _movementRange = 6;

        public int MovementRange => _movementRange;

        public bool HasActed { get; set; }

        public IMovementCallbacks MovementCallbacks { get; } = new LetterMovementCallbacks();

        public void SetLetter(char letter)
        {
            _letter = letter.ToString();
            name = _letter;

            // TODO: Replace this with a scriptableobject or something
            var mesh = meshAlphabet.First(x => x.name == $"{letter}_Upper");
            var mf = GetComponentInChildren<MeshFilter>();
            mf.sharedMesh = mesh;
        }

        public override void Destroy()
        {
            BlowUp();
        }
    }
}
