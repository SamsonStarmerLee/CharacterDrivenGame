using Assets.Scripts.Characters;

namespace Assets.Scripts.Actions
{
    public class DamagePlayerAction
    {
        public int Damage;
        public ICharacter Damaged;

        public DamagePlayerAction(int damage, ICharacter damaged)
        {
            Damage = damage;
            Damaged = damaged;
        }
    }
}
