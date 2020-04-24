using System.Collections;
using UnityEngine;

namespace ProjectZombie
{
    [RequireComponent(typeof(AudioSource))]
    public class MeleeWeapon: MonoBehaviour, IWeapon
    {
        #pragma warning disable 0649
        [SerializeField] int attackDamage;
        [SerializeField] float maxAttackDistance;
        [SerializeField] float secondsBetweenAttacks;
        [SerializeField] AudioClip attackSound = null;
        #pragma warning restore 0649

        RaycastHit hitInfo;
        Stopwatch stopwatch;
        AudioSource audioSource;
        float? timeOfLastAttack = null;

        void Awake()
        {
            stopwatch = GameObject.FindWithTag("Globals").GetComponent<Stopwatch>();
            audioSource = GetComponent<AudioSource>();
        }

        public int Damage => attackDamage;

        public bool CanAttack
        {
            get => timeOfLastAttack == null || stopwatch.ElapsedSeconds - timeOfLastAttack >= secondsBetweenAttacks;
        }

        [SmartCoroutineEnabled]
        public IEnumerator Attack()
        {
            if (!CanAttack)
                yield return SmartCoroutine.Exit;
            // Unity won't let me use a labelled optional argument here!
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxAttackDistance))
            {
                Actor target = hitInfo.collider.gameObject.GetComponent<Actor>();
                if (target != null)
                {
                    target.OnAttacked(this);
                    if (attackSound != null)
                        audioSource.PlayOneShot(attackSound);
                }
                else
                    yield return SmartCoroutine.Exit;
            }
            else
                yield return SmartCoroutine.Exit;
            timeOfLastAttack = stopwatch.ElapsedSeconds;
            yield break;
        }
    }
}
