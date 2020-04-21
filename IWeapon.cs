using System.Collections;

namespace ProjectZombie
{
    public interface IWeapon
    {
        int Damage {get;}
        bool CanAttack {get;}

        [SmartCoroutineEnabled]
        IEnumerator Attack();
    }
}
