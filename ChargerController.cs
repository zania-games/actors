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
        public override Actions SupportedActions => Actions.NormalMove | Actions.FastMove | Actions.Turn;

        public override void NormalMove(Vector3 direction) => ImplMove(direction, moveSpeed);
        public override void FastMove(Vector3 direction) => ImplMove(direction, chargeSpeed);
        public override void Turn(float angularVelocity) => ImplTurn(turnSpeed * angularVelocity);
    }
}
