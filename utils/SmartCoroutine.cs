using System;
using System.Collections;
using System.Collections.Generic;

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

        public static SmartCoroutine Create(IEnumerator coroutine, Action onExit = null, Action onCompletion = null)
        {
            SmartCoroutine smartCoroutine = coroutine as SmartCoroutine;
            if (smartCoroutine == null)
                return new SmartCoroutine(coroutine, onExit, onCompletion);
            else
            {
                if (onExit != null)
                    smartCoroutine.exitCallbacks.Add(onExit);
                if (onCompletion != null)
                    smartCoroutine.completionCallbacks.Add(onCompletion);
                return smartCoroutine;
            }
        }

        IEnumerator wrapped;
        IList<Action> exitCallbacks = new List<Action>();
        IList<Action> completionCallbacks = new List<Action>();

        SmartCoroutine(IEnumerator coroutine, Action onExit, Action onCompletion)
        {
            wrapped = coroutine;
            if (onExit != null)
                exitCallbacks.Add(onExit);
            if (onCompletion != null)
                completionCallbacks.Add(onCompletion);
        }

        public object Current => wrapped.Current;
        public Result Status {get; private set;} = Result.Pending;
        public bool IsFinished => Status != Result.Pending;

        public bool MoveNext()
        {
            if (wrapped.MoveNext())
            {
                if (wrapped.Current == Exit)
                {
                    Status = Result.WasExited;
                    foreach (Action cb in exitCallbacks)
                        cb();
                    return false;
                }
                else
                    return true;
            }
            else
            {
                Status = Result.Complete;
                foreach (Action cb in completionCallbacks)
                    cb();
                return false;
            }
        }

        public void Reset() => wrapped.Reset();
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SmartCoroutineEnabledAttribute: Attribute {}
}
