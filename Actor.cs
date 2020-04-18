using UnityEngine;

namespace ProjectZombie
{
    public abstract class Actor: MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] protected int hitPoints;
        #pragma warning restore 0649

        public int HitPoints => hitPoints;
    }
}
