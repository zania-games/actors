using System;
using System.Collections;

namespace ProjectZombie
{
    public class SmartCoroutine: IEnumerator
    {
        public static readonly object Exit = new object();

        public enum Result
        {
            Pending,
            Complete,
            WasExited
        }

        IEnumerator wrapped;
        Action onExit;
        Action onComplete;

        public SmartCoroutine(IEnumerator coroutine, Action onExit = null, Action onComplete = null)
        {
            wrapped = coroutine;
            this.onExit = onExit != null ? onExit : () => {};
            this.onComplete = onComplete != null ? onComplete : () => {};
        }

        public object Current => wrapped.Current;
        public Result Status {get; private set;} = Result.Pending;
        public bool IsFinished => Status != Result.Pending;
        public Action OnComplete => onComplete;
        public Action OnExit => onExit;

        public bool MoveNext()
        {
            if (wrapped.MoveNext())
            {
                if (wrapped.Current == Exit)
                {
                    Status = Result.WasExited;
                    onExit();
                    return false;
                }
                else
                    return true;
            }
            else
            {
                Status = Result.Complete;
                onComplete();
                return false;
            }
        }

        public void Reset() => wrapped.Reset();
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SmartCoroutineEnabledAttribute: Attribute {}
}
