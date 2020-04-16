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

        protected virtual void Awake()
        {
            charController = GetComponent<CharacterController>();
        }

        void OnActionNotSupported(Actions action) => throw new ArgumentException("Action not supported.", "action");

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
            OnActionBegin(Actions.Jump);
            Vector3 velocity = charController.velocity + speed * Vector3.up;
            do
            {
                charController.Move(Time.fixedDeltaTime * velocity);
                velocity += DefaultGravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
                yield return null;
            }
            while (!charController.isGrounded);
            OnActionEnd(Actions.Jump);
        }

        protected void ImplTurn(float angularVelocity)
        {
            transform.Rotate(0, angularVelocity, 0);
        }

        public abstract Actions SupportedActions {get;}
        public Actions CurrentActions {get; protected set;} = Actions.None;
        public bool SetupComplete {get; protected set;} = false;

        public virtual void OnActionBegin(Actions action)
        {
            if ((action & SupportedActions) == 0)
                OnActionNotSupported(action);
            CurrentActions |= action;
        }

        public virtual void OnActionEnd(Actions action)
        {
            if ((action & SupportedActions) == 0)
                OnActionNotSupported(action);
            CurrentActions &= ~action;
        }

        public virtual void SlowMove(Vector3 direction) => throw new NotImplementedException();
        public virtual void NormalMove(Vector3 direction) => throw new NotImplementedException();
        public virtual void FastMove(Vector3 direction) => throw new NotImplementedException();
        public virtual IEnumerator Jump() => throw new NotImplementedException();
        public virtual void Turn(float angularVelocity) => throw new NotImplementedException();

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
                    throw new ArgumentException("Invalid move type.", "moveType");
            }
        }
    }
}
