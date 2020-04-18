using System;
using System.Collections;

namespace ProjectZombie
{
    public class SmartCoroutine: IEnumerator
    {
        public static readonly object FailFlag = new object();

        public enum Result
        {
            NotComplete,
            Success,
            Failure
        }

        IEnumerator wrapped;
        Action onFailure;
        Action onSuccess;

        public SmartCoroutine(IEnumerator coroutine, Action onFailure = null, Action onSuccess = null)
        {
            wrapped = coroutine;
            this.onFailure = onFailure != null ? onFailure : () => {};
            this.onSuccess = onSuccess != null ? onSuccess : () => {};
        }

        public object Current => wrapped.Current;
        public Result Status {get; private set;} = Result.NotComplete;
        public bool IsComplete => Status != Result.NotComplete;
        public Action OnSuccess => onSuccess;
        public Action OnFailure => onFailure;

        public bool MoveNext()
        {
            if (wrapped.MoveNext())
            {
                if (wrapped.Current == FailFlag)
                {
                    Status = Result.Failure;
                    onFailure();
                    return false;
                }
                else
                    return true;
            }
            else
            {
                Status = Result.Success;
                onSuccess();
                return false;
            }
        }

        public void Reset() => wrapped.Reset();
    }
}
