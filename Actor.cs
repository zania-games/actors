using System;
using UnityEngine;

namespace ProjectZombie
{
    public abstract class Actor: MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] protected int hitPoints;
        #pragma warning restore 0649

        ActorController controller;

        protected ActorController Controller => controller;

        protected virtual void Awake()
        {
            controller = GetComponent<ActorController>();
        }

        protected virtual int ResistanceTo(IWeapon weapon) => 0;

        public int HitPoints => hitPoints;
        public Actions CurrentActions {get; protected set;} = Actions.None;

        public abstract void OnDeath();

        public virtual void OnActionBegin(Actions action)
        {
            if ((action & controller.SupportedActions) == 0)
                throw new ArgumentException($"Action `{action}` is not supported.", "action");
            CurrentActions |= action;
        }

        public virtual void OnActionEnd(Actions action)
        {
            if ((action & controller.SupportedActions) == 0)
                throw new ArgumentException($"Action `{action}` is not supported.", "action");
            CurrentActions &= ~action;
        }

        public virtual void OnAttacked(IWeapon weapon)
        {
            hitPoints -= (weapon.Damage - ResistanceTo(weapon));
        }
    }
}
