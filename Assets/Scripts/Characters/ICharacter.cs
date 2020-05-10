namespace Assets.Scripts.Characters
{
    public interface ICharacter : IOccupant
    {
        int MovementRange { get; }

        void Tick();
    }
}