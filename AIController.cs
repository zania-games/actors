using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie
{
    public abstract class AIController: ActorController
    {
        public IEnumerator TurnDegrees(float theta)
        {
            OnActionBegin(Actions.Turn);
            Quaternion target = Quaternion.Euler(0, theta, 0) * transform.rotation;
            float sign = Mathf.Sign(theta);
            while (Mathf.Abs(Quaternion.Angle(transform.rotation, target)) > Mathf.Epsilon)
            {
                Turn(sign);
                yield return null;
            }
            OnActionEnd(Actions.Turn);
        }

        public IEnumerator TurnInPlace(IEnumerable<float> steps, float delayBetween)
        {
            IEnumerator<float> iter = steps.GetEnumerator();
            if (!iter.MoveNext())
                yield break;
            yield return TurnDegrees(iter.Current);
            while (iter.MoveNext())
            {
                yield return new WaitForSeconds(delayBetween);
                yield return TurnDegrees(iter.Current);
            }
        }
    }
}
