using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie
{
    public abstract class AIController: ActorController
    {
        const float SecondsBetweenPathFinds = 0.01f;
        const float SecondsBeforeResumeFollow = 0.25f;
        const float DefaultDistanceFromTarget = 0;
        const float DefaultMaxError = 2;

        static float PlanarDistance(Vector3 u, Vector3 v)
        {
            float x = u.x - v.x, z = u.z - v.z;
            return Mathf.Sqrt(x*x + z*z);
        }

        static float PlanarAngle(Vector3 u, Vector3 v)
        {
            return Mathf.Rad2Deg * Mathf.Atan2(v.x*u.z - u.x*v.z, u.x*v.x + u.z*v.z);
        }

        static bool DefaultBreakCondition() => false;

        Vector3? FindPointNear(Vector3 v, float distance, float maxError)
        {
            float distanceFromSelf = Vector3.Distance(transform.position, v);
            Vector3 optimalPoint = Vector3.MoveTowards(transform.position, v, distanceFromSelf - distance);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(optimalPoint, out hit, maxError, NavMesh.AllAreas))
                return hit.position;
            else
                return null;
        }

        [SmartCoroutineEnabled]
        IEnumerator BlindMoveTo(Vector3 destination, Actions moveType)
        {
            MoveMethod mover = GetMoveMethod(moveType);
            float distanceBefore = PlanarDistance(transform.position, destination);
            if (distanceBefore < Mathf.Epsilon)
                yield break;
            yield return TurnDegrees(PlanarAngle(transform.forward, destination - transform.position));
            actor.OnActionBegin(moveType);
            while (true)
            {
                mover(Vector3.forward);
                yield return null;
                if (!IsMoving)
                    yield return SmartCoroutine.Exit;
                float distanceAfter = PlanarDistance(transform.position, destination);
                if (distanceAfter >= distanceBefore)
                    break;
                distanceBefore = distanceAfter;
            }
            mover(Vector3.back);
            actor.OnActionEnd(moveType);
        }

        [SmartCoroutineEnabled]
        IEnumerator ImplMoveTo(Func<Vector3?> targetFinder, Actions moveType, float waypointDistance,
            float searchRadius, Func<bool> breakCondition)
        {
            NavMeshHit hit;
            NavMeshPath path = new NavMeshPath();
            Vector3? destination = targetFinder();
            if (destination == null)
                yield return SmartCoroutine.Exit;
            float distance = PlanarDistance(transform.position, (Vector3)destination);
            for (; distance > waypointDistance; distance = PlanarDistance(transform.position, (Vector3)destination))
            {
                if (breakCondition())
                    yield break;
                Vector3 optimalWaypoint =
                    Vector3.MoveTowards(transform.position, (Vector3)destination, waypointDistance);
                if (!NavMesh.SamplePosition(optimalWaypoint, out hit, searchRadius, NavMesh.AllAreas))
                    yield return SmartCoroutine.Exit;
                if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
                    yield return SmartCoroutine.Exit;
                for (int i = 1; i < path.corners.Length; ++i)
                    yield return BlindMoveTo(path.corners[i], moveType);
                yield return new WaitForSeconds(SecondsBetweenPathFinds);
                destination = targetFinder();
                if (destination == null)
                    yield return SmartCoroutine.Exit;
            }
            NavMesh.CalculatePath(transform.position, (Vector3)destination, NavMesh.AllAreas, path);
            if (breakCondition())
                yield break;
            else if (path.status == NavMeshPathStatus.PathComplete)
                yield return BlindMoveTo((Vector3)destination, moveType);
            else
                yield return SmartCoroutine.Exit;
        }

        protected abstract float TurnSpeed {get;}
        public bool IsMoving => charController.velocity.sqrMagnitude >= Mathf.Epsilon;

        public IEnumerator TurnDegrees(float theta)
        {
            actor.OnActionBegin(Actions.Turn);
            float sign = Mathf.Sign(theta);
            float turns = Mathf.Abs(theta) / TurnSpeed;
            int completeTurns = (int)turns;
            for (int i = 0; i < completeTurns; ++i)
            {
                Turn(sign);
                yield return null;
            }
            Turn(sign * (turns - completeTurns));
            actor.OnActionEnd(Actions.Turn);
        }

        public IEnumerator TurnInPlace(IEnumerable<float> steps, float delayBetween, Func<bool> breakCondition)
        {
            IEnumerator<float> iter = steps.GetEnumerator();
            if (breakCondition() || !iter.MoveNext())
                yield break;
            yield return TurnDegrees(iter.Current);
            while (!breakCondition() && iter.MoveNext())
            {
                yield return new WaitForSeconds(delayBetween);
                yield return TurnDegrees(iter.Current);
            }
        }

        public IEnumerator TurnInPlace(IEnumerable<float> steps, float delayBetween)
        {
            return TurnInPlace(steps, delayBetween, DefaultBreakCondition);
        }

        [SmartCoroutineEnabled]
        public IEnumerator MoveTo(Vector3 destination, Actions moveType, float waypointDistance, float searchRadius,
            Func<bool> breakCondition)
        {
            return ImplMoveTo(() => destination, moveType, waypointDistance, searchRadius, breakCondition);
        }

        [SmartCoroutineEnabled]
        public IEnumerator MoveTo(Vector3 destination, Actions moveType, float waypointDistance, float searchRadius)
        {
            return MoveTo(destination, moveType, waypointDistance, searchRadius, DefaultBreakCondition);
        }

        [SmartCoroutineEnabled]
        public IEnumerator Approach(Transform target, Actions moveType, float waypointDistance, float searchRadius,
            float distanceFromTarget = DefaultDistanceFromTarget, float maxError = DefaultMaxError,
            Func<bool> breakCondition = null)
        {
            if (breakCondition == null)
                breakCondition = DefaultBreakCondition;
            Func<Vector3?> f = () => FindPointNear(target.position, distanceFromTarget, maxError);
            return ImplMoveTo(f, moveType, waypointDistance, searchRadius, breakCondition);
        }

        public IEnumerator Follow(Transform target, Actions moveType, float waypointDistance, float searchRadius,
            float distanceFromTarget = DefaultDistanceFromTarget, float maxError = DefaultMaxError,
            Func<bool> breakCondition = null, Action onReach = null)
        {
            if (breakCondition == null)
                breakCondition = DefaultBreakCondition;
            if (onReach == null)
                onReach = () => {};
            Func<bool> predicate =
                () => PlanarDistance(transform.position, target.position) > distanceFromTarget + maxError;
            while (!breakCondition())
            {
                IEnumerator approacher = Approach(target, moveType, waypointDistance, searchRadius, distanceFromTarget,
                    maxError);
                yield return SmartCoroutine.Create(approacher, onCompletion: onReach);
                yield return new WaitForSeconds(SecondsBeforeResumeFollow);
                yield return new WaitUntil(predicate);
            }
        }
    }
}
