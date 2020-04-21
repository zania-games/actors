using System.Collections;
using UnityEngine;

namespace ProjectZombie
{
    public class ChargerController: AIController
    {
        #pragma warning disable 0649
        [SerializeField] float moveSpeed;
        [SerializeField] float chargeSpeed;
        [SerializeField] float turnSpeed;
        #pragma warning restore 0649

        protected override float TurnSpeed => turnSpeed;

        public override Actions SupportedActions
        {
            get => Actions.NormalMove | Actions.FastMove | Actions.Turn | Actions.Attack;
        }

        public override void NormalMove(Vector3 direction) => ImplMove(direction, moveSpeed);
        public override void FastMove(Vector3 direction) => ImplMove(direction, chargeSpeed);
        public override void Turn(float angularVelocity) => ImplTurn(turnSpeed * angularVelocity);
        public override IEnumerator Attack(IWeapon weapon) => SmartCoroutine.Create(ImplAttack(weapon));
    }
}
