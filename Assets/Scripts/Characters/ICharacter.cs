using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.Characters
{
    public interface ICharacter : IOccupant
    {
        int MovementRange { get; }

        bool HasActed { get; set; }

        void Tick();

        IMovementCallbacks MovementCallbacks { get; }
    }
}