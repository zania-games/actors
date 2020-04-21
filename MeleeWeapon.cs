using System.Collections;
using UnityEngine;

namespace ProjectZombie
{
    public class MeleeWeapon: MonoBehaviour, IWeapon
    {
        #pragma warning disable 0649
        [SerializeField] int attackDamage;
        [SerializeField] float maxAttackDistance;
        [SerializeField] float secondsBetweenAttacks;
        #pragma warning restore 0649

        RaycastHit hitInfo;
        Stopwatch stopwatch;
        float? timeOfLastAttack = null;

        void Awake()
        {
            stopwatch = GameObject.FindWithTag("Globals").GetComponent<Stopwatch>();
        }

        public int Damage => attackDamage;

        public bool CanAttack
        {
            get => timeOfLastAttack == null || stopwatch.ElapsedSeconds - timeOfLastAttack >= secondsBetweenAttacks;
        }

        public IEnumerator Attack()
        {
            if (!CanAttack)
                yield break;
            // Unity won't let me use a labelled optional argument here!
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxAttackDistance))
            {
                Actor target = hitInfo.collider.gameObject.GetComponent<Actor>();
                if (target != null)
                    target.OnAttacked(this);
            }
            timeOfLastAttack = stopwatch.ElapsedSeconds;
            yield break;
        }
    }
}
