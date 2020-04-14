using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie
{
    public abstract class AIController: ActorController
    {
        const float SecondsBetweenPathFinds = 0.01f;

        static float PlanarDistance(Vector3 u, Vector3 v)
        {
            float x = u.x - v.x, z = u.z - v.z;
            return Mathf.Sqrt(x*x + z*z);
        }

        static float PlanarAngle(Vector3 u, Vector3 v)
        {
            float num = u.x*v.x + u.z*v.z;
            float denom = Mathf.Sqrt((u.x*u.x + u.z*u.z) * (v.x*v.x + v.z*v.z));
            return Mathf.Rad2Deg * Mathf.Acos(num / denom);
        }

        NavMeshHit hit;
        NavMeshPath path;

        protected override void Awake()
        {
            base.Awake();
            path = new NavMeshPath();
        }

        IEnumerator BlindMoveTo(Vector3 destination, Actions moveType)
        {
            MoveMethod mover = GetMoveMethod(moveType);
            float distanceBefore = PlanarDistance(transform.position, destination);
            if (distanceBefore < Mathf.Epsilon)
                yield break;
            yield return TurnDegrees(PlanarAngle(transform.forward, destination - transform.position));
            OnActionBegin(moveType);
            while (true)
            {
                mover(Vector3.forward);
                yield return null;
                float distanceAfter = PlanarDistance(transform.position, destination);
                if (distanceAfter >= distanceBefore)
                    break;
                distanceBefore = distanceAfter;
            }
            mover(Vector3.back);
            OnActionEnd(moveType);
        }

        protected IEnumerator ImplTurnDegrees(float theta, float speed)
        {
            OnActionBegin(Actions.Turn);
            float sign = Mathf.Sign(theta);
            float turns = Mathf.Abs(theta) / speed;
            int completeTurns = (int)turns;
            for (int i = 0; i < completeTurns; ++i)
            {
                Turn(sign);
                yield return null;
            }
            Turn(turns - completeTurns);
            OnActionEnd(Actions.Turn);
        }

        public abstract IEnumerator TurnDegrees(float theta);

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

        // TODO: Handle situations where a path is found, but invalidated later, such as when another actor gets in the
        // way. Such situations may prevent the derived coroutine from terminating on its own.
        public IEnumerator MoveTo(Vector3 destination, Actions moveType, float waypointDistance, float searchRadius)
        {
            float distance = PlanarDistance(transform.position, destination);
            for (; distance > waypointDistance; distance = PlanarDistance(transform.position, destination))
            {
                Vector3 optimalWaypoint = Vector3.MoveTowards(transform.position, destination, waypointDistance);
                if (!NavMesh.SamplePosition(optimalWaypoint, out hit, searchRadius, NavMesh.AllAreas))
                    yield break;
                if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
                    yield break;
                for (int i = 1; i < path.corners.Length; ++i)
                    yield return BlindMoveTo(path.corners[i], moveType);
                yield return new WaitForSeconds(SecondsBetweenPathFinds);
            }
            NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if (path.status == NavMeshPathStatus.PathComplete)
                yield return BlindMoveTo(destination, moveType);
        }
    }
}
