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
        Action onSuccess;
        Action onFailure;

        public SmartCoroutine(IEnumerator coroutine, Action onFailure = null, Action onSuccess = null)
        {
            wrapped = coroutine;
            this.onSuccess = onSuccess != null ? onSuccess : () => {};
            this.onFailure = onFailure != null ? onFailure : () => {};
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
