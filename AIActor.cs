using UnityEngine;

namespace ProjectZombie
{
    public abstract class AIActor: Actor
    {
        RaycastHit hitInfo;

        protected bool IsVisible(Transform target, float maxDistance, float maxAngle)
        {
            Vector3 direction = target.position - transform.position;
            return Vector3.Angle(transform.forward, direction) <= maxAngle &&
                   Physics.Raycast(transform.position, direction, out hitInfo, maxDistance) &&
                   hitInfo.collider.gameObject == target.gameObject;
        }
    }
}
