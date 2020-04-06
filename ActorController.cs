using System;
using System.Collections;
using UnityEngine;

namespace ProjectZombie
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class ActorController: MonoBehaviour
    {
        protected const float DefaultGravityMultiplier = 2;

        protected CharacterController charController;

        protected virtual void Awake()
        {
            charController = GetComponent<CharacterController>();
        }

        protected void ImplMove(Vector3 direction, float speed)
        {
            Vector3 offset = speed * Time.deltaTime * transform.TransformDirection(direction);
            charController.Move(offset + new Vector3(0, -0.001f, 0));
        }

        protected IEnumerator ImplJump(float speed)
        {
            OnActionBegin(Actions.Jump);
            Vector3 velocity = charController.velocity + speed * Vector3.up;
            do
            {
                charController.Move(Time.fixedDeltaTime * velocity);
                velocity += DefaultGravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
                yield return new WaitForFixedUpdate();
            }
            while (!charController.isGrounded);
            OnActionEnd(Actions.Jump);
        }

        protected void ImplTurn(float angularVelocity)
        {
            transform.Rotate(0, angularVelocity, 0);
        }

        protected void OnActionNotSupported(Actions action)
        {
            throw new ArgumentException("Action not supported.", "action");
        }

        public abstract Actions SupportedActions {get;}
        public abstract Actions CurrentActions {get;}

        public abstract void OnActionBegin(Actions action);
        public abstract void OnActionEnd(Actions action);

        public virtual void SlowMove(Vector3 direction) => throw new NotImplementedException();
        public virtual void NormalMove(Vector3 direction) => throw new NotImplementedException();
        public virtual void FastMove(Vector3 direction) => throw new NotImplementedException();
        public virtual IEnumerator Jump() => throw new NotImplementedException();
        public virtual void Turn(float angularVelocity) => throw new NotImplementedException();

        public virtual IEnumerator FallToGround()
        {
            while (!charController.isGrounded)
            {
                charController.Move(DefaultGravityMultiplier * Time.fixedDeltaTime * Physics.gravity);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
