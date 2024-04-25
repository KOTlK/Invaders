using UnityEngine;
using static Pools;
using static UnityEngine.Mathf;

public class Entity : MonoBehaviour
{
    public int        Id;
    public EntityType Type;
    
    public void Destroy()
    {
        Destroy(gameObject);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if(Type == EntityType.Missile)
        {
            ref var rocket     = ref MissilePool.Get(Id);
            ref var eTransform = ref TransformPool.Get(Id);
            
            var length      = rocket.searchRadius;
            var orientation = eTransform.orientation;
            var left        = (orientation - rocket.fov) * Deg2Rad;
            var right       = (orientation + rocket.fov) * Deg2Rad;
            var leftVec     = new Vector3(Cos(left), Sin(left), 0f).normalized * length;
            var rightVec    = new Vector3(Cos(right), Sin(right), 0f).normalized * length;
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(eTransform.position, new Vector3(Cos(orientation * Deg2Rad), Sin(orientation * Deg2Rad), 0f));
            Gizmos.color = Color.green;
            Gizmos.DrawRay(eTransform.position, leftVec);
            Gizmos.DrawRay(eTransform.position, rightVec);
        }
    }
    #endif
}
