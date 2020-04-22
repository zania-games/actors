using System.Collections;
using UnityEngine;

namespace ProjectZombie
{
    public class PlayerController: ActorController
    {
        #pragma warning disable 0649
        [SerializeField] float slowSpeed;
        [SerializeField] float normalSpeed;
        [SerializeField] float fastSpeed;
        [SerializeField] float jumpSpeed;
        [SerializeField] float turnSpeed;
        #pragma warning restore 0649

        public override Actions SupportedActions => Actions.Movement | Actions.Turn | Actions.Attack;

        public override void SlowMove(Vector3 direction) => ImplMove(direction, slowSpeed);
        public override void NormalMove(Vector3 direction) => ImplMove(direction, normalSpeed);
        public override void FastMove(Vector3 direction) => ImplMove(direction, fastSpeed);
        public override IEnumerator Jump() => ImplJump(jumpSpeed);
        public override void Turn(float angularVelocity) => ImplTurn(turnSpeed * angularVelocity);
        public override IEnumerator Attack(IWeapon weapon) => ImplAttack(weapon);
    }
}
