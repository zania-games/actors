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

        Actions currentActions = Actions.None;

        public override Actions SupportedActions => Actions.Movement;
        public override Actions CurrentActions => currentActions;

        public override void OnActionBegin(Actions action)
        {
            switch (action)
            {
                case Actions.SlowMove:
                case Actions.NormalMove:
                case Actions.FastMove:
                    currentActions &= ~Actions.Movement;
                    break;
                case Actions.Jump:
                case Actions.Turn:
                    break;
                default:
                    OnActionNotSupported(action);
                    break;
            }
            currentActions |= action;
        }

        public override void OnActionEnd(Actions action)
        {
            if ((action & SupportedActions) == 0)
                OnActionNotSupported(action);
            currentActions &= ~action;
        }

        public override void SlowMove(Vector3 direction) => ImplMove(direction, slowSpeed);
        public override void NormalMove(Vector3 direction) => ImplMove(direction, normalSpeed);
        public override void FastMove(Vector3 direction) => ImplMove(direction, fastSpeed);
        public override IEnumerator Jump() => ImplJump(jumpSpeed);
        public override void Turn(float angularVelocity) => ImplTurn(turnSpeed * angularVelocity);
    }
}
