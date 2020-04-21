using UnityEngine;

namespace ProjectZombie
{
    public abstract class Actor: MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] protected int hitPoints;
        #pragma warning restore 0649

        protected virtual int ResistanceTo(IWeapon weapon) => 0;

        public int HitPoints => hitPoints;

        public virtual void OnAttacked(IWeapon weapon)
        {
            hitPoints -= (weapon.Damage - ResistanceTo(weapon));
        }
    }
}
