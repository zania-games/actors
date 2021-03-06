using System;
using System.Collections;
using UnityEngine;

namespace ProjectZombie
{
    public delegate void MoveMethod(Vector3 direction);

    [RequireComponent(typeof(CharacterController))]
    public abstract class ActorController: MonoBehaviour
    {
        protected const float DefaultGravityMultiplier = 2;

        protected CharacterController charController;
        protected Actor actor;

        protected virtual void Awake()
        {
            charController = GetComponent<CharacterController>();
            actor = GetComponent<Actor>();
        }

        protected virtual IEnumerator Start()
        {
            yield return FallToGround();
            SetupComplete = true;
        }

        protected void ImplMove(Vector3 direction, float speed)
        {
            Vector3 offset = speed * transform.TransformDirection(direction);
            if (!charController.isGrounded)
                offset += DefaultGravityMultiplier * Physics.gravity;
            charController.Move(Time.deltaTime * offset + new Vector3(0, -0.001f, 0));
        }

        protected IEnumerator ImplJump(float speed)
        {
            actor.OnActionBegin(Actions.Jump);
            Vector3 velocity = charController.velocity + speed * Vector3.up;
            do
            {
                charController.Move(Time.fixedDeltaTime * velocity);
                velocity += DefaultGravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
                yield return null;
            }
            while (!charController.isGrounded);
            actor.OnActionEnd(Actions.Jump);
        }

        protected void ImplTurn(float angularVelocity)
        {
            transform.Rotate(0, angularVelocity, 0);
        }

        [SmartCoroutineEnabled]
        protected IEnumerator ImplAttack(IWeapon weapon)
        {
            actor.OnActionBegin(Actions.Attack);
            SmartCoroutine weaponAttack = SmartCoroutine.Create(weapon.Attack());
            yield return weaponAttack;
            actor.OnActionEnd(Actions.Attack);
            if (weaponAttack.Status == SmartCoroutine.Result.WasExited)
                yield return SmartCoroutine.Exit;
        }

        public abstract Actions SupportedActions {get;}
        public bool SetupComplete {get; protected set;} = false;

        public virtual void SlowMove(Vector3 direction) => throw new NotSupportedException();
        public virtual void NormalMove(Vector3 direction) => throw new NotSupportedException();
        public virtual void FastMove(Vector3 direction) => throw new NotSupportedException();
        public virtual IEnumerator Jump() => throw new NotSupportedException();
        public virtual void Turn(float angularVelocity) => throw new NotSupportedException();

        [SmartCoroutineEnabled]
        public virtual IEnumerator Attack(IWeapon weapon) => throw new NotSupportedException();

        public virtual IEnumerator FallToGround()
        {
            while (!charController.isGrounded)
            {
                charController.Move(DefaultGravityMultiplier * Time.deltaTime * Physics.gravity);
                yield return null;
            }
        }

        public MoveMethod GetMoveMethod(Actions moveType)
        {
            switch (moveType)
            {
                case Actions.SlowMove:
                    return SlowMove;
                case Actions.NormalMove:
                    return NormalMove;
                case Actions.FastMove:
                    return FastMove;
                default:
                    throw new ArgumentException($"`{moveType}` is not a valid move type.", "moveType");
            }
        }
    }
}
